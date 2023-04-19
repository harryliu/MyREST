using Dapper;
using MyREST.Plugin;
using System.Data;

namespace MyREST
{
    public class Engine
    {
        private readonly IConfiguration _configuration;

        private GlobalConfig _globalConfig;
        private SystemConfig _systemConfig;
        private List<DbConfig> _dbConfigs;
        private XmlFileContainer _xmlFileContainer;
        private AppState _appState;
        private FirewallPlugin _firewallPlugin;
        private BasicAuthPlugin _basicAuthPlugin;
        private JwtAuthPlugin _jwtAuthPlugin;
        private readonly ILogger<Engine> _logger;

        public Engine(ILogger<Engine> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs,
            XmlFileContainer xmlFileContainer, AppState appState,
            FirewallPlugin firewallPlugin, BasicAuthPlugin basicAuthPlugin, JwtAuthPlugin jwtAuthPlugin)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;
            _dbConfigs = dbConfigs;
            _xmlFileContainer = xmlFileContainer;
            _appState = appState;
            _firewallPlugin = firewallPlugin;
            _basicAuthPlugin = basicAuthPlugin;
            _jwtAuthPlugin = jwtAuthPlugin;
        }

        private DbConfig getDbConfig(string dbName)
        {
            var matchedDbConfigs = from db in _dbConfigs where db.name.Trim() == dbName.Trim() select db;
            if (matchedDbConfigs.Count() != 1)
            {
                throw new TomlFileException($"Expected one database named as {dbName}, but {matchedDbConfigs.Count()} found.");
            }
            else
            {
                return matchedDbConfigs.First();
            }
        }

        private void validateRequest(SqlRequestWrapper sqlRequestWrapper)
        {
            //check traceId
            if (String.IsNullOrWhiteSpace(sqlRequestWrapper.request.traceId))
            {
                throw new RequestArgumentException("request.traceId required");
            }

            var sqlContext = sqlRequestWrapper.request.sqlContext;

            //check db
            string dbName = sqlContext.db;
            if (String.IsNullOrWhiteSpace(dbName))
            {
                throw new RequestArgumentException("request.sqlContext.db required");
            }
            DbConfig dbConfig = getDbConfig(dbName);
            if (dbConfig == null)
            {
                throw new RequestArgumentException($"database {dbName} not defined in configuration file");
            }
        }

        private void writebackRequest(SqlRequestWrapper sqlRequestWrapper, SqlResultWrapper result)
        {
            string traceId = sqlRequestWrapper.request.traceId;
            if (_systemConfig.writebackRequest)
            {
                result.request = sqlRequestWrapper.request; //writeback both sqlContext and traceId
                if (_systemConfig.writebackInBase64)
                {
                    result.request.sqlContext.sql = result.request.sqlContext.getBase64Sql();
                }
                else
                {
                    result.request.sqlContext.sql = result.request.sqlContext.getPlainSql();
                }
            }
            else
            {
                if (result.request == null)
                {
                    result.request = new SqlRequest(); //set one empty request in result object
                }
                result.request.traceId = traceId; //just only writeback traceId
            }
        }

        public SqlResultWrapper process(HttpContext httpContext, SqlRequestWrapper sqlRequestWrapper, EndpointContext endpointContext)
        {
            SqlResultWrapper result = new SqlResultWrapper();
            SqlResponse sqlResponse = new SqlResponse();
            result.response = sqlResponse;
            try
            {
                _appState.markNewRequest();

                //firewall check
                string firewallMsg;
                if (_firewallPlugin.check(httpContext, endpointContext, out firewallMsg) == false)
                {
                    throw new SecurityException(firewallMsg);
                }

                //basic auth check
                string basicAuthCheckMsg;
                if (_basicAuthPlugin.check(httpContext, endpointContext, out basicAuthCheckMsg) == false)
                {
                    throw new SecurityException(basicAuthCheckMsg);
                }

                //jwt auth check
                string jwtAuthCheckMsg;
                if (_jwtAuthPlugin.check(httpContext, endpointContext, out jwtAuthCheckMsg) == false)
                {
                    throw new SecurityException(jwtAuthCheckMsg);
                }

                //validate request and sqlFile
                validateRequest(sqlRequestWrapper);

                //execute SQL
                executeSql(sqlRequestWrapper, endpointContext, result);

                _appState.markRequestCompleted(isFailed: false);
            }
            catch (MyRestException ex)
            {
                _appState.markRequestCompleted(isFailed: true);
                _logger.LogWarning(ex.Message);
                _logger.LogDebug(ex.ToString());
                result.response.returnCode = ex.getErrorCode();
                result.response.errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _appState.markRequestCompleted(isFailed: true);
                _logger.LogWarning(ex.Message);
                _logger.LogDebug(ex.ToString());
                result.response.returnCode = 1;
                result.response.errorMessage = ex.Message;
            }
            finally
            {
                //feedback request
                writebackRequest(sqlRequestWrapper, result);
            }
            return result;
        }

        private void executeSql(SqlRequestWrapper sqlRequestWrapper, EndpointContext endpointContext, SqlResultWrapper result)
        {
            SqlContext sqlContext;
            object dapperParameters;
            prepareDapperArguments(sqlRequestWrapper, endpointContext, result, out sqlContext, out dapperParameters);

            string dbName = sqlContext.db;
            DbConfig dbConfig = getDbConfig(dbName);
            string connectionString = dbConfig.connectionString;
            string dbType = dbConfig.dbType;

            using (IDbConnection conn = DbFactory.newConnection(dbType, connectionString))
            {
                _logger.LogInformation($"Begin to run SQL in db {dbName}, SQL: {sqlContext.getPlainSql()}");
                if (sqlContext.isSelect)
                {
                    if (sqlContext.isScalar == false)
                    {
                        IEnumerable<dynamic> rows = conn.Query(sqlContext.getPlainSql(), dapperParameters);
                        result.response.scalarValue = null;
                        result.response.affectedCount = 0;
                        result.response.rows = rows;
                        result.response.rowCount = rows.Count();
                    }
                    else
                    {
                        result.response.scalarValue = conn.ExecuteScalar(sqlContext.getPlainSql(), dapperParameters);
                        result.response.affectedCount = 0;
                        result.response.rows = null;
                        result.response.rowCount = 0;
                    }
                }
                else if (sqlContext.isSelect == false && endpointContext.onlyAllowSelect == false)
                {
                    result.response.affectedCount = conn.Execute(sqlContext.getPlainSql(), dapperParameters);
                    result.response.scalarValue = null;
                    result.response.rows = null;
                    result.response.rowCount = 0;
                }
                else
                {
                    throw new SqlExecuteException($"{endpointContext.name} only allow select statement");
                }
            }
            _logger.LogInformation($"End to run SQL in db {dbName}, SQL: {sqlContext.getPlainSql()}");
            result.response.returnCode = 0;
            result.response.errorMessage = "";
        }

        private void prepareDapperArguments(SqlRequestWrapper sqlRequestWrapper, EndpointContext endpointContext,
            SqlResultWrapper result, out SqlContext sqlContext, out object dapperParameters)
        {
            SqlRequest request = sqlRequestWrapper.request;
            sqlContext = request.sqlContext;
            string dbName = sqlContext.db;
            DbConfig dbConfig = getDbConfig(dbName);
            string sqlFileHome = dbConfig.sqlFileHome;
            string sqlFile = sqlContext.sqlFile;
            string sqlId = sqlContext.sqlId;

            //rebuild SqlContext
            String fullFileName = Path.Join(sqlFileHome, sqlFile.Trim());
            if (File.Exists(fullFileName))
            {
                _xmlFileContainer.addFile(fullFileName);
                var xmlFileParser = _xmlFileContainer.getParser(fullFileName);
                if ((xmlFileParser != null) && (string.IsNullOrWhiteSpace(sqlId) == false))
                {
                    xmlFileParser.rebuildSqlContext(sqlContext, sqlId);
                }
            }
            if (_systemConfig.enableClientSql == false && sqlContext.isUseClientSql())
            {
                throw new MyRestException("system configuration does not allow clientSql ");
            }
            if (endpointContext.onlyServerSideSql && sqlContext.isUseClientSql())
            {
                throw new MyRestException($"{endpointContext.name} endpoint does not allow clientSql ");
            }

            //build dapper parameters
            dapperParameters = XmlFileParser.buildDapperParameters(sqlContext);
        }
    }
}
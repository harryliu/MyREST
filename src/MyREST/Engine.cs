using Dapper;
using MyREST.Plugin;
using System.Data;
using System.Diagnostics;

namespace MyREST
{
    public class Engine
    {
        private readonly IConfiguration _configuration;

        private GlobalConfig _globalConfig;
        private SystemConfig _systemConfig;
        private List<DbConfig> _dbConfigs;
        private XmlFileContainer _xmlFileContainer;
        private FirewallPlugin _firewall;

        private readonly ILogger<Engine>? _logger;

        public Engine(ILogger<Engine>? logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer, FirewallPlugin firewall
            )
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;
            _dbConfigs = dbConfigs;
            _xmlFileContainer = xmlFileContainer;
            _firewall = firewall;
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

        public SqlResultWrapper process(HttpContext httpContext, SqlRequestWrapper sqlRequestWrapper)
        {
            SqlResultWrapper result = new SqlResultWrapper();
            SqlResponse sqlResponse = new SqlResponse();
            result.response = sqlResponse;
            try
            {
                //feedback traceId
                string traceId = sqlRequestWrapper.request.traceId;
                if (_globalConfig.system.writebackRequest)
                {
                    result.request = sqlRequestWrapper.request; //writeback both sqlContext and traceId
                }
                else
                {
                    result.request = new SqlRequest(); //set one empty request in result object
                    result.request.traceId = traceId; //just only writeback traceId
                }

                //security check 
                string firewallMsg;
                if (_firewall.check(httpContext, out firewallMsg) == false)
                {
                    throw new SecurityException(firewallMsg);
                }

                validateRequest(sqlRequestWrapper);

                executeSql(sqlRequestWrapper, result);
            }
            catch (MyRestException ex)
            {
                result.response.returnCode = ex.getErrorCode();
                result.response.errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                result.response.returnCode = 1;
                result.response.errorMessage = ex.Message;
            }
            return result;
        }

        private void executeSql(SqlRequestWrapper sqlRequestWrapper, SqlResultWrapper result)
        {
            SqlContext sqlContext;
            DynamicParameters dapperParameters;
            prepareDapperArguments(sqlRequestWrapper, result, out sqlContext, out dapperParameters);

            string dbName = sqlContext.db;
            DbConfig dbConfig = getDbConfig(dbName);
            string connectionString = dbConfig.connectionString;
            string dbType = dbConfig.dbType;

            using (IDbConnection conn = ConnectionFactory.newConnection(dbType, connectionString))
            {
                if (sqlContext.isSelect)
                {
                    if (sqlContext.isScalar == false)
                    {
                        IEnumerable<dynamic> rows = conn.Query(sqlContext.sql, dapperParameters);
                        result.response.scalarValue = null;
                        result.response.affectedCount = 0;
                        result.response.rows = rows;
                        result.response.rowCount = rows.Count();
                    }
                    else
                    {
                        result.response.scalarValue = conn.ExecuteScalar(sqlContext.sql, dapperParameters);
                        result.response.affectedCount = 0;
                        result.response.rows = null;
                        result.response.rowCount = 0;
                    }
                }
                else
                {
                    result.response.affectedCount = conn.Execute(sqlContext.sql, dapperParameters);
                    result.response.scalarValue = null;
                    result.response.rows = null;
                    result.response.rowCount = 0;
                }
            }
            result.response.returnCode = 0;
            result.response.errorMessage = "";
        }

        private void prepareDapperArguments(SqlRequestWrapper sqlRequestWrapper, SqlResultWrapper result, out SqlContext sqlContext, out DynamicParameters dapperParameters)
        {
            SqlRequest request = sqlRequestWrapper.request;
            string traceId = request.traceId;
            sqlContext = request.sqlContext;
            string dbName = sqlContext.db;
            DbConfig dbConfig = getDbConfig(dbName);
            string sqlFileHome = dbConfig.trimedSqlFileHome();
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
                throw new MyRestException("system does not allow clientSql ");
            }

            //build dapper parameters
            dapperParameters = XmlFileParser.buildDapperParameters(sqlContext);
        }
    }
}
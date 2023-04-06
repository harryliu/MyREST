using Dapper;
using MySql.Data.MySqlClient;
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
        private Firewall _firewall;

        private readonly ILogger<Engine>? _logger;

        public Engine(ILogger<Engine>? logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer, Firewall firewall
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
                throw new ArgumentException($"Expected one database named as {dbName}, but {matchedDbConfigs.Count()} found.");
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
                throw new ArgumentException("request.traceId required");
            }

            var sqlContext = sqlRequestWrapper.request.sqlContext;

            //check db
            string dbName = sqlContext.db;
            if (String.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentException("request.sqlContext.db required");
            }
            DbConfig dbConfig = getDbConfig(dbName);
            if (dbConfig == null)
            {
                throw new ArgumentException($"database {dbName} not defined in configuration file");
            }

            //check parameter.dataType

            //check parameter.direction

            //check parameter.format
        }

        public SqlResultWrapper process(String? clientIpAddress, SqlRequestWrapper sqlRequestWrapper)
        {
            SqlResultWrapper result = new SqlResultWrapper();
            SqlResponse sqlResponse = new SqlResponse();
            result.response = sqlResponse;
            try
            {
                string firewallMsg;
                if (_firewall.pipelineCheck(clientIpAddress, out firewallMsg) == false)
                {
                    throw new FirewallException(firewallMsg);
                }
                internalProcess(sqlRequestWrapper, result);
            }
            catch (FirewallException ex)
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

        private void internalProcess(SqlRequestWrapper sqlRequestWrapper, SqlResultWrapper result)
        {
            validateRequest(sqlRequestWrapper);

            SqlRequest request = sqlRequestWrapper.request;
            string traceId = request.traceId;
            SqlContext sqlContext = request.sqlContext;
            string dbName = sqlContext.db;
            DbConfig dbConfig = getDbConfig(dbName);
            string connectionString = dbConfig.connectionString;
            string dbType = dbConfig.dbType;
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
                throw new ArgumentException("system does not allow clientSql ");
            }

            if (_globalConfig.system.writebackRequest)
            {
                result.request = request; //writeback both sqlContext and traceId
            }
            else
            {
                result.request = new SqlRequest();
                result.request.traceId = traceId; //just only writeback traceId
            }

            using (IDbConnection conn = new MySqlConnection(connectionString))
            {
                if (sqlContext.isSelect)
                {
                    IEnumerable<dynamic> rows = conn.Query(sqlContext.sql);
                    result.response.affectedCount = 0;
                    result.response.rows = rows;
                    result.response.rowCount = rows.Count();
                }
                else
                {
                    result.response.affectedCount = conn.Execute(sqlContext.sql);
                    result.response.rows = null;
                    result.response.rowCount = 0;
                }
            }
            result.response.returnCode = 0;
            result.response.errorMessage = "";
        }
    }
}
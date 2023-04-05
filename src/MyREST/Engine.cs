using Dapper;
using Microsoft.AspNetCore.Mvc;
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

        private readonly ILogger<Controller> _logger;

        public Engine(ILogger<Controller> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;
            _dbConfigs = dbConfigs;
            _xmlFileContainer = xmlFileContainer;
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

            ////check sqlFile/sqlId/sql
            //string sqlFile = sqlContext.sqlFile;
            //string sqlId = sqlContext.sqlId;
            //string sql = sqlContext.sql;
            //bool useClientSql = (string.IsNullOrWhiteSpace(sql) == false);
            //bool useServerSql = (string.IsNullOrWhiteSpace(dbConfig.trimedSqlFileHome()) == false);
            //if (useServerSql)
            //{
            //    String fullFileName = Path.Join(dbConfig.trimedSqlFileHome(), sqlFile.Trim());
            //    if (string.IsNullOrEmpty(sqlFile) || string.IsNullOrEmpty(sqlId))
            //    {
            //        useServerSql = false;
            //    }
            //    else if (Path.Exists(dbConfig.trimedSqlFileHome()))
            //    {
            //        useServerSql = false;
            //    }
            //    else if (File.Exists(fullFileName))
            //    {
            //        useServerSql = false;
            //    }
            //}
            //if (useClientSql || useServerSql)
            //{
            //    throw new ArgumentException("request.sqlContext sql or sqlFile+sqlId should be provided");
            //}

            //check parameter.dataType

            //check parameter.direction

            //check parameter.format
        }

        public void process(SqlRequestWrapper sqlRequestWrapper, ref SqlResultWrapper result)
        {
        }

        public SqlResultWrapper process(SqlRequestWrapper sqlRequestWrapper)
        {
            //prepare response objects
            SqlResultWrapper result = new SqlResultWrapper();
            SqlResponse sqlResponse = new SqlResponse();

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
                throw new ArgumentException("system does not enable clientSql ");
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
                try
                {
                    if (sqlContext.isSelect)
                    {
                        IEnumerable<dynamic> rows = conn.Query(sqlContext.sql);
                        sqlResponse.affectedCount = 0;
                        sqlResponse.rows = rows;
                        sqlResponse.rowCount = rows.Count();
                    }
                    else
                    {
                        sqlResponse.affectedCount = conn.Execute(sqlContext.sql);
                        sqlResponse.rows = null;
                        sqlResponse.rowCount = 0;
                    }
                    sqlResponse.returnCode = 0;
                    sqlResponse.errorMessage = "";
                }
                catch (Exception ex)
                {
                    sqlResponse.returnCode = 1;
                    sqlResponse.errorMessage = ex.Message;
                }
            }

            result.response = sqlResponse;
            return result;
        }
    }
}
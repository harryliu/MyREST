using Dapper;
using Microsoft.AspNetCore.Mvc;
using MyREST.Controllers;
using MySql.Data.MySqlClient;
using System.Data;
using static MyREST.SystemConfig;

namespace MyREST
{
    public class Engine
    {
        private readonly IConfiguration _configuration;

        private GlobalConfig _globalConfig;
        private List<DbConfig> _dbConfigs;
        private XmlFileContainer _xmlFileContainer;

        private readonly ILogger<QueryController> _logger;

        public Engine(ILogger<QueryController> logger, IConfiguration configuration,
            GlobalConfig globalConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
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
            if (String.IsNullOrWhiteSpace(sqlRequestWrapper.request.traceId))
            {
                throw new ArgumentException("request.traceId required");
            }

            var sqlContext = sqlRequestWrapper.request.sqlContext;

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

            string command = sqlContext.command;
            if (String.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("request.sqlContext.command should be 'Execute' or 'Query' ");
            }
            List<String> commandCandidates = new List<string>();
            commandCandidates.Add("Execute");
            commandCandidates.Add("Query");
            var matchedCommands = from item in commandCandidates
                                  where item.ToUpper() == command.ToUpper()
                                  select item;
            if (matchedCommands.Count() != 1)
            {
                throw new ArgumentException("request.sqlContext.command should be 'Execute' or 'Query' ");
            }

            string sqlFile = sqlContext.sqlFile;
            string sqlId = sqlContext.sqlId;
            string sql = sqlContext.sql;
            if (string.IsNullOrWhiteSpace(sql) && (string.IsNullOrEmpty(sqlFile) || string.IsNullOrEmpty(sqlId)))
            {
                throw new ArgumentException("Neither request.sqlContext.sql nor sqlContext.sqlFile+sqlContext.sqlId supplied");
            }

            //check parameter.dataType

            //check parameter.direction

            //check parameter.format
        }

        public SqlResultWrapper process(SqlRequestWrapper sqlRequestWrapper)
        {
            validateRequest(sqlRequestWrapper);

            SqlRequest request = sqlRequestWrapper.request;
            string traceId = request.traceId;
            SqlContext sqlContext = request.sqlContext;
            string dbName = sqlContext.db;
            if (String.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentException("request.sqlContext.db required");
            }
            DbConfig dbConfig = getDbConfig(dbName);
            string connectionString = dbConfig.connectionString;
            string sqlFile = sqlContext.sqlFile;
            string sqlId = sqlContext.sqlId;
            if (string.IsNullOrWhiteSpace(sqlFile) == false && string.IsNullOrEmpty(sqlId) == false)
            {
                string fullFileName = System.IO.Path.Join(dbConfig.sqlFileHome, sqlFile);
                _xmlFileContainer.AddFile(fullFileName);
                var xmlFileParser = _xmlFileContainer.getParser(fullFileName);
                xmlFileParser.rebuildSqlContext(sqlContext, sqlId);
            }

            SqlResultWrapper result = new SqlResultWrapper();
            SqlResponse sqlResponse = new SqlResponse();

            if (_globalConfig.system.writebackRequest)
            {
                result.request = request; //writeback both sqlContext and traceId
            }
            else
            {
                result.request.traceId = traceId; //just writeback the traceId
            }

            using (IDbConnection conn = new MySqlConnection(connectionString))
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                IEnumerable<dynamic> rows = conn.Query("");
                sqlResponse.rowCount = 100;
                sqlResponse.affectedCount = 200;
                sqlResponse.returnCode = 0;
                sqlResponse.errorMessage = "aaaaaaaaaa";
                sqlResponse.rows = rows;
            }
            result.response = sqlResponse;
            return result;
        }
    }
}
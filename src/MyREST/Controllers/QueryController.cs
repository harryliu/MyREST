using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;

namespace MyREST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private GlobalConfig _globalConfig;

        private SystemConfig _systemConfig;
        private List<DbConfig> _dbConfigs;

        private XmlFileContainer _xmlFileContainer;

        private readonly ILogger<QueryController> _logger;
        private Engine _engine;

        public QueryController(ILogger<QueryController> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer)

        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;
            _dbConfigs = dbConfigs;
            _xmlFileContainer = xmlFileContainer;
            _engine = new Engine(_logger, _configuration, _globalConfig, _systemConfig, _dbConfigs, _xmlFileContainer);
        }

        [HttpGet("/test")]
        public IEnumerable<dynamic> Get2()
        {
            string selectSql = """
                select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate
                from actor
                """;
            ;
            string connectionString = "Server=localhost;Port=3306;Database=sakila;Uid=root;Pwd=TOOR;";
            using (IDbConnection conn = new MySqlConnection(connectionString))
            {
                //conn.ExecuteReader(sql).GetSchemaTable()
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                return conn.Query(selectSql);
            }
        }

        private DbConfig getDbConfig(string name)
        {
            var matchedDbConfigs = from db in _dbConfigs where db.name.Trim() == name.Trim() select db;
            if (matchedDbConfigs.Count() != 1)
            {
                throw new ArgumentException($"Expected one database named as {name}, but {matchedDbConfigs.Count()} found.");
            }
            else
            {
                return matchedDbConfigs.First();
            }
        }

        [HttpPost("/invoke")]
        public SqlResultWrapper invoke([FromBody] SqlRequestWrapper sqlRequestWrapper)
        {
            SqlResultWrapper result = _engine.process(sqlRequestWrapper);
            return result;
        }

        private SqlResultWrapper Get223(  SqlRequestWrapper sqlRequestWrapper)
        {
            string selectSql = """
                select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate
                from actor
                """;
            ;

            SqlRequest request = sqlRequestWrapper.request;
            string traceId = request.traceId;
            SqlContext sqlContext = request.sqlContext;
            SqlResultWrapper result = new SqlResultWrapper();
            SqlResponse sqlResponse = new SqlResponse();
            if (_globalConfig.system.writebackRequest)
            {
                result.request = request; //writeback both sqlContext and traceId
            }
            else
            {
                result.request = new SqlRequest();
                result.request.traceId = traceId; //just writeback the traceId
            }

            string connectionString = "Server=localhost;Port=3306;Database=sakila;Uid=root;Pwd=TOOR;";
            using (IDbConnection conn = new MySqlConnection(connectionString))
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                IEnumerable<dynamic> rows = conn.Query(selectSql);
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
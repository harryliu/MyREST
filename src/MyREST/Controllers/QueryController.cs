using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.ComponentModel.Design;
using System.Data;
using System.Text.RegularExpressions;

namespace MyREST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        private GlobalConfig _globalConfig;
        private List<DbConfig> _dbConfigs;
        private XmlFileContainer _xmlFileContainer;

        private readonly ILogger<QueryController> _logger;

        public QueryController(ILogger<QueryController> logger, IConfiguration configuration,
            GlobalConfig globalConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("mydb1");
            _globalConfig = globalConfig;
            _dbConfigs = dbConfigs;
            _xmlFileContainer = xmlFileContainer;
        }

        [HttpGet("/test")]
        public IEnumerable<dynamic> Get2()
        {
            string selectSql = """
                select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate
                from actor
                """;
            ;

            using (IDbConnection conn = new MySqlConnection(_connectionString))
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
        public SqlResultWrapper Get22([FromBody] SqlRequestWrapper sqlRequestWrapper)
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

            using (IDbConnection conn = new MySqlConnection(_connectionString))
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
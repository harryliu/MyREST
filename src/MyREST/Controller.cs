using Dapper;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace MyREST
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private GlobalConfig _globalConfig;

        private SystemConfig _systemConfig;
        private List<DbConfig> _dbConfigs;

        private XmlFileContainer _xmlFileContainer;

        private readonly ILogger<Controller> _logger;
        private Engine _engine;
        private Firewall _firewall;

        public Controller(ILogger<Controller> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer, Firewall firewall, Engine engine)

        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;
            _dbConfigs = dbConfigs;
            _xmlFileContainer = xmlFileContainer;
            _firewall = firewall;
            _engine = engine;
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

        [HttpPost("/invoke")]
        public SqlResultWrapper invoke([FromBody] SqlRequestWrapper sqlRequestWrapper)
        {
            string? clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ??
            HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();

            SqlResultWrapper result = _engine.process(clientIpAddress, sqlRequestWrapper);
            return result;
        }

        private SqlResultWrapper Get223(SqlRequestWrapper sqlRequestWrapper)
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
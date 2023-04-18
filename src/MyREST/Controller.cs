using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyREST.Plugin;
using System.Data;
using System.Xml.Linq;

namespace MyREST
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Controller> _logger;
        private Engine _engine;
        private AppState _appState;
        private List<DbConfig> _dbConfigs;

        public Controller(ILogger<Controller> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs,
            XmlFileContainer xmlFileContainer, FirewallPlugin firewall, Engine engine, AppState appState)

        {
            _logger = logger;
            _configuration = configuration;
            _engine = engine;
            _appState = appState;
            _dbConfigs = dbConfigs;
        }

        [HttpPost("/run")]
        public SqlResultWrapper run([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SqlRequestWrapper sqlRequestWrapper)
        {
            _logger.LogInformation("Incoming request ==> " + sqlRequestWrapper.ToString());
            SqlResultWrapper result = _engine.process(this.HttpContext, sqlRequestWrapper);
            _logger.LogInformation("Output response ==> " + result.ToString());
            return result;
        }

        [HttpGet("/status")]
        public StateQueryResult queryStatus()
        {
            //_logger.LogInformation("Incoming request ==> " + sqlRequestWrapper.ToString());
            StateQueryResult result = new StateQueryResult();
            result.appState = _appState;
            foreach (var item in _dbConfigs)
            {
                using (IDbConnection conn = DbFactory.newConnection(item.dbType, item.connectionString))
                {
                    string testSql =
                    conn.Query( .getPlainSql(), dapperParameters);
                }
            }

            // _logger.LogInformation("Output response ==> " + result.ToString());
            return result;
        }
    }
}
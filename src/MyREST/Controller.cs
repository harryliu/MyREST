using Dapper;
using Microsoft.AspNetCore.Http;
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
        private FirewallPlugin _firewallPlugin;

        public Controller(ILogger<Controller> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs,
            XmlFileContainer xmlFileContainer, FirewallPlugin firewallPlugin, Engine engine, AppState appState)

        {
            _logger = logger;
            _configuration = configuration;
            _engine = engine;
            _appState = appState;
            _dbConfigs = dbConfigs;
            _firewallPlugin = firewallPlugin;
        }

        [HttpPost("/run")]
        public SqlResultWrapper run([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SqlRequestWrapper sqlRequestWrapper)
        {
            _logger.LogInformation("Run endpoint request ==> " + sqlRequestWrapper.ToString());
            SqlResultWrapper result = _engine.process(this.HttpContext, sqlRequestWrapper);
            _logger.LogInformation("Run endpoint response ==> " + result.ToString());
            return result;
        }

        [HttpGet("/")]
        [HttpGet("/health")]
        public string healthCheck()
        {
            string result = "healthy";
            _logger.LogInformation("health endpoint response ==> " + result.ToString());
            return result;
        }

        [HttpGet("/status")]
        public StateQueryResult queryStatus()
        {
            StateQueryResult result = new StateQueryResult();

            try
            {
                //firewall check
                string firewallMsg;
                if (_firewallPlugin.check(this.HttpContext, out firewallMsg) == false)
                {
                    throw new SecurityException(firewallMsg);
                }

                //return app state
                result.appState = _appState;

                //check all databases connection
                var dbStates = new List<DbState>();
                result.DbStates = dbStates;

                foreach (var item in _dbConfigs)
                {
                    using (IDbConnection conn = DbFactory.newConnection(item.dbType, item.connectionString))
                    {
                        try
                        {
                            string testSql = DbFactory.getTestQuerySql(item.dbType);
                            conn.Query(testSql);
                            dbStates.Add(new DbState() { name = item.name, status = "connected" });
                        }
                        catch
                        {
                            dbStates.Add(new DbState() { name = item.name, status = "fail to connect" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.message = ex.Message;
            }

            _logger.LogInformation("Status endpoint response ==> " + result.ToString());
            return result;
        }
    }
}
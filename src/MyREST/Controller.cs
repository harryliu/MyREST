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
        private GlobalConfig _globalConfig;
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
            _globalConfig = globalConfig;
            _engine = engine;
            _appState = appState;
            _dbConfigs = dbConfigs;
            _firewallPlugin = firewallPlugin;
        }

        [HttpPost("/sql")]
        public SqlResultWrapper runSql([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SqlRequestWrapper sqlRequestWrapper)
        {
            _logger.LogInformation("sql endpoint request ==> " + sqlRequestWrapper.ToString());
            EndpointContext endpointContext = new EndpointContext()
            {
                name = "sql",
                enabled = true,
                needBasicAuthCheck = true,
                needJwtAuthCheck = true,
                needFirewallCheck = true,
                onlyAllowSelect = false,
                onlyServerSideSql = false,
                rowCountLimit = -1
            };

            SqlResultWrapper result = _engine.process(this.HttpContext, sqlRequestWrapper, endpointContext);
            _logger.LogInformation("sql endpoint response ==> " + result.ToString());
            return result;
        }

        [HttpPost("/greenChannelSelect")]
        public SqlResultWrapper runGreenChannelSelect([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SqlRequestWrapper sqlRequestWrapper)
        {
            _logger.LogInformation("greenChannelSelect endpoint request ==> " + sqlRequestWrapper.ToString());
            EndpointContext endpointContext = new EndpointContext()
            {
                name = "greenChannelSelect",
                enabled = _globalConfig.system.enableGreenChannelSelect,
                needBasicAuthCheck = false,
                needJwtAuthCheck = false,
                needFirewallCheck = true,
                onlyAllowSelect = true,
                onlyServerSideSql = true,
                rowCountLimit = 1
            };

            SqlResultWrapper result = _engine.process(this.HttpContext, sqlRequestWrapper, endpointContext);
            _logger.LogInformation("greenChannelSelect endpoint response ==> " + result.ToString());
            return result;
        }

        [HttpGet("/")]
        public string index()
        {
            string result = "MyREST is a universal database RESTful service";
            _logger.LogInformation("index endpoint response ==> " + result.ToString());
            return result;
        }

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
            EndpointContext endpointContext = new EndpointContext()
            {
                name = "status",
                needBasicAuthCheck = false,
                needJwtAuthCheck = false,
                needFirewallCheck = true,
                onlyAllowSelect = true,
                onlyServerSideSql = true
            };

            StateQueryResult result = new StateQueryResult();
            try
            {
                //firewall check
                string firewallMsg;
                if (_firewallPlugin.check(this.HttpContext, endpointContext, out firewallMsg) == false)
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
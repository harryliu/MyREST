using Dapper;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MyREST.Plugin;
using MySql.Data.MySqlClient;
using System.Data;

namespace MyREST
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Controller> _logger;
        private Engine _engine;

        public Controller(ILogger<Controller> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig, List<DbConfig> dbConfigs, XmlFileContainer xmlFileContainer, FirewallPlugin firewall, Engine engine)

        {
            _logger = logger;
            _configuration = configuration;
            _engine = engine;
        }

        [HttpPost("/invoke")]
        public SqlResultWrapper invoke([FromBody] SqlRequestWrapper sqlRequestWrapper)
        {
            SqlResultWrapper result = _engine.process(this.HttpContext, sqlRequestWrapper);
            return result;
        }

        //[HttpGet("/status")]
    }
}
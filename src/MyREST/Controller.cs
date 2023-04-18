using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyREST.Plugin;

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

        [HttpPost("/run")]
        public SqlResultWrapper run([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SqlRequestWrapper sqlRequestWrapper)
        {
            _logger.LogInformation("Incoming request ==> " + sqlRequestWrapper.ToString());
            SqlResultWrapper result = _engine.process(this.HttpContext, sqlRequestWrapper);
            _logger.LogInformation("Output response ==> " + result.ToString());
            return result;
        }

        //[HttpGet("/status")]
    }
}
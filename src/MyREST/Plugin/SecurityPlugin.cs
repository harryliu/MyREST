namespace MyREST.Plugin
{
    public abstract class SecurityPlugin
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<SecurityPlugin>? _logger;
        protected GlobalConfig _globalConfig;
        protected SystemConfig _systemConfig;

        public SecurityPlugin(ILogger<SecurityPlugin>? logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;
        }

        public abstract bool check(HttpContext httpContext, out string checkMessage);
    }
}
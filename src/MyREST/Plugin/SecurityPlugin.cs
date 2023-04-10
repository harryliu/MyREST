namespace MyREST.Plugin
{
    public abstract class SecurityPlugin
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<SecurityPlugin>? _logger;
        protected GlobalConfig _globalConfig;

        public SecurityPlugin(ILogger<SecurityPlugin>? logger, IConfiguration configuration,
            GlobalConfig globalConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
        }

        protected abstract bool internalCheck(HttpContext httpContext, out string checkMessage);

        public bool check(HttpContext httpContext, out string checkMessage)
        {
            try
            {
                return internalCheck(httpContext, out checkMessage);
            }
            catch (Exception ex)
            {
                throw new SecurityException("Security check exception: " + ex.Message);
            }
        }
    }
}
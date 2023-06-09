﻿namespace MyREST.Plugin
{
    public abstract class SecurityPlugin
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<SecurityPlugin> _logger;
        protected GlobalConfig _globalConfig;

        public SecurityPlugin(ILogger<SecurityPlugin> logger, IConfiguration configuration,
            GlobalConfig globalConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
        }

        protected abstract bool internalCheck(HttpContext httpContext, EndpointContext endpointContext, out string checkMessage);

        public bool check(HttpContext httpContext, EndpointContext endpointContext, out string checkMessage)
        {
            try
            {
                return internalCheck(httpContext, endpointContext, out checkMessage);
            }
            catch (Exception ex)
            {
                throw new SecurityException("Security check exception: " + ex.Message);
            }
        }
    }
}
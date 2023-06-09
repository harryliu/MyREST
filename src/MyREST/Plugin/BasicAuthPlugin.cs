﻿using System.Text;

namespace MyREST.Plugin
{
    public class BasicAuthPlugin : SecurityPlugin
    {
        private BasicAuthConfig _basicAuthConfig;

        public BasicAuthPlugin(ILogger<SecurityPlugin> logger, IConfiguration configuration, GlobalConfig globalConfig) :
            base(logger, configuration, globalConfig)
        {
            _basicAuthConfig = globalConfig.basicAuth;
        }

        protected override bool internalCheck(HttpContext httpContext, EndpointContext endpointContext, out string checkMessage)
        {
            bool result = basicAuthCheck(httpContext, endpointContext, out checkMessage);
            _logger.LogInformation(checkMessage);
            return result;
        }

        private bool basicAuthCheck(HttpContext httpContext, EndpointContext endpointContext, out string checkMessage)
        {
            checkMessage = "BasicAuth check bypassed";
            if (_basicAuthConfig.enableBasicAuth == false || endpointContext.needBasicAuthCheck == false)
            {
                return true;
            }
            var cfgUsername = _basicAuthConfig.username;
            var cfgPassword = _basicAuthConfig.password;

            var authHeaders = httpContext.Request.Headers["Authorization"];
            foreach (var header in authHeaders)
            {
                if (string.IsNullOrWhiteSpace(header) == false && header.StartsWith("Basic"))
                {
                    string encodedUserPass = header.Substring("Basic ".Length).Trim();
                    _logger.LogDebug($"Incoming basicAuth encodedUserPass is {encodedUserPass}");

                    Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                    string userPass = encoding.GetString(Convert.FromBase64String(encodedUserPass));
                    string[] parts = userPass.Split(':');
                    string username = parts[0].Trim();
                    string password = parts[1].Trim();
                    if ((username == cfgUsername) && (password == cfgPassword))
                    {
                        checkMessage = "BasicAuth check passed";
                        return true;
                    }
                }
            }

            checkMessage = "BasicAuth check failed because of missed authorization header";
            return false;
        }
    }
}
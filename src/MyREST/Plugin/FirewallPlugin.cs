using GlobExpressions;
using Microsoft.AspNetCore.Http.Features;

namespace MyREST.Plugin
{
    public class FirewallPlugin : SecurityPlugin
    {
        private FirewallConfig _firewallConfig;
        private bool _needCheckIpWhiteList = false;
        private bool _needCheckIpBlackList = false;
        private List<string> _ipWhiteList;
        private List<Glob> _IpWhiteGlobList;
        private List<Glob> _ipBlackGlobList;
        private List<string> _ipBlackList;

        public FirewallPlugin(ILogger<SecurityPlugin> logger, IConfiguration configuration, GlobalConfig globalConfig) :
            base(logger, configuration, globalConfig)
        {
            _firewallConfig = globalConfig.firewall;

            _ipWhiteList = new List<string>();
            _IpWhiteGlobList = new List<Glob>();
            if (_firewallConfig.ipWhiteList != null)
            {
                foreach (string item in _firewallConfig.ipWhiteList)
                {
                    _ipWhiteList.Add(item.Trim());
                    _IpWhiteGlobList.Add(new Glob(item.Trim()));
                }
            }
            _needCheckIpWhiteList = _firewallConfig.enableIpWhiteList && _ipWhiteList.Count() > 0;

            _ipBlackList = new List<string>();
            _ipBlackGlobList = new List<Glob>();
            if (_firewallConfig.ipBlackList != null)
            {
                foreach (var item in _firewallConfig.ipBlackList)
                {
                    _ipBlackList.Add(item.Trim());
                    _ipBlackGlobList.Add(new Glob(item.Trim()));
                }
            }
            _needCheckIpBlackList = _firewallConfig.enableIpBlackList && _ipBlackList.Count() > 0;
        }

        protected override bool internalCheck(HttpContext httpContext, EndpointContext endpointContext, out string checkMessage)
        {
            string? clientIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ??
                httpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
            bool result = pipelineCheck(clientIpAddress, endpointContext, out checkMessage);

            _logger.LogInformation($"{checkMessage} for incoming ip address {clientIpAddress} ");
            return result;
        }

        private bool pipelineCheck(string? clientIpAddress, EndpointContext endpointContext, out string checkMessage)
        {
            //initial
            bool result = true;
            checkMessage = "Firewall check bypassed ";
            if (string.IsNullOrEmpty(clientIpAddress) || endpointContext.needFirewallCheck == false)
            {
                return result;
            }
            if (result == false)
            {
                return result;
            }

            //firstly, check whiteList
            if (_needCheckIpWhiteList == false)
            {
                result = true;
                checkMessage = "Firewall ipWhiteList check bypassed";
            }
            else
            {
                bool matched = false;
                foreach (var glob in _IpWhiteGlobList)
                {
                    if (glob.IsMatch(clientIpAddress))
                    {
                        matched = true;
                        break;
                    }
                }
                if (matched)
                {
                    result = true;
                    checkMessage = "Firewall ipWhiteList check passed";
                }
                else
                {
                    result = false;
                    checkMessage = "Firewall ipWhiteList check failed";
                }
            }
            if (result == false)
            {
                return result;
            }

            //secondly, check blackList
            if (_needCheckIpBlackList == false)
            {
                result = true;
                checkMessage = "Firewall ipBlackList check bypassed";
            }
            else
            {
                bool matched = false;
                foreach (var glob in _ipBlackGlobList)
                {
                    if (glob.IsMatch(clientIpAddress))
                    {
                        matched = true;
                        break;
                    }
                }
                if (matched)
                {
                    result = false;
                    checkMessage = "Firewall ipBlackList check failed";
                }
                else
                {
                    result = true;
                    checkMessage = "Firewall ipBlackList check passed";
                }
            }

            return result;
        }
    }
}
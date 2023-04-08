using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Configuration;
using GlobExpressions;
using Microsoft.AspNetCore.Http.Features;

namespace MyREST.Plugin
{
    public class FirewallPlugin : SecurityPlugin
    {
        private bool _needCheckIpWhiteList;
        private bool _needCheckIpBlackList;
        private List<string> _ipWhiteList;
        private List<Glob> _IpWhiteGlobList;
        private List<Glob> _ipBlackGlobList;
        private List<string> _ipBlackList;

        public FirewallPlugin(ILogger<SecurityPlugin>? logger, IConfiguration configuration, GlobalConfig globalConfig, SystemConfig systemConfig) :
            base(logger, configuration, globalConfig, systemConfig)
        {
            _ipWhiteList = new List<string>();
            _IpWhiteGlobList = new List<Glob>();
            foreach (string item in _systemConfig.ipWhiteList)
            {
                _ipWhiteList.Add(item.Trim());
                _IpWhiteGlobList.Add(new Glob(item.Trim()));
            }
            _needCheckIpWhiteList = systemConfig.enableIpWhiteList && _ipWhiteList.Count() > 0;

            _ipBlackList = new List<string>();
            _ipBlackGlobList = new List<Glob>();
            foreach (var item in _systemConfig.ipBlackList)
            {
                _ipBlackList.Add(item.Trim());
                _ipBlackGlobList.Add(new Glob(item.Trim()));
            }
            _needCheckIpBlackList = systemConfig.enableIpBlackList && _ipBlackList.Count() > 0;
        }

        public override bool check(HttpContext httpContext, out string checkMessage)
        {
            string? clientIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ??
                httpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
            return pipelineCheck(clientIpAddress, out checkMessage);
        }

        private bool pipelineCheck(string? clientIpAddress, out string checkMessage)
        {
            //initial
            bool result = true;
            checkMessage = "Firewall check bypassed ";
            if (string.IsNullOrEmpty(clientIpAddress))
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
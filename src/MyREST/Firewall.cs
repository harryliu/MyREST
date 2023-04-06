using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Configuration;
using GlobExpressions;

namespace MyREST
{
    public class Firewall
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Controller> _logger;
        private GlobalConfig _globalConfig;
        private SystemConfig _systemConfig;
        private bool _hasIpWhiteList;
        private bool _hasIpBlackList;
        private List<String> _clientIpWhiteList;
        private List<Glob> _clientIpWhiteGlobList;
        private List<Glob> _clientIpBlackGlobList;
        private List<String> _clientIpBlackList;

        public Firewall(ILogger<Controller> logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;

            List<string> strategyList = new List<string>();
            foreach (var item in _systemConfig.firewallStrategies)
            {
                strategyList.Add(item);
            }
            _hasIpWhiteList = strategyList.Contains("clientIpWhiteList");
            _hasIpBlackList = strategyList.Contains("clientIpBlackList");

            _clientIpWhiteList = new List<string>();
            _clientIpWhiteGlobList = new List<Glob>();
            foreach (var item in _systemConfig.clientIpWhiteList)
            {
                _clientIpWhiteList.Add(item.Trim());
                _clientIpWhiteGlobList.Add(new Glob(item.Trim()));
            }

            _clientIpBlackList = new List<string>();
            _clientIpBlackGlobList = new List<Glob>();
            foreach (var item in _systemConfig.clientIpBlackList)
            {
                _clientIpBlackList.Add(item.Trim());
                _clientIpBlackGlobList.Add(new Glob(item.Trim()));
            }
        }

        public bool pipelineCheck(string? clientIpAddress, out string checkMessage)
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
            if (_hasIpWhiteList == false)
            {
                result = true;
                checkMessage = "Firewall clientIpWhiteList check bypassed";
            }
            else
            {
                bool matched = false;
                foreach (var glob in _clientIpWhiteGlobList)
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
                    checkMessage = "Firewall clientIpWhiteList check passed";
                }
                else
                {
                    result = false;
                    checkMessage = "Firewall clientIpWhiteList check failed";
                }
            }
            if (result == false)
            {
                return result;
            }

            //secondly, check blackList
            if (_hasIpBlackList == false)
            {
                result = true;
                checkMessage = "Firewall clientIpBlackList check bypassed";
            }
            else
            {
                bool matched = false;
                foreach (var glob in _clientIpBlackGlobList)
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
                    checkMessage = "Firewall clientIpBlackList check failed";
                }
                else
                {
                    result = true;
                    checkMessage = "Firewall clientIpBlackList check passed";
                }
            }

            return result;
        }
    }
}
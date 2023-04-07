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
        private readonly ILogger<Firewall>? _logger;
        private GlobalConfig _globalConfig;
        private SystemConfig _systemConfig;
        private bool _needCheckIpWhiteList;
        private bool _needCheckIpBlackList;
        private List<String> _ipWhiteList;
        private List<Glob> _IpWhiteGlobList;
        private List<Glob> _ipBlackGlobList;
        private List<String> _ipBlackList;

        public Firewall(ILogger<Firewall>? logger, IConfiguration configuration,
            GlobalConfig globalConfig, SystemConfig systemConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _globalConfig = globalConfig;
            _systemConfig = systemConfig;

            _ipWhiteList = new List<string>();
            _IpWhiteGlobList = new List<Glob>();
            foreach (var item in _systemConfig.ipWhiteList)
            {
                _ipWhiteList.Add(item.Trim());
                _IpWhiteGlobList.Add(new Glob(item.Trim()));
            }
            _needCheckIpWhiteList = (systemConfig.enableIpWhiteList && _ipWhiteList.Count() > 0);

            _ipBlackList = new List<string>();
            _ipBlackGlobList = new List<Glob>();
            foreach (var item in _systemConfig.ipBlackList)
            {
                _ipBlackList.Add(item.Trim());
                _ipBlackGlobList.Add(new Glob(item.Trim()));
            }
            _needCheckIpBlackList = (systemConfig.enableIpBlackList && _ipBlackList.Count() > 0);
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
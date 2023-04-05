using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Configuration;

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
            foreach (var item in _systemConfig.clientIpWhiteList)
            {
                _clientIpWhiteList.Add(item);
            }
            _clientIpBlackList = new List<string>();
            foreach (var item in _systemConfig.clientIpBlackList)
            {
                _clientIpBlackList.Add(item);
            }
        }

        public bool pipelineCheck(string clientIpAddress, out string checkMessage)
        {
            //firstly, check whiteList

            //secondly, check blackList
        }

        // Check if IP Address is Allowed
        private bool IsAllowedIPAddress(IPAddress ipAddress)
        {
            // Implement your own logic to check if the IP Address is allowed
            return true; // For now, return true to allow all IP addresses
        }

        // Implement Firewall
        private void ImplementFirewall(Socket clientSocket)
        {
            IPAddress ipAddress = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;
            if (!IsAllowedIPAddress(ipAddress))
            {
                clientSocket.Close();
            }
        }

        // Accept Incoming Connections
        private void AcceptIncomingConnection(Socket serverSocket)
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                ImplementFirewall(clientSocket);
                // Process Incoming Request
            }
        }
    }
}
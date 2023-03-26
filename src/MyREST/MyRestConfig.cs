namespace MyREST
{
    public class SystemConfig
    {
        public bool debug { get; set; }
        public int port { get; set; }
        public string host { get; set; }
        public string baseRoute { get; set; }
        public bool enableClientSql { get; set; }
        public string clientAccessPolicy { get; set; }
        public string clientIpWhiteList { get; set; }
        public string clientIpBlackList { get; set; }

        //public SystemConfig(IConfiguration configuration)
        //{
        //    //debug = configuration.GetSection("debug").valu GetValue<Boolean>();
        //    //Environment = configuration.GetSection("Environment").Value;
        //}
    }

    /*
     * [ "system" ]
debug = true  #debug模式下将开启swagger ui
port = 3000
host = "localhost"
basePath ="/myrest"
enableClientSql =true  #allow client to submit SQL statement. It only works in debug mode.
clientAccessPolicy=["whiteList","blackList"]
clientIpWhiteList=["localhost","192.168.0.1"]
clientIpBlackList=["192.168.0.155"]

     */
}
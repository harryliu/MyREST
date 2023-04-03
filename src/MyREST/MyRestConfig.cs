using Nett;

namespace MyREST
{
    public class GlobalConfig
    {
        public SystemConfig system { get; set; }
        //public AuthConfig auth { get; set; }
    }

    public class SystemConfig
    {
        public bool debug { get; set; } = false;

        public int port { get; set; }

        public string host { get; set; }
        public string baseRoute { get; set; }
        public bool enableClientSql { get; set; } = false;
        public string[] clientAccessPolicy { get; set; }
        public string[] clientIpWhiteList { get; set; }
        public string[] clientIpBlackList { get; set; }
        public bool useResponseCompression { get; set; } = false;
        public bool hotReloadSqlFile { get; set; } = false;
    }

    public class DbConfig
    {
        public string name { get; set; }
        public string dbType { get; set; }
        public string connectionString { get; set; }
        public string sqlFileHome { get; set; }
    }

    /*
    isMainDb=true  #MainDb or StdDb, auth userTable will only stored in mainDb
    dbType="mysql" # sqlite,mysql,mssql,postgresql,oracle
    connectionString="Server=localhost;Port=3306;Database=sakila;Uid=root;Pwd=TOOR;"
    route="/db1"
    sqlFileHome=""

     */
}
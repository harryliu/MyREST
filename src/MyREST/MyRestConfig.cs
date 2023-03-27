namespace MyREST
{
    public class SystemConfig
    {
        public const string Section = "system";
        public bool debug { get; set; }
        public int port { get; set; }
        public string host { get; set; }
        public string baseRoute { get; set; }
        public bool enableClientSql { get; set; }
        public string[] clientAccessPolicy { get; set; }
        public string[] clientIpWhiteList { get; set; }
        public string[] clientIpBlackList { get; set; }
    }

    public class DbConfig
    {
        public bool isMainDb { get; set; }
        public string dbType { get; set; }
        public string connectionString { get; set; }
        public string route { get; set; }
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
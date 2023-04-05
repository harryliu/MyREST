using System.Data;

namespace MyREST
{
    public class GlobalConfig
    {
        public SystemConfig system { get; set; }

        //public AuthConfig auth { get; set; }
    }

    public class SystemConfig
    {
        public bool enableSwagger { get; set; } = true;
        public bool enableClientSql { get; set; } = false;

        public bool useResponseCompression { get; set; } = false;
        public bool hotReloadSqlFile { get; set; } = false;

        public bool writebackRequest { get; set; } = false;

        public string[] clientAccessPolicies { get; set; }
        public string[] clientIpWhiteList { get; set; }
        public string[] clientIpBlackList { get; set; }

        public void validate()
        {
            HashSet<string> builtinPolicies = new HashSet<string>() { "whiteList", "blackList" };
            HashSet<string> userPolicies = new HashSet<string>();
            foreach (var item in clientAccessPolicies)
            {
                userPolicies.Add(item);
            }
            var minusResult = userPolicies.Except(builtinPolicies);
            if (minusResult.Count() > 0)
            {
                throw new Exception("invalid clientAccessPolicies. It can only contains whiteList and blackList ");
            }
        }
    }

    public class DbConfig
    {
        public string name { get; set; }
        public string dbType { get; set; }
        public string connectionString { get; set; }
        public string sqlFileHome { get; set; }

        public string trimedSqlFileHome()
        {
            return sqlFileHome.Trim();
        }

        /// <summary>
        /// validate one single database configuration
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void validate()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("name should be assigned");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception($"connectionString should be assigned for database {name}");
            }

            if (string.IsNullOrWhiteSpace(sqlFileHome))
            {
                throw new Exception($"sqlFileHome should be assigned for database {name}");
            }

            if (Path.Exists(trimedSqlFileHome()) == false)
            {
                throw new Exception($"sqlFileHome path {trimedSqlFileHome()} does not exist for database {name}");
            }

            String supportedDbType = "sqlite,mysql,mssql,postgresql,oracle";
            var supportedDbTypeArray = supportedDbType.Split(',');
            List<string> builtinDbTypes = supportedDbTypeArray.ToList<String>();
            if (builtinDbTypes.Contains(dbType.Trim()) == false)
            {
                throw new Exception($"invalid dbType {dbType.Trim()}. it should be {supportedDbType} ");
            }
        }

        /// <summary>
        /// validate all databases configuration
        /// </summary>
        /// <param name="dbConfigs"></param>
        /// <exception cref="Exception"></exception>
        public static void validate(List<DbConfig> dbConfigs)
        {
            var names = from db in dbConfigs select db.name.Trim();
            if (names.Distinct().Count() != dbConfigs.Count())
            {
                throw new Exception("duplicated db name found");
            }
        }
    }
}
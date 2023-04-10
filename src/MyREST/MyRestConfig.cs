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

        public bool enableIpWhiteList { get; set; } = false;
        public bool enableIpBlackList { get; set; } = false;
        public string[] ipWhiteList { get; set; }
        public string[] ipBlackList { get; set; }

        public void validate()
        {
        }
    }

    public class DbConfig
    {
        public string name { get; set; }
        public string dbType { get; set; }
        public string connectionString { get; set; }

        public string sqlFileHome
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_sqlFileHome))
                {
                    return Directory.GetCurrentDirectory();
                }
                else
                {
                    return _sqlFileHome.Trim();
                }
            }
            set { _sqlFileHome = value; }
        }

        private string? _sqlFileHome = null;

        /// <summary>
        /// validate one single database configuration
        /// </summary>
        /// <exception cref="TomlFileException"></exception>
        public void validate()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new TomlFileException("name should be assigned");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new TomlFileException($"connectionString should be assigned for database {name}");
            }

            if (string.IsNullOrWhiteSpace(sqlFileHome))
            {
                throw new TomlFileException($"sqlFileHome should be assigned for database {name}");
            }

            if (Path.Exists(sqlFileHome) == false)
            {
                throw new TomlFileException($"sqlFileHome path {sqlFileHome} does not exist for database {name}");
            }

            String supportedDbType = "sqlite,mysql,mssql,postgresql,oracle";
            var supportedDbTypeArray = supportedDbType.Split(',');
            List<string> builtinDbTypes = supportedDbTypeArray.ToList<String>();
            if (builtinDbTypes.Contains(dbType.Trim()) == false)
            {
                throw new TomlFileException($"invalid dbType {dbType.Trim()}. it should be {supportedDbType} ");
            }
        }

        /// <summary>
        /// validate all databases configuration
        /// </summary>
        /// <param name="dbConfigs"></param>
        /// <exception cref="TomlFileException"></exception>
        public static void validate(List<DbConfig> dbConfigs)
        {
            var names = from db in dbConfigs select db.name.Trim();
            if (names.Distinct().Count() != dbConfigs.Count())
            {
                throw new TomlFileException("duplicated db name found");
            }
        }
    }
}
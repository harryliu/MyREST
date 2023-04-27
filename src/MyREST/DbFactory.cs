using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MyREST
{
    public class DbFactory
    {
        public static IDbConnection newConnection(string dbType, string connectionString)
        {
            dbType = dbType.Trim().ToLower();
            if (dbType == "mysql")
            {
                return new MySqlConnection(connectionString);
            }
            else if (dbType == "mssql")
            {
                return new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            }
            else if (dbType == "postgresql")
            {
                return new NpgsqlConnection(connectionString);
            }
            else if (dbType == "oracle")
            {
                return new OracleConnection(connectionString);
            }
            else
            {
                return new SqliteConnection(connectionString);
            }
        }

        /// <summary>
        /// https://bobby-tables.com/adodotnet
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static List<string> getParameterPrefix(string dbType)
        {
            List<string> lst = new List<string>();
            dbType = dbType.Trim().ToLower();
            if (dbType == "mysql")
            {
                lst.Add("@");
            }
            else if (dbType == "mssql")
            {
                lst.Add("@");
            }
            else if (dbType == "postgresql")
            {
                lst.Add("@");
                lst.Add(":");
            }
            else if (dbType == "oracle")
            {
                lst.Add(":");
            }
            else
            {
                lst.Add("@");
                lst.Add("$");
                lst.Add(":");
            }
            return lst;
        }

        public static string getTestQuerySql(string dbType)
        {
            dbType = dbType.Trim().ToLower();
            if (dbType == "mysql")
            {
                return "select 1";
            }
            else if (dbType == "mssql")
            {
                return "select 1";
            }
            else if (dbType == "postgresql")
            {
                return "select 1";
            }
            else if (dbType == "oracle")
            {
                return "select 1 from dual";
            }
            else
            {
                return "select 1 from dual";
            }
        }
    }
}
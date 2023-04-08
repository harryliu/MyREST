using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MyREST
{
    public class ConnectionFactory
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
    }
}
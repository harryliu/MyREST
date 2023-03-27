using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.ComponentModel.Design;
using System.Data;
using System.Text.RegularExpressions;

namespace MyREST.Controllers
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        private SystemConfig _systemConfig;
        private List<DbConfig> _dbConfigs;

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<QueryController> _logger;

        public QueryController(ILogger<QueryController> logger, IConfiguration configuration,
            SystemConfig systemConfig, IOptions<List<DbConfig>> dbConfigs)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("mydb1");
            _systemConfig = systemConfig;
            _dbConfigs = dbConfigs.Value;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        [HttpGet("/test")]
        public IEnumerable<dynamic> Get2()
        {
            string selectSql = """
                select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate
                from actor
                """;
            ;

            DynamicParameters parameter = new DynamicParameters();
            using (IDbConnection conn = new MySqlConnection(_connectionString))
            {
                //conn.ExecuteReader(sql).GetSchemaTable()
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                return conn.Query(selectSql);
            }
        }
    }
}
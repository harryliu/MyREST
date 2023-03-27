using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

using Tommy.Extensions.Configuration;
using Tommy;

//using Alexinea.Extensions.Configuration.Toml;

namespace MyREST
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            AddConfiguration(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            //app.Run("http://localhost:3000");
            app.Run();
        }

        private static void AddConfiguration(IServiceCollection services)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("appsettings.json")
                //.AddTomlFile("myrest.toml")
                .AddTomlFile("myrest.toml", optional: false, reloadOnChange: false)
                ;

            IConfiguration configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);

            //注入 system configuration
            SystemConfig systemConfig = new SystemConfig();
            configuration.Bind(SystemConfig.Section, systemConfig);
            services.AddSingleton(systemConfig);

            List<DbConfig> dbConfigList = new List<DbConfig>();

            TomlArray tomlArray = new TomlArray();
            tomlArray.IsTableArray = true;
            TOML.p
            configuration.Get()
            var section = configuration.GetSection("[system]");
            services.Configure<List<DbConfig>>(section);

            //configuration.Bind("", dbConfigList);

            //services.Configure<SystemConfig>(configuration.GetSection("system"));
        }
    }
}
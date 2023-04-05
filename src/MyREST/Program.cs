using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Nett;
using System;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Builder;

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

            GlobalConfig globalConfig;
            AddConfiguration(builder.Services, out globalConfig);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || globalConfig.system.enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void AddConfiguration(IServiceCollection services, out GlobalConfig globalConfig)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);

            string tomlFile = "myrest.toml";

            //read system configuration in toml file
            globalConfig = Toml.ReadFile<GlobalConfig>(tomlFile);
            SystemConfig systomConfig = globalConfig.system;
            systomConfig.validate();

            //read db configurations in toml file
            TomlTableArray databaseTable = Toml.ReadFile(tomlFile).Get<TomlTableArray>("databases");
            List<DbConfig> dbConfigList = databaseTable.Items
                                            .Select(item => item.Get<DbConfig>())
                                            .ToList();
            //validate db configuration
            foreach (var db in dbConfigList)
            {
                db.validate();
            }
            DbConfig.validate(dbConfigList);

            //add Toml configuration objects to DI
            services.AddSingleton<SystemConfig>(systomConfig);
            services.AddSingleton<GlobalConfig>(globalConfig);
            services.AddSingleton<List<DbConfig>>(dbConfigList);

            //add server side sql XmlFileContainer to DI
            var hotReload = systomConfig.hotReloadSqlFile;
            XmlFileContainer xmlFileContainer = new XmlFileContainer(hotReload);
            services.AddSingleton<XmlFileContainer>(xmlFileContainer);
        }
    }
}
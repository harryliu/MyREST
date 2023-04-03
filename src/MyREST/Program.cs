using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Nett;
using System;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;

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
                .AddJsonFile("appsettings.json")
                //.AddTomlFile("myrest.toml")
                //.AddTomlFile("myrest.toml", optional: false, reloadOnChange: false)
                ;

            IConfiguration configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);

            TomlTableArray databaseTable = Toml.ReadFile("myrest.toml").Get<TomlTableArray>("databases");
            List<DbConfig> dbConfigList = databaseTable.Items
                                            .Select(item => item.Get<DbConfig>())
                                            .ToList();

            services.AddSingleton<List<DbConfig>>(dbConfigList);

            GlobalConfig globalConfig = Toml.ReadFile<GlobalConfig>("myrest.toml");
            services.AddSingleton<GlobalConfig>(globalConfig);

            var hotReload = globalConfig.system.hotReloadSqlFile;
            XmlFileContainer xmlFileContainer = new XmlFileContainer(hotReload);
            services.AddSingleton<XmlFileContainer>(xmlFileContainer);
        }
    }
}
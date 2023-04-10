using Microsoft.AspNetCore.ResponseCompression;
using MyREST.Plugin;
using Nett;
using System.IO.Compression;
using NLog;
using static Org.BouncyCastle.Math.EC.ECCurve;
using NLog.Extensions.Logging;
using NLog.Web;

namespace MyREST
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //   Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //enable NLog
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            GlobalConfig globalConfig;
            AddConfiguration(builder.Services, out globalConfig);

            if (globalConfig.system.useResponseCompression)
            {
                AddCompressionProvider(builder.Services);
            }

            var app = builder.Build();

            if (globalConfig.system.useResponseCompression)
            {
                app.UseResponseCompression();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || globalConfig.system.enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();  //
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void AddCompressionProvider(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.EnableForHttps = true;
            });
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
        }

        private static void AddConfiguration(IServiceCollection services, out GlobalConfig globalConfig)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);

            //Add NLog
            var logger = LogManager.Setup()
                                   .LoadConfigurationFromSection(configuration)
                                   .GetCurrentClassLogger();

            //read myrest.toml file
            string tomlFile = "myrest.toml";

            //read system configuration in toml file
            globalConfig = Toml.ReadFile<GlobalConfig>(tomlFile);
            SystemConfig systemConfig = globalConfig.system;
            systemConfig.validate();

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

            //register Toml configuration objects into DI
            services.AddSingleton<SystemConfig>(systemConfig);
            services.AddSingleton<GlobalConfig>(globalConfig);
            services.AddSingleton<List<DbConfig>>(dbConfigList);

            //register server side sql XmlFileContainer into DI
            var hotReload = systemConfig.hotReloadSqlFile;
            XmlFileContainer xmlFileContainer = new XmlFileContainer(hotReload);
            services.AddSingleton<XmlFileContainer>(xmlFileContainer);

            //register firewall object
            var provider = services.BuildServiceProvider();
            var firewallLogger = provider.GetService<ILogger<FirewallPlugin>>();
            FirewallPlugin firewall = new FirewallPlugin(firewallLogger, configuration, globalConfig, systemConfig);
            services.AddSingleton<FirewallPlugin>(firewall);

            //register engine object
            var engineLLogger = provider.GetService<ILogger<Engine>>();
            Engine engine = new Engine(engineLLogger, configuration, globalConfig, systemConfig, dbConfigList, xmlFileContainer, firewall);
            services.AddSingleton<Engine>(engine);
        }
    }
}
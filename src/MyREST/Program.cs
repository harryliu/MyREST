using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using MyREST.Plugin;
using Nett;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System.IO.Compression;

namespace MyREST
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the DI container
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //register NLog
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            var logger = LogManager.GetCurrentClassLogger();
            GlobalConfig globalConfig;
            AddCustomizedObjects(builder.Services, out globalConfig);

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
            logger.Info("MyREST service started");
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

        private static void AddCustomizedObjects(IServiceCollection services, out GlobalConfig globalConfig)
        {
            var provider = services.BuildServiceProvider();
            globalConfig = new GlobalConfig();

            IConfigurationBuilder cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration configuration = cfgBuilder.Build();
            services.AddSingleton<IConfiguration>(configuration);

            //initialize logger object
            var logger = LogManager.Setup()
                                   .LoadConfigurationFromSection(configuration)
                                   .GetCurrentClassLogger();
            //var logger2 = LogManager.GetCurrentClassLogger();

            try
            {
                //read MyREST.toml file
                string tomlFile = "MyREST.toml";

                //read system configuration in toml file
                globalConfig = Toml.ReadFile<GlobalConfig>(tomlFile);
                SystemConfig systemConfig = globalConfig.system;
                systemConfig.validate();
                FirewallConfig firewallConfig = globalConfig.firewall;
                firewallConfig.validate();

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
                services.AddSingleton<FirewallConfig>(firewallConfig);
                services.AddSingleton<GlobalConfig>(globalConfig);
                services.AddSingleton<List<DbConfig>>(dbConfigList);

                //register server side sql XmlFileContainer into DI
                var hotReload = systemConfig.hotReloadSqlFile;
                XmlFileContainer xmlFileContainer = new XmlFileContainer(hotReload);
                services.AddSingleton<XmlFileContainer>(xmlFileContainer);

                //register firewallPlugin object
                var firewallLogger = provider.GetRequiredService<ILogger<FirewallPlugin>>();
                FirewallPlugin firewallPlugin = new FirewallPlugin(firewallLogger, configuration, globalConfig);
                services.AddSingleton<FirewallPlugin>(firewallPlugin);

                //register basicAuthPlugin object
                var basicAuthLogger = provider.GetRequiredService<ILogger<BasicAuthPlugin>>();
                BasicAuthPlugin basicAuthPlugin = new BasicAuthPlugin(basicAuthLogger, configuration, globalConfig);
                services.AddSingleton<BasicAuthPlugin>(basicAuthPlugin);

                //register jwtAuthPlugin object
                var jwtAuthLogger = provider.GetRequiredService<ILogger<JwtAuthPlugin>>();
                JwtAuthPlugin jwtAuthPlugin = new JwtAuthPlugin(jwtAuthLogger, configuration, globalConfig);
                services.AddSingleton<JwtAuthPlugin>(jwtAuthPlugin);

                //register AppState object
                var appState = new AppState() { startTime = DateTime.Now };
                services.AddSingleton<AppState>(appState);

                //register engine object
                var engineLLogger = provider.GetRequiredService<ILogger<Engine>>();
                Engine engine = new Engine(engineLLogger, configuration, globalConfig, systemConfig,
                    dbConfigList, xmlFileContainer, appState, firewallPlugin, basicAuthPlugin, jwtAuthPlugin);
                services.AddSingleton<Engine>(engine);

                //register MyRestHostLifetime to ensure running requrest handled
                var lifetimeLogger = provider.GetRequiredService<ILogger<MyRestHostLifetime>>();
                services.AddSingleton<IHostLifetime>(sp => new MyRestHostLifetime(
                    sp.GetRequiredService<IHostApplicationLifetime>(),
                    TimeSpan.FromSeconds(0.1),
                    appState, lifetimeLogger));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }
    }
}
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

//using Alexinea.Extensions.Configuration.Toml;

//using Tomlyn.Extensions.Configuration;
using Tommy.Extensions.Configuration;

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

            ///services.AddOptions();
            SystemConfig systemConfig = new SystemConfig();
            configuration.Bind("system", systemConfig);
            services.AddSingleton(systemConfig);

            //services.Configure<SystemConfig>(configuration.GetSection("system"));
        }
    }
}
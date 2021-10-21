using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyHealth.API.Body;
using MyHealth.API.Body.Repository;
using MyHealth.API.Body.Repository.Interfaces;
using MyHealth.API.Body.Services;
using MyHealth.API.Body.Services.Interfaces;
using MyHealth.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyHealth.API.Body
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddLogging();

            builder.Services.AddSingleton(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
                {
                    MaxRetryAttemptsOnRateLimitedRequests = 3,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60)
                };
                return new CosmosClient(configuration["CosmosDBConnectionString"], cosmosClientOptions);
            });

            builder.Services.AddSingleton<IServiceBusHelpers>(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                return new ServiceBusHelpers(configuration["ServiceBusConnectionString"]);
            });

            builder.Services.AddTransient<IBodyRepository, BodyRepository>();
            builder.Services.AddTransient<IBodyService, BodyService>();
        }
    }
}

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(SunTech.Exam.Startup))]

namespace SunTech.Exam
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string endpointUri = Environment.GetEnvironmentVariable("EndPointUri");
            string primaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
            CosmosClient cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "SunTech.Exam" });
            builder.Services.AddSingleton(typeof(CosmosClient), cosmosClient);
        }
    }
}

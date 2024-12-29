using Infra;
using Orleans.Configuration;
using Orleans.Serialization;

namespace DynamicCodeApi;

public static class OrleansClientManager
{
    public static async Task<IClusterClient> GetClient()
    {
        var client = new HostBuilder()
            .UseOrleansClient(clientBuilder =>
            {
                clientBuilder.UseLocalhostClustering();
                clientBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Constants.ClusterId;
                    options.ServiceId = Constants.ServiceId;
                });
            })
            // .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
            .ConfigureServices(f => f.AddSerializer(ser =>
            {
                ser.AddNewtonsoftJsonSerializer(isSupported: type => type.Namespace.StartsWith("Infra"));
            }))
            .Build();
         
        await client.StartAsync();

        return client.Services.GetService<IClusterClient>();
    }
}


using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Serialization;
using Infra;
using Microsoft.Extensions.DependencyInjection;
using Infra.Interfaces;

using var host = new HostBuilder()
    .UseOrleans(builder =>
    {
        builder
            .UseLocalhostClustering()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = Constants.ClusterId;
                options.ServiceId = Constants.ServiceId;
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .Services.AddSerializer(ser =>
            {
                ser.AddNewtonsoftJsonSerializer(isSupported: type => type.Namespace.StartsWith("Infra"));
            })
            // TODO .AddSingleton(YourKeyValueStoreImpl)
            ;
    })
    .Build();

await host.StartAsync();
Console.WriteLine("\n *************************************************************************");
Console.WriteLine("    The Orleans silo started. Press Enter to terminate...    ");
Console.WriteLine("\n *************************************************************************");
Console.ReadLine();
await host.StopAsync();
using Microsoft.Extensions.Hosting;
using Network.ClientServices;
using Network.ServerServices;
using Networking.Communication;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

public static class CommunicationFactory
{
    private static readonly CommunicatorClient _communicatorClient = new();
    private static readonly CommunicatorServer _communicatorServer = new();
    private static ClientServices _grpcClientServices;
    private static ServerServices _grpcServerServices;

    private static IHost _grpcHost; // Global static host variable
    private static readonly object _lock = new(); // Lock for thread safety

    /// <summary>
    /// Factory function to get the communicator.
    /// </summary>
    /// <param name="isClientSide">
    /// Boolean telling if it is client side or server side.
    /// </param>
    /// <param name="isGrpc">Specifies if gRPC is enabled.</param>
    /// <returns>The communicator singleton instance.</returns>
    public static ICommunicator GetCommunicator(bool isClientSide = true, bool isGrpc = false)
    {
        Trace.WriteLine("[Networking] CommunicationFactory.GetCommunicator() function called with isClientSide: " + isClientSide);

        if (isGrpc)
        {
            // Ensure the host is initialized only once
            if (_grpcHost == null)
            {
                lock (_lock)
                {
                    if (_grpcHost == null)
                    {
                        int port = isClientSide ? 7009 : 7000;
                        _grpcHost = Host.CreateDefaultBuilder()
                            .ConfigureWebHostDefaults(webBuilder =>
                            {
                                webBuilder.ConfigureServices(services =>
                                {
                                    services.AddGrpc();
                                    services.AddSingleton<ServerServices>();
                                    services.AddSingleton<ClientServices>();
                                });

                                webBuilder.ConfigureKestrel(options =>
                                {
                                    options.ListenAnyIP(port, listenOptions =>
                                    {
                                        listenOptions.Protocols = HttpProtocols.Http2; // Enable HTTP/2
                                    });
                                });

                                webBuilder.Configure(app =>
                                {
                                    app.UseRouting();
                                    app.UseEndpoints(endpoints =>
                                    {
                                        endpoints.MapGrpcService<ServerServices>();
                                        endpoints.MapGrpcService<ClientServices>();
                                        endpoints.MapGet("/", () => "server running");
                                    });
                                });

                                webBuilder.UseUrls($"http://0.0.0.0:{port}");
                            })
                            .Build();

                        // Start the host in a background thread
                        _grpcHost.Start();

                        // Initialize server services
                        using var scope = _grpcHost.Services.CreateScope();
                        _grpcServerServices = scope.ServiceProvider.GetRequiredService<ServerServices>();
                        _grpcClientServices = scope.ServiceProvider.GetRequiredService<ClientServices>();
                    }
                }
            }

            // Return the appropriate instance
            return isClientSide ? _grpcClientServices : _grpcServerServices;
        }
        else
        {
            // Return non-gRPC instances
            return isClientSide ? _communicatorClient : _communicatorServer;
        }
    }
}

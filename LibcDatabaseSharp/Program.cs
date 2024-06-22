using System.Net;
using LibcDatabaseSharp.Enum;
using LibcDatabaseSharp.Generic;
using LibcDatabaseSharp.WebUI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LibcDatabaseSharp
{
    public class Program
    {
        private static CancellationTokenSource? WebCancellactionTokenSource = new();
        public static ushort WebPort { get; set; } = 8089;
        
        public static async Task Main()
        {
            try
            {
                Logger.SetLogLevel(LogLevel.Debug);
                
                File.Delete(Logger.LogFilePath);
                
                API.Handler.ReadAllLibcFile();

                InitWebServer();
                Console.WriteLine("Task WebService started.");
                Console.WriteLine("Press any key to stop.");
                Console.ReadLine();
                WebCancellactionTokenSource.Cancel();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        
        private static async Task InitWebServer()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var host = new WebHostBuilder()
                        .UseKestrel(options => { options.Listen(IPAddress.Any, WebPort); })
                        .ConfigureServices(services =>
                        {
                            var webService = new Startup();
                            webService.ConfigureServices(services);
                        })
                        .Configure(app =>
                        {
                            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                            var webService = new Startup();
                            webService.Configure(app, env);
                        })
                        .Build();

                    await host.RunAsync(WebCancellactionTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Task WebService stopped.");
                }
            });
        }
    }
}

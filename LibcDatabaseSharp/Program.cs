using System.Net;
using LibcDatabaseSharp.Class;
using LibcDatabaseSharp.Enum;
using LibcDatabaseSharp.Generic;
using LibcDatabaseSharp.WebUI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LibcDatabaseSharp
{
    public class Program
    {
        private static CancellationTokenSource? WebCancellactionTokenSource = new();
        private static ushort WebPort { get; set; } = 8089;

        private static readonly string ConfigFilePath = "config.json";

        private static readonly string Version = "LibcDatabaseSharp v1.0.5 Powered by Kaguya.";

        public static Config Config = null;
        
        public static async Task Main()
        {
            try
            {
                if (LoadConfig())
                {
                    Logger.LogInfo("Config loaded.");
                }

                File.Delete(Logger.LogFilePath);
                API.Handler.ReadAllLibcFile();
                InitWebServer();
                Logger.LogInfo(Version);
                Logger.LogInfo("Press any key to stop.");
                Console.ReadLine();
                WebCancellactionTokenSource.Cancel();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static bool LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                string file = File.ReadAllText(ConfigFilePath);
                
                try
                {
                    Config = JsonConvert.DeserializeObject<Config>(file);
                    
                    Utils.TryParseLogLevel(Config.LogLevel, out LogLevel logLevel);

                    Logger.SetLogLevel(logLevel);
                    WebPort = Config.WebUiPort;

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError('\n' + ex.Message + '\n' + ex.StackTrace);
                    return false;
                }
            }
            else
            {
                Logger.LogError("Config file not exist. Using default config.");
                
                Config config = new Config
                {
                    LogLevel = "Info",
                    WebUiPort = 8089,
                };
                
                string file = JsonConvert.SerializeObject(config);
                
                File.WriteAllText(ConfigFilePath, file);

                return true;
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
                    Logger.LogInfo($"Task WebService stopped.");
                }
            });
        }
    }
}

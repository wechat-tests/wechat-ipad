using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using Topshelf;

namespace Wechat.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<MainServices>(sc => {
                    sc.ConstructUsing(name => new MainServices(args));
                    sc.WhenStarted((s, hc) => s.Start(hc));
                    sc.WhenStopped((s, hc) => s.Stop(hc));
                    sc.WhenShutdown((s, hc) => s.Shutdown(hc));
                });
                x.SetDescription("wechat helper service for puietel ssm");
                x.SetDisplayName("wechat-helper");
                x.SetServiceName("wechat-helper");
                x.EnableServiceRecovery(c => {
                    c.RestartService(1);
                });
                x.StartAutomatically();
                x.RunAsLocalSystem();
            });
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                //byte[] pub=new byte[57];
                //byte[] pri= new byte[328];
                //WeChatHelper.GenerateECKey(0, out pub, out pri);
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog()// NLog: Setup NLog for Dependency injection
            ;
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Topshelf;

namespace Wechat.Api
{
    public class MainServices
    {
        private string[] args;
        //https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-3
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public MainServices(string[] vs)
        {
            args = vs;
        }
        public bool Start(HostControl _)
        {
            logger.Info("service start ");
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = CreateHostBuilder(args.Where(arg => arg != "--console").ToArray());
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                builder.UseContentRoot(pathToContentRoot);
            }
            var host = builder.Build();
            if (isService)
            {
                host.RunAsync();
            }
            else
            {
                host.Run();
            }
            return true;
        }
        public bool Stop(HostControl _)
        {
            logger.Info("service stop ");
            // hostControl.Stop(TopshelfExitCode.Ok);
            return true;
        }
        public bool Shutdown(HostControl _)
        {
            logger.Info("service shutdown ");
            LogManager.Shutdown();
            //hostControl.Stop(TopshelfExitCode.Ok);
            return true;
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
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
}

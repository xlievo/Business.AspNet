using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.AspNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            //.UseConsoleLifetime(opts => opts.SuppressStatusMessages = true)
            .ConfigureLogging(logging => logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error))
            .ConfigureLogging(logging => logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Error))
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                //.UseUrls("http://localhost:6001", "http://192.168.1.107:6001")
                .UseStartup<Startup>();
            });
    }
}

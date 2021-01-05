using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Business.AspNet;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Core;
using System.Net.Http;

namespace WebAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //Document pages need to be accessed across domains
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            //Enable MVC
            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson();

            services.AddHttpClient(string.Empty)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() 
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                });

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /*
            location / {
                proxy_pass http://mystie.com:80;
                proxy_set_header Host $host;
                proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header X-Forwarded-Proto $scheme;
            }
            */

            appLifetime.ApplicationStopped.Register(() =>
            {
                Console.WriteLine("Terminating application...");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            });

            //Document pages need to be accessed across domains
            app.UseCors("any");

            //Override the original global log directory
            Utils.Hosting.LogPath = "/data/mylog.txt";

            //Using third party log components
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                .WriteTo.File(Utils.Hosting.LogPath,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
                .CreateLogger();

            app.CreateBusiness(logOptions =>
            {
                logOptions.Log = (type, message) =>
                {
                    //all non business information
                    switch (type)
                    {
                        case LogOptions.LogType.Error:
                            Log.Error(message);
                            break;
                        case LogOptions.LogType.Exception:
                            Log.Fatal(message);
                            break;
                        case LogOptions.LogType.Info:
                            Log.Information(message);
                            break;
                    }
                };
            }, "test123")
            .UseDoc(options =>
            {
                options.Debug = true;
                options.Benchmark = true;
                options.Navigtion = true;
                options.Testing = true;
            })
            //.UseJsonOptions(options =>
            //{
            //    options.PropertyNamingPolicy = null;
            //})
            //.UseNewtonsoftJson(options =>
            //{
            //    options.ContractResolver = null;
            //})
            .UseMessagePack(options => options.WithCompression(MessagePack.MessagePackCompression.Lz4Block))
            .UseResultType(typeof(MyResultObject<>))//use ResultObject
            .UseWebSocket(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(120);
                options.ReceiveBufferSize = 1024 * 500;
            })
            .UseServer(server =>
            {
                //form
                server.FormOptions.KeyLengthLimit = int.MaxValue;
                server.FormOptions.ValueCountLimit = int.MaxValue;
                server.FormOptions.ValueLengthLimit = int.MaxValue;
                server.FormOptions.MultipartHeadersLengthLimit = int.MaxValue;
                server.FormOptions.MultipartBodyLengthLimit = long.MaxValue;
                server.FormOptions.MultipartBoundaryLengthLimit = int.MaxValue;

                //kestrel
                if (null != server.KestrelOptions)
                {
                    server.KestrelOptions.Limits.MinRequestBodyDataRate = null;
                    server.KestrelOptions.Limits.MinResponseDataRate = null;
                    server.KestrelOptions.Limits.MaxConcurrentConnections = long.MaxValue;
                    server.KestrelOptions.Limits.MaxConcurrentUpgradedConnections = long.MaxValue;
                    server.KestrelOptions.Limits.MaxRequestBodySize = null;
                }
            })
            .UseRouteCTD(options =>
            {
                options.C = "c";
                options.T = "t";
                options.D = "d";
            })
            .UseMultipleParameterDeserialize((parametersType, group, data) =>
            group switch
            {
                //Utils.GroupJson => Help.TryJsonDeserialize(data, parametersType, Business.Core.Configer.JsonOptionsMultipleParameter),
                Utils.GroupJson => Utils.TryNewtonsoftJsonDeserialize(data, parametersType, Utils.NewtonsoftJsonOptions),
                Utils.GroupWebSocket => Utils.MessagePackDeserialize(data, parametersType),
                _ => null,
            })
            .Build();
        }
    }
}

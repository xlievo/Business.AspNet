/*==================================
             ########
            ##########

             ########
            ##########
          ##############
         #######  #######
        ######      ######
        #####        #####
        ####          ####
        ####   ####   ####
        #####  ####  #####
         ################
          ##############
==================================*/

using Business.Core;
using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using Business.Core.Document;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.WebSockets;
using Microsoft.AspNetCore.HttpOverrides;
using Business.Core.Auth;

namespace Business.AspNet
{
    #region Socket Support

    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct ResultObject<Type> : IResult<Type>
    {
        /// <summary>
        /// Activator.CreateInstance
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="genericDefinition"></param>
        /// <param name="checkData"></param>
        public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true)
        {
            this.DataType = dataType;
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = checkData ? !Equals(null, data) : false;
            this.Callback = default;

            this.GenericDefinition = genericDefinition;
        }

        /// <summary>
        /// MessagePack.MessagePackSerializer.Serialize(this)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public ResultObject(Type data, int state = 1, string message = null)
        {
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = !Equals(null, data);

            this.Callback = null;
            this.DataType = null;
            this.GenericDefinition = null;
        }

        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("S")]
        public int State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        public string Message { get; set; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get => Data; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("D")]
        public Type Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("H")]
        public bool HasData { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [System.Text.Json.Serialization.JsonPropertyName("B")]
        public string Callback { get; set; }

        /// <summary>
        /// Data type
        /// </summary>
        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        public System.Type DataType { get; set; }

        /// <summary>
        /// Result object generic definition
        /// </summary>
        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        public System.Type GenericDefinition { get; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Help.JsonSerialize(this);

        /// <summary>
        /// Json format Data
        /// </summary>
        /// <returns></returns>
        public string ToDataString() => Help.JsonSerialize(this.Data);

        /// <summary>
        /// ProtoBuf format
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes() => MessagePack.MessagePackSerializer.Serialize(this);

        /// <summary>
        /// ProtoBuf format Data
        /// </summary>
        /// <returns></returns>
        public byte[] ToDataBytes() => MessagePack.MessagePackSerializer.Serialize(this.Data);
    }

    struct ReceiveData
    {
        /// <summary>
        /// business
        /// </summary>
        public string a;

        /// <summary>
        /// cmd
        /// </summary>
        public string c;

        /// <summary>
        /// token
        /// </summary>
        public string t;

        /// <summary>
        /// data
        /// </summary>
        public byte[] d;

        /// <summary>
        /// callback
        /// </summary>
        public string b;
    }

    /// <summary>
    /// Deserialization of binary format
    /// </summary>
    public class MessagePackArg : ArgumentAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public MessagePackArg(int state = -13, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "MessagePackArg Binary parsing";
            this.ArgMeta.Filter |= FilterModel.NotDefinition;
        }

        /// <summary>
        /// processing method
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            try
            {
                return this.ResultCreate(MessagePack.MessagePackSerializer.Deserialize<Type>(value));
            }
            catch (Exception ex) { return this.ResultCreate(State, Message ?? $"Arguments {this.Alias} MessagePack deserialize error. {ex.Message}"); }
        }
    }

    #endregion

    struct Logs
    {
        public IEnumerable<Logger.LoggerData> Data { get; set; }

        public string Index { get; set; }
    }

    struct Log
    {
        public string Data { get; set; }

        public string Index { get; set; }
    }

    /// <summary>
    /// Environment
    /// </summary>
    public struct Host
    {
        /// <summary>
        /// The urls the hosted application will listen on.
        /// </summary>
        public string Addresses { get; set; }

        /// <summary>
        /// Configuration file "AppSettings" node
        /// </summary>
        public IConfigurationSection AppSettings { get; set; }

        //public IHostingEnvironment ENV { get; set; }

        /// <summary>
        /// HttpClient factory
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; }
    }

    /// <summary>
    /// Built in token object
    /// </summary>
    [Use]
    [Logger(canWrite: false)]
    public struct Token : IToken
    {
        /// <summary>
        /// token
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("K")]
        public string Key { get; set; }

        /// <summary>
        /// Client IP address
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("R")]
        public string Remote { get; set; }

        /// <summary>
        /// Request path
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("P")]
        public string Path { get; set; }

        /// <summary>
        /// Request callback data
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Callback { get; set; }

        /// <summary>
        /// Source of request
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public OriginValue Origin { get; set; }

        /// <summary>
        /// Source of request
        /// </summary>
        public enum OriginValue
        {
            /// <summary>
            /// Default call
            /// </summary>
            Default,
            /// <summary>
            /// HTTP request
            /// </summary>
            Http,
            /// <summary>
            /// WebSocket request
            /// </summary>
            WebSocket
        }
    }

    /// <summary>
    /// Business base class
    /// <para>With "j", "s" group</para>
    /// </summary>
    [Command(Group = Utils.BusinessJsonGroup)]
    [JsonArg(Group = Utils.BusinessJsonGroup)]
    [Command(Group = Utils.BusinessSocketGroup)]
    [MessagePackArg(Group = Utils.BusinessSocketGroup)]
    [Logger]
    public abstract class BusinessBase : BusinessBase<ResultObject<object>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BusinessBase() => this.Logger = new Logger(async (Logger.LoggerData x) => Help.Console(x.ToString()));

        /// <summary>
        /// Get the requested token
        /// </summary>
        public Func<dynamic, Token, Task<IToken>> GetToken { get; set; }
    }

    /// <summary>
    /// A class for an MVC controller with view support.
    /// </summary>
    [Use]
    //Internal object do not write logs
    [Logger(canWrite: false)]
    [RequestSizeLimit(long.MaxValue)]
    //int.MaxValue bug https://github.com/aspnet/AspNetCore/issues/13719
    [RequestFormLimits(KeyLengthLimit = 1_009_100_000, ValueCountLimit = 1_009_100_000, ValueLengthLimit = 1_009_100_000, MultipartHeadersLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue, MultipartBoundaryLengthLimit = int.MaxValue)]
    public class BusinessController : Controller
    {
        /// <summary>
        /// Call
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [EnableCors("any")]
        public async Task<dynamic> Call()
        {
            #region route fixed grouping

            var g = Utils.BusinessJsonGroup;//fixed grouping
            var path = this.Request.Path.Value.TrimStart('/');
            if (!(Configer.Routes.TryGetValue(path, out Configer.Route route) || Configer.Routes.TryGetValue($"{path}/{g}", out route)) || !Utils.bootstrap.BusinessList.TryGetValue(route.Business, out BusinessBase business)) { return this.NotFound(); }

            string c = null;
            string t = null;
            string d = null;
            string value = null;
            //g = route.Group;
            IDictionary<string, string> parameters = null;

            switch (this.Request.Method)
            {
                case "GET":
                    parameters = this.Request.Query.ToDictionary(k => k.Key, v =>
                    {
                        var v2 = (string)v.Value;
                        return !string.IsNullOrEmpty(v2) ? v2 : null;
                    });
                    c = route.Command ?? (parameters.TryGetValue("c", out value) ? value : null);
                    t = parameters.TryGetValue("t", out value) ? value : null;
                    d = parameters.TryGetValue("d", out value) ? value : null;
                    break;
                case "POST":
                    {
                        if (this.Request.HasFormContentType)
                        {
                            parameters = (await this.Request.ReadFormAsync()).ToDictionary(k => k.Key, v =>
                            {
                                var v2 = (string)v.Value;
                                return !string.IsNullOrEmpty(v2) ? v2 : null;
                            });
                            c = route.Command ?? (parameters.TryGetValue("c", out value) ? value : null);
                            t = parameters.TryGetValue("t", out value) ? value : null;
                            d = parameters.TryGetValue("d", out value) ? value : null;
                        }
                        else
                        {
                            c = route.Command;
                            d = System.Web.HttpUtility.UrlDecode(await this.Request.Body.StreamReadStringAsync(), System.Text.Encoding.UTF8);
                        }
                    }
                    break;
                default: return this.NotFound();
            }

            #endregion

            #region benchmark

            if ("benchmark" == c)
            {
                var arg = d.TryJsonDeserialize<DocUI.BenchmarkArg>();
                if (default(DocUI.BenchmarkArg).Equals(arg)) { return new ArgumentNullException(nameof(arg)).Message; }
                //arg.host = $"{this.Request.Scheme}://localhost:{this.HttpContext.Connection.LocalPort}/{business.Configer.Info.BusinessName}";
                arg.host = $"{Utils.Host.Addresses}/{business.Configer.Info.BusinessName}";
                return await DocUI.Benchmark(arg);
            }

            #endregion

            //this.Request.Headers["X-Real-IP"].FirstOrDefault() 

            var cmd = business.Command.GetCommand(
                //the cmd of this request.
                c,
                //the group of this request.
                g);

            if (null == cmd)
            {
                return Help.ErrorCmd(business, c);
            }

            var token = await business.GetToken(this.HttpContext, new Token //token
            {
                Origin = Token.OriginValue.Http,
                Key = t,
                Remote = string.Format("{0}:{1}", this.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(), this.HttpContext.Connection.RemotePort),
                Path = this.Request.Path.Value,
            });

            var result = null != route.Command && null != parameters ?
                    // Normal routing mode
                    await cmd.AsyncCall(
                        //the data of this request, allow null.
                        parameters,
                        //the incoming use object
                        new UseEntry(this, Utils.contextParameterNames), //context
                        new UseEntry(token, "session")) :
                    // Framework routing mode
                    await cmd.AsyncCall(
                        //the data of this request, allow null.
                        cmd.HasArgSingle ? new object[] { d } : d.TryJsonDeserializeStringArray(),
                        //the incoming use object
                        new UseEntry(this, Utils.contextParameterNames), //context
                        new UseEntry(token, "session"));

            return result;
        }
    }

    /// <summary>
    /// Business.AspNet
    /// </summary>
    public static class Utils
    {
        internal static BootstrapAll<BusinessBase> bootstrap;

        /// <summary>
        /// "context", "socket", "httpFile" 
        /// </summary>
        internal static readonly string[] contextParameterNames = new string[] { "context", "socket", "httpFile" };

        /// <summary>
        /// Default JSON format grouping
        /// </summary>
        public const string BusinessJsonGroup = "j";
        /// <summary>
        /// Default binary format grouping
        /// </summary>
        public const string BusinessSocketGroup = "s";

        /// <summary>
        /// local log path
        /// </summary>
        public static readonly string LocalLogPath = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

        /// <summary>
        /// result type
        /// </summary>
        public static readonly Type ResultType = typeof(ResultObject<>).GetGenericTypeDefinition();

        /// <summary>
        /// Environment instance
        /// </summary>
        public static Host Host = new Host();

        /// <summary>
        /// Log client
        /// </summary>
        public readonly static HttpClient LogClient;

        static Utils()
        {
            Console.WriteLine($"Date: {DateTime.Now}");
            Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            Console.WriteLine($"LocalLogPath: {LocalLogPath}");

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => (e.ExceptionObject as Exception)?.ExceptionWrite(true, true, LocalLogPath);
            //ThreadPool.SetMinThreads(50, 50);
            //ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            //ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);

            //Console.WriteLine($"Min {workerThreads}, {completionPortThreads}");
            //Console.WriteLine($"Max {workerThreads2}, {completionPortThreads2}");

            MessagePack.MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithResolver(MessagePack.Resolvers.CompositeResolver.Create(new MessagePack.Formatters.IMessagePackFormatter[] { new MessagePack.Formatters.IgnoreFormatter<Type>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodBase>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.PropertyInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.FieldInfo>() }, new MessagePack.IFormatterResolver[] { MessagePack.Resolvers.ContractlessStandardResolver.Instance }));

            Host.HttpClientFactory = new ServiceCollection()
                .AddHttpClient("any").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                }).Services
                .BuildServiceProvider().GetService<IHttpClientFactory>();
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            LogClient = Host.HttpClientFactory.CreateClient("log");
        }

        #region Call

        /// <summary>
        /// Called in "c,t,d" "application/x-www-form-urlencoded" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static async Task<string> Callctd(this HttpClient httpClient, string c, string t, string d) => await Call(httpClient, new KeyValuePair<string, string>("c", c), new KeyValuePair<string, string>("t", t), new KeyValuePair<string, string>("d", d));
        /// <summary>
        /// Called in "application/x-www-form-urlencoded" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public static async Task<string> Call(this HttpClient httpClient, params KeyValuePair<string, string>[] keyValues)
        {
            if (null == httpClient) { throw new ArgumentNullException(nameof(httpClient)); }
            if (null == keyValues) { throw new ArgumentNullException(nameof(keyValues)); }

            using (var content = new FormUrlEncodedContent(keyValues))
            {
                return await httpClient.Call(content);
            }
        }
        /// <summary>
        /// Called in "application/json" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="data"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static async Task<string> Call(this HttpClient httpClient, string data, string mediaType = "application/json")
        {
            if (null == httpClient) { throw new ArgumentNullException(nameof(httpClient)); }
            if (null == data) { throw new ArgumentNullException(nameof(data)); }

            using (var content = new StringContent(data, System.Text.Encoding.UTF8, mediaType))
            {
                return await httpClient.Call(content);
            }
        }
        /// <summary>
        /// HTTP call
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="content"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static async Task<string> Call(this HttpClient httpClient, HttpContent content, HttpMethod method = null)
        {
            using (var request = new HttpRequestMessage { Method = method ?? HttpMethod.Post, Content = content })
            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        #endregion

        /// <summary>
        /// Write out the default log
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task<string> Log(this HttpClient httpClient, Logger.LoggerData data, string index = "log", string c = "Write") => await httpClient.Callctd(c, null, new Log { Index = index, Data = data.ToString() }.JsonSerialize());

        /// <summary>
        /// Configure Business.Core in the startup class configure method
        /// <para>Injection context parameter name: "context", "socket", "httpFile"</para>
        /// <para>Injection token parameter name:"session"</para>
        /// </summary>
        /// <param name="app"></param>
        /// <param name="bootstrap"></param>
        /// <param name="docDir"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBusiness(this IApplicationBuilder app, BootstrapAll<BusinessBase> bootstrap = null, string docDir = "wwwroot")
        {
            if (null == app) { throw new ArgumentNullException(nameof(app)); }

            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

            Host.AppSettings = app.ApplicationServices.GetService<IConfiguration>().GetSection("AppSettings");

            Host.Addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault() ?? "http://localhost:5000";
            var addresses = Host.Addresses.ToLower();
            if (addresses.StartsWith("http://*:") || addresses.StartsWith("https://*:") || addresses.StartsWith("http://+:") || addresses.StartsWith("https://+:"))
            {
                Host.Addresses = Host.Addresses.Replace("*", "localhost").Replace("+", "localhost");
            }

            var staticDir = app.UseStaticDir(docDir);
            Console.WriteLine($"Static Directory: {staticDir}");
            Console.WriteLine($"Addresses: {Host.Addresses}");

            bootstrap = bootstrap ?? Bootstrap.CreateAll<BusinessBase>();
            bootstrap.UseType(contextParameterNames)
                .IgnoreSet(new Ignore(IgnoreMode.Arg), contextParameterNames)
                .LoggerSet(new LoggerAttribute(canWrite: false), contextParameterNames);

            if (null == bootstrap.Config.UseDoc)
            {
                bootstrap.UseDoc(staticDir, new Config { Debug = true, Benchmark = true, Group = BusinessJsonGroup });
            }

            if (string.IsNullOrWhiteSpace(bootstrap.Config.UseDoc.OutDir))
            {
                bootstrap.Config.UseDoc.OutDir = staticDir;
            }
            if (string.IsNullOrWhiteSpace(bootstrap.Config.UseDoc.Config.Group))
            {
                bootstrap.Config.UseDoc.Config.Group = BusinessJsonGroup;
            }
            if (string.IsNullOrWhiteSpace(bootstrap.Config.UseDoc.Config.Host))
            {
                bootstrap.Config.UseDoc.Config.Host = Host.Addresses;
            }

            bootstrap.Build();
            Utils.bootstrap = bootstrap;

            //writ url to page
            DocUI.Write(staticDir);

            //add route
            app.UseMvc(routes =>
            {
                foreach (var item in Configer.BusinessList)
                {
                    routes.MapRoute(
                    name: item.Key,
                    template: $"{item.Key}/{{*path}}",
                    defaults: new { controller = "Business", action = "Call" });
                }
            });

            /* 3.x
            services.AddControllers()

            app.UseEndpoints(endpoints =>
            {
                foreach (var item in Business.Core.Configer.BusinessList)
                {
                    endpoints.MapControllerRoute(item.Key, $"{item.Key}/{{*path}}", new { controller = "Business", action = "Call" });
                }
            });
            */
            #region AcceptWebSocket

            var webSocketcfg = Host.AppSettings.GetSection("WebSocket");
            SocketKeepAliveInterval = webSocketcfg.GetValue("KeepAliveInterval", SocketKeepAliveInterval);
            SocketReceiveBufferSize = webSocketcfg.GetValue("ReceiveBufferSize", SocketReceiveBufferSize);
            SocketMaxDegreeOfParallelism = webSocketcfg.GetValue("MaxDegreeOfParallelism", SocketMaxDegreeOfParallelism);
            //var allowedOrigins = webSocketcfg.GetSection("AllowedOrigins").GetChildren();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(SocketKeepAliveInterval),
                ReceiveBufferSize = SocketReceiveBufferSize
            };

            //foreach (var item in allowedOrigins)
            //{
            //    webSocketOptions.AllowedOrigins.Add(item.Value);
            //}

            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        Sockets.TryAdd(context.Connection.Id, webSocket);
#if DEBUG
                        Console.WriteLine($"Add:{context.Connection.Id} Sockets:{Sockets.Count}");
#endif
                        await Keep(context, webSocket);

                        Sockets.TryRemove(context.Connection.Id, out _);
#if DEBUG
                        Console.WriteLine($"Remove:{context.Connection.Id} Sockets:{Sockets.Count}");
#endif
                    }
                }
                else
                {
                    await next();
                }
            });

            #endregion

            return app;
        }

        static string UseStaticDir(this IApplicationBuilder app, string staticDir)
        {
            if (null == app) { throw new ArgumentNullException(nameof(app)); }
            if (string.IsNullOrWhiteSpace(staticDir)) { throw new ArgumentException("must have value", nameof(staticDir)); }

            var dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, staticDir);

            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // Set up custom content types -associating file extension to MIME type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            //provider.Mappings[".yaml"] = "text/yaml";
            provider.Mappings[".doc"] = "application/json";
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(dir),
                ContentTypeProvider = provider,
                OnPrepareResponse = c =>
                {
                    if (c.File.Exists && string.Equals(".doc", System.IO.Path.GetExtension(c.File.Name)))
                    {
                        //c.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=600"; //600
                        c.Context.Response.Headers[HeaderNames.CacheControl] = "public, no-cache, no-store";
                        c.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                        c.Context.Response.Headers[HeaderNames.Expires] = "-1";
                        //c.Context.Response.Headers[HeaderNames.CacheControl] = Configuration["StaticFiles:Headers:Cache-Control"];
                        //c.Context.Response.Headers[HeaderNames.Pragma] = Configuration["StaticFiles:Headers:Pragma"];
                        //c.Context.Response.Headers[HeaderNames.Expires] = Configuration["StaticFiles:Headers:Expires"];
                    }

                    c.Context.Response.Headers[HeaderNames.AccessControlAllowOrigin] = "*";
                }
            });

            return dir;
        }

        #region WebSocket

        /// <summary>
        /// 120
        /// </summary>
        public static int SocketKeepAliveInterval = 120;

        /// <summary>
        /// 4096
        /// </summary>
        public static int SocketReceiveBufferSize = 4096;

        /// <summary>
        /// -1
        /// </summary>
        public static int SocketMaxDegreeOfParallelism = -1;

        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> Sockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

        static async Task Keep(HttpContext context, WebSocket webSocket)
        {
            //var auth = context.Request.Headers["u"].ToString();

            //if (string.IsNullOrWhiteSpace(auth))
            //{
            //    return;
            //}

            var id = context.Connection.Id;

            try
            {
                var buffer = new byte[SocketReceiveBufferSize];
                WebSocketReceiveResult socketResult;
                do
                {
                    socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    try
                    {
                        var receiveData = MessagePack.MessagePackSerializer.Deserialize<ReceiveData>(buffer);

                        dynamic result;

                        //* test data
                        //receiveData.a = "API";
                        //receiveData.c = "Test001";
                        //receiveData.t = "token";
                        //receiveData.d = new Args.Test001 { A = "error", B = "bbb" }.BinarySerialize();
                        //receiveData.b = "bbb";
                        //*

                        if (string.IsNullOrWhiteSpace(receiveData.a) || !bootstrap.BusinessList.TryGetValue(receiveData.a, out BusinessBase business))
                        {
                            result = ResultType.ErrorBusiness(receiveData.a);
                            await SocketSendAsync(result.ToBytes(), id);
                        }
                        else
                        {
                            var b = receiveData.b ?? receiveData.c;

                            result = await business.Command.AsyncCall(
                            //the cmd of this request.
                            receiveData.c,
                            //the data of this request, allow null.
                            new object[] { receiveData.d },
                            //the group of this request.
                            BusinessSocketGroup, //fixed grouping
                                                 //the incoming use object
                            new UseEntry(context, "context"), //context
                            new UseEntry(webSocket, "socket"), //webSocket
                            new UseEntry(await business.GetToken(context, new Token //token
                            {
                                Origin = Token.OriginValue.WebSocket,
                                Key = receiveData.t,
                                Remote = string.Format("{0}:{1}", context.Connection.RemoteIpAddress.MapToIPv4().ToString(), context.Connection.RemotePort),
                                Callback = b,
                                //Path = this.Request.Path.Value,
                            }), "session")
                            );

                            // Socket set callback
                            if (!Equals(null, result))
                            {
                                if (typeof(IResult).IsAssignableFrom(result.GetType()))
                                {
                                    var result2 = result as IResult;
                                    result2.Callback = b;

                                    var data = result2.ResultCreateToDataBytes().ToBytes();

                                    await SocketSendAsync(data, id);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Help.ExceptionWrite(ex, true, true, LocalLogPath);
                        var result = ResultType.ResultCreate(0, Convert.ToString(ex));
                        await SocketSendAsync(result.ToBytes(), id);
                    }

                    if (webSocket.State != WebSocketState.Open)
                    {
                        break;
                    }
                } while (!socketResult.CloseStatus.HasValue);

                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(socketResult.CloseStatus.Value, socketResult.CloseStatusDescription, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Help.ExceptionWrite(ex, true, true, LocalLogPath);
                var result = ResultType.ResultCreate(0, Convert.ToString(ex));
                await SocketSendAsync(result.ToBytes(), id);
            }
        }

        #region SendAsync

        public static async Task SocketSendAsync(byte[] bytes, params string[] id) => await SocketSendAsync(bytes, WebSocketMessageType.Binary, true, id);

        public static async Task SocketSendAsync(byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, params string[] id) => await SocketSendAsync(bytes, messageType, true, id);

        public static async Task SocketSendAsync(byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true, params string[] id)
        {
            if (null == id || 0 == id.Length)
            {
                Parallel.ForEach(Sockets, new ParallelOptions { MaxDegreeOfParallelism = SocketMaxDegreeOfParallelism }, async c =>
                {
                    if (c.Value.State != WebSocketState.Open) { return; }

                    await c.Value.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
                });
            }
            else if (1 == id.Length)
            {
                var c = id[0];

                if (string.IsNullOrWhiteSpace(c)) { return; }

                if (!Sockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                if (webSocket.State != WebSocketState.Open) { return; }

                await webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
            }
            else
            {
                Parallel.ForEach(id, new ParallelOptions { MaxDegreeOfParallelism = SocketMaxDegreeOfParallelism }, async c =>
                {
                    if (string.IsNullOrWhiteSpace(c)) { return; }

                    if (!Sockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                    if (webSocket.State != WebSocketState.Open) { return; }

                    await webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
                });
            }
        }

        #endregion

        #endregion
    }
}

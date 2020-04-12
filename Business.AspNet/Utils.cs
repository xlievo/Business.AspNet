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
using Business.Core.Auth;
using Business.Core.Annotations;
using Business.Core.Result;
using Business.Core.Utils;
using Business.Core.Document;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpOverrides;
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

    readonly struct WebSocketReceive
    {
        public WebSocketReceive(string token, IReceiveData data, BusinessBase business, HttpContext context, WebSocket webSocket)
        {
            Token = token;
            Data = data;
            Business = business;
            Context = context;
            WebSocket = webSocket;
        }

        public string Token { get; }

        public IReceiveData Data { get; }

        public BusinessBase Business { get; }

        public HttpContext Context { get; }

        public WebSocket WebSocket { get; }
    }

    /// <summary>
    /// Business package
    /// </summary>
    public interface IReceiveData
    {
        /// <summary>
        /// business
        /// </summary>
        string a { get; set; }

        /// <summary>
        /// cmd
        /// </summary>
        string c { get; set; }

        //string t { get; set; }

        /// <summary>
        /// data
        /// </summary>
        byte[] d { get; set; }

        /// <summary>
        /// callback Default c
        /// </summary>
        string b { get; set; }
    }

    /// <summary>
    /// Business package
    /// </summary>
    public struct ReceiveData : IReceiveData
    {
        /// <summary>
        /// business
        /// </summary>
        public string a { get; set; }

        /// <summary>
        /// cmd
        /// </summary>
        public string c { get; set; }

        ///// <summary>
        ///// token
        ///// </summary>
        //public string t { get; set; }

        /// <summary>
        /// data
        /// </summary>
        public byte[] d { get; set; }

        /// <summary>
        /// callback Default c
        /// </summary>
        public string b { get; set; }
    }

    /// <summary>
    /// Deserialization of binary format
    /// </summary>
    public class MessagePackAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public MessagePackAttribute(int state = -13, string message = null) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "MessagePackArg Binary parsing";
            this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, IEnumerable<ArgumentAttribute> arguments) => !hasDefinition;
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

    /// <summary>
    /// Socket group
    /// </summary>
    public abstract class SocketGroupAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Socket group
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public SocketGroupAttribute(int state, string message) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Socket group";
            this.Group = Utils.BusinessWebSocketGroup;
            //this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, IEnumerable<ArgumentAttribute> arguments) => !hasDefinition;
        }
    }

    /// <summary>
    /// Json group
    /// </summary>
    public abstract class JsonGroupAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Json group
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public JsonGroupAttribute(int state, string message) : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Json group";
            this.Group = Utils.BusinessJsonGroup;
            //this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, IEnumerable<ArgumentAttribute> arguments) => !hasDefinition;
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
    public readonly struct Environment
    {
        /// <summary>
        /// Environment
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="appSettings"></param>
        /// <param name="httpClientFactory"></param>
        public Environment(string addresses, IConfigurationSection appSettings, IHttpClientFactory httpClientFactory)
        {
            Addresses = addresses;
            AppSettings = appSettings;
            HttpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// The urls the hosted application will listen on.
        /// </summary>
        public string Addresses { get; }

        /// <summary>
        /// Configuration file "AppSettings" node
        /// </summary>
        public IConfigurationSection AppSettings { get; }

        //public IHostingEnvironment ENV { get; set; }

        /// <summary>
        /// HttpClient factory
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; }
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
    /// Business base class for ASP.Net Core
    /// <para>fixed group: BusinessJsonGroup = j, BusinessWebSocketGroup = w</para>
    /// </summary>
    [Command(Group = Utils.BusinessJsonGroup)]
    [JsonArg(Group = Utils.BusinessJsonGroup)]
    [Command(Group = Utils.BusinessWebSocketGroup)]
    [MessagePack(Group = Utils.BusinessWebSocketGroup)]
    [Logger(Group = Utils.BusinessJsonGroup)]
    [Logger(Group = Utils.BusinessWebSocketGroup, ValueType = Logger.ValueType.Out)]
    public abstract class BusinessBase : BusinessBase<ResultObject<object>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BusinessBase() => this.Logger = new Logger(async (Logger.LoggerData x) => Help.Console(x.ToString()));

        /// <summary>
        /// Get the requested token
        /// </summary>
        /// <returns></returns>
        [Ignore]
        public virtual async ValueTask<IToken> GetToken(HttpContext context, Token token) => token;

        /// <summary>
        /// Accept a websocket connection. If null token is returned, it means reject, default string.Empty accept.
        /// <para>checked and return a token</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        [Ignore]
        public virtual async ValueTask<string> WebSocketAccept(HttpContext context, WebSocket webSocket) => string.Empty;

        /// <summary>
        /// Receive a websocket packet, return IReceiveData object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [Ignore]
        public virtual async ValueTask<IReceiveData> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer) => MessagePack.MessagePackSerializer.Deserialize<ReceiveData>(buffer);

        /// <summary>
        /// WebSocket dispose
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        [Ignore]
        public virtual async ValueTask WebSocketDispose(HttpContext context, WebSocket webSocket) { }
    }

    /// <summary>
    /// A class for an MVC controller with view support.
    /// </summary>
    [Use]
    //Internal object do not write logs
    [Logger(canWrite: false)]
    [RequestSizeLimit(long.MaxValue)]
    //int.MaxValue bug https://github.com/aspnet/AspNetCore/issues/13719
    //[RequestFormLimits(KeyLengthLimit = 1_009_100_000, ValueCountLimit = 1_009_100_000, ValueLengthLimit = 1_009_100_000, MultipartHeadersLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue, MultipartBoundaryLengthLimit = int.MaxValue)]
    [RequestFormLimits(KeyLengthLimit = int.MaxValue, ValueCountLimit = int.MaxValue, ValueLengthLimit = int.MaxValue, MultipartHeadersLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue, MultipartBoundaryLengthLimit = int.MaxValue)]
    public class BusinessController : Controller
    {
        /// <summary>
        /// Call
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [EnableCors("any")]
        public virtual async ValueTask<dynamic> Call()
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
                arg.host = $"{Utils.Environment.Addresses}/{business.Configer.Info.BusinessName}";
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
        internal static BusinessBase businessFirst;

        /// <summary>
        /// "context", "socket", "httpFile" 
        /// </summary>
        internal static readonly string[] contextParameterNames = new string[] { "context", "socket", "httpFile" };

        /// <summary>
        /// Default JSON format grouping
        /// </summary>
        public const string BusinessJsonGroup = "j";
        /// <summary>
        /// Default WebSocket format grouping
        /// </summary>
        public const string BusinessWebSocketGroup = "w";
        ///// <summary>
        ///// Default binary format grouping
        ///// </summary>
        //public const string BusinessUDPGroup = "u";

        /// <summary>
        /// local log path
        /// </summary>
        public static readonly string LocalLogPath = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

        /// <summary>
        /// result type
        /// </summary>
        public static readonly Type ResultType = typeof(ResultObject<>).GetGenericTypeDefinition();

        /// <summary>
        /// Host environment instance
        /// </summary>
        public static Environment Environment;

        ///// <summary>
        ///// Log client
        ///// </summary>
        //public readonly static HttpClient LogClient;

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

            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            //LogClient = Environment.HttpClientFactory.CreateClient("log");
        }

        #region Call

        /// <summary>
        /// Called in POST "c,t,d" "application/x-www-form-urlencoded" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static async ValueTask<string> Callctd(this HttpClient httpClient, string c, string t, string d) => await Call(httpClient, new KeyValuePair<string, string>("c", c), new KeyValuePair<string, string>("t", t), new KeyValuePair<string, string>("d", d));
        /// <summary>
        /// Called in POST "application/x-www-form-urlencoded" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public static async ValueTask<string> Call(this HttpClient httpClient, params KeyValuePair<string, string>[] keyValues)
        {
            if (null == httpClient) { throw new ArgumentNullException(nameof(httpClient)); }
            if (null == keyValues) { throw new ArgumentNullException(nameof(keyValues)); }

            using (var content = new FormUrlEncodedContent(keyValues))
            {
                return await httpClient.Call(content);
            }
        }
        /// <summary>
        /// Called in POST "application/json" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="data"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static async ValueTask<string> Call(this HttpClient httpClient, string data, string mediaType = "application/json")
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
        /// <param name="httpClient">httpClient</param>
        /// <param name="content">content</param>
        /// <param name="method">Default POST</param>
        /// <returns></returns>
        public static async ValueTask<string> Call(this HttpClient httpClient, HttpContent content, HttpMethod method = null)
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
        /// Write out the Elasticsearch default log
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async ValueTask<string> Log(this HttpClient httpClient, Logger.LoggerData data, string index = "log", string c = "Write") => await httpClient.Callctd(c, null, new Log { Index = index, Data = data.ToString() }.JsonSerialize());

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

            var addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault() ?? "http://localhost:5000";
            var addresses2 = addresses.ToLower();
            if (addresses2.StartsWith("http://*:") || addresses2.StartsWith("https://*:") || addresses2.StartsWith("http://+:") || addresses2.StartsWith("https://+:"))
            {
                addresses = addresses.Replace("*", "localhost").Replace("+", "localhost");
            }

            Environment = new Environment(addresses, app.ApplicationServices.GetService<IConfiguration>().GetSection("AppSettings"), new ServiceCollection()
                .AddHttpClient("any").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                }).Services
                .BuildServiceProvider().GetService<IHttpClientFactory>());

            var staticDir = app.UseStaticDir(docDir);
            Console.WriteLine($"Static Directory: {staticDir}");
            Console.WriteLine($"Addresses: {Environment.Addresses}");

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
                bootstrap.Config.UseDoc.Config.Host = Environment.Addresses;
            }

            bootstrap.Build();
            Utils.bootstrap = bootstrap;
            businessFirst = bootstrap.BusinessList.FirstOrDefault().Value;

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

            // 3.x
            //services.AddControllers()

            //app.UseEndpoints(endpoints =>
            //{
            //    foreach (var item in Business.Core.Configer.BusinessList)
            //    {
            //        endpoints.MapControllerRoute(item.Key, $"{item.Key}/{{*path}}", new { controller = "Business", action = "Call" });
            //    }
            //});

            #region AcceptWebSocket

            var webSocketcfg = Environment.AppSettings.GetSection("WebSocket");
            SocketKeepAliveInterval = webSocketcfg.GetValue("KeepAliveInterval", SocketKeepAliveInterval);
            SocketReceiveBufferSize = webSocketcfg.GetValue("ReceiveBufferSize", SocketReceiveBufferSize);
            //SocketMaxDegreeOfParallelism = webSocketcfg.GetValue("MaxDegreeOfParallelism", SocketMaxDegreeOfParallelism);
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
                        await Keep(context, webSocket);
                    }
                }
                else
                {
                    await next();
                }
            });

            //Task.Factory.StartNew(() =>
            //{
            //    foreach (var item in WebSocketQueue.GetConsumingEnumerable())
            //    {
            //        Task.Run(async () => await WebSocketCall(item).ContinueWith(c => c.Exception?.Console()));
            //    }
            //}, TaskCreationOptions.LongRunning);

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
        public static int SocketReceiveBufferSize = 4 * 1024;

        /// <summary>
        /// -1
        /// </summary>
        //public static int SocketMaxDegreeOfParallelism = -1;

        ///// <summary>
        ///// WebSocket dictionary
        ///// </summary>
        //public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> WebSockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

        //static readonly System.Collections.Concurrent.BlockingCollection<WebSocketReceive> WebSocketQueue = new System.Collections.Concurrent.BlockingCollection<WebSocketReceive>();

        static async ValueTask<dynamic> WebSocketCall(WebSocketReceive receive)
        {
            var b = receive.Data.b ?? receive.Data.c;

            var result = await receive.Business.Command.AsyncCall(
            //the cmd of this request.
            receive.Data.c,
            //the data of this request, allow null.
            new object[] { receive.Data.d },
            //the group of this request.
            BusinessWebSocketGroup, //fixed grouping
                                    //the incoming use object
            new UseEntry(receive.Context, "context"), //context
            new UseEntry(receive.WebSocket, "socket"), //webSocket
            new UseEntry(await receive.Business.GetToken(receive.Context, new Token //token
            {
                Origin = Token.OriginValue.WebSocket,
                Key = receive.Token,
                //Key = System.Text.Encoding.UTF8.GetString(receiveData.t),
                Remote = string.Format("{0}:{1}", receive.Context.Connection.RemoteIpAddress.MapToIPv4().ToString(), receive.Context.Connection.RemotePort),
                Callback = b,
                Path = receive.Context.Request.Path.Value,
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

                    await receive.WebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
                    //await SocketSendAsync(data, receive.Context.Connection.Id);
                }
            }

            return result;
        }

        static async ValueTask Keep(HttpContext context, WebSocket webSocket)
        {
            if (null == businessFirst)
            {
                return;
            }

            //var id = context.Connection.Id;
            var acceptBusiness = businessFirst;

            try
            {
                string token = null;
                var hasBusiness = true;
                var a = context.Request.Headers["a"].ToString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    hasBusiness = bootstrap.BusinessList.TryGetValue(a, out acceptBusiness);
                }

                if (hasBusiness)
                {
                    token = await acceptBusiness.WebSocketAccept(context, webSocket);
                }

                if (null == token)
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "(1007) The client or server is terminating the connection because it has received data inconsistent with the message type.", CancellationToken.None);
                    }

                    return;
                }

                //                WebSockets.TryAdd(id, webSocket);
                //#if DEBUG
                //                Console.WriteLine($"Add:{id} Connections:{WebSockets.Count}");
                //#endif

                var buffer = new byte[SocketReceiveBufferSize];
                WebSocketReceiveResult socketResult = null;

                do
                {
                    socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    try
                    {
                        var receiveData = await acceptBusiness.WebSocketReceive(context, webSocket, buffer);

                        if (string.IsNullOrWhiteSpace(receiveData.a) || !bootstrap.BusinessList.TryGetValue(receiveData.a, out BusinessBase business))
                        {
                            await webSocket.SendAsync(new ArraySegment<byte>(ResultType.ErrorBusiness(receiveData.a).ToBytes()), WebSocketMessageType.Binary, true, CancellationToken.None);
                        }
                        else
                        {
                            //WebSocketQueue.TryAdd(new WebSocketReceive(token, receiveData, business, context, webSocket));

                            Task.Factory.StartNew(async c => await WebSocketCall((WebSocketReceive)c).AsTask().ContinueWith(c2 => c2.Exception?.Console()), new WebSocketReceive(token, receiveData, business, context, webSocket));
                        }
                    }
                    catch (Exception ex)
                    {
                        Help.ExceptionWrite(ex, true, true, LocalLogPath);
                        var result = ResultType.ResultCreate(0, Convert.ToString(ex));
                        await webSocket.SendAsync(new ArraySegment<byte>(result.ToBytes()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }

                    if (webSocket.State != WebSocketState.Open)
                    {
                        break;
                    }
                } while (!socketResult.CloseStatus.HasValue);

                if (webSocket.State == WebSocketState.Open)
                {
                    //client close
                    await webSocket.CloseAsync(socketResult.CloseStatus.Value, socketResult.CloseStatusDescription, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Help.ExceptionWrite(ex, true, true, LocalLogPath);
                //var result = ResultType.ResultCreate(0, Convert.ToString(ex));
                //await SocketSendAsync(result.ToBytes(), id);
                if (webSocket.State == WebSocketState.Open)
                {
                    //server close
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, Convert.ToString(ex), CancellationToken.None);
                }
            }
            finally
            {
                //                WebSockets.TryRemove(id, out _);
                //#if DEBUG
                //                Console.WriteLine($"Remove:{id} Connectionss:{WebSockets.Count}");
                //#endif
                await acceptBusiness.WebSocketDispose(context, webSocket);
            }
        }

        #region SendAsync

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SocketSendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, params string[] id) => await SocketSendAsync(webSockets, bytes, WebSocketMessageType.Binary, true, -1, id);

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="sendMaxDegreeOfParallelism"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SocketSendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, int sendMaxDegreeOfParallelism = -1, params string[] id) => await SocketSendAsync(webSockets, bytes, WebSocketMessageType.Binary, true, sendMaxDegreeOfParallelism, id);

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="messageType"></param>
        /// <param name="sendMaxDegreeOfParallelism"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SocketSendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, int sendMaxDegreeOfParallelism = -1, params string[] id) => await SocketSendAsync(webSockets, bytes, messageType, true, sendMaxDegreeOfParallelism, id);

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="sendMaxDegreeOfParallelism"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SocketSendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true, int sendMaxDegreeOfParallelism = -1, params string[] id)
        {
            if (null == id || 0 == id.Length)
            {
                Parallel.ForEach(webSockets, new ParallelOptions { MaxDegreeOfParallelism = sendMaxDegreeOfParallelism }, async c =>
                {
                    if (c.Value.State != WebSocketState.Open) { return; }

                    await c.Value.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
                });
            }
            else if (1 == id.Length)
            {
                var c = id[0];

                if (string.IsNullOrWhiteSpace(c)) { return; }

                if (!webSockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                if (webSocket.State != WebSocketState.Open) { return; }

                await webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
            }
            else
            {
                Parallel.ForEach(id, new ParallelOptions { MaxDegreeOfParallelism = sendMaxDegreeOfParallelism }, async c =>
                {
                    if (string.IsNullOrWhiteSpace(c)) { return; }

                    if (!webSockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                    if (webSocket.State != WebSocketState.Open) { return; }

                    await webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage, CancellationToken.None);
                });
            }
        }

        #endregion

        #endregion
    }
}

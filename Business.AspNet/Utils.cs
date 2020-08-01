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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting.Server;
using static Business.AspNet.LogOptions;

namespace Business.AspNet
{
    #region Socket Support

    /*
    public class BusinessInfoFormatter : MessagePack.Formatters.IMessagePackFormatter<IBusinessInfo>
    {
        public static readonly BusinessInfoFormatter Instance = new BusinessInfoFormatter();

        public IBusinessInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (!reader.TryReadArrayHeader(out _)) { return null; }
            return new BusinessInfo { BusinessName = reader.ReadString(), Command = reader.ReadString() };
        }

        public void Serialize(ref MessagePackWriter writer, IBusinessInfo value, MessagePackSerializerOptions options)
        {
            if (null == value) { return; }
            writer.WriteArrayHeader(2);
            writer.Write(value.BusinessName);
            writer.Write(value.Command);
        }
    }

    public interface IBusinessInfo
    {
        /// <summary>
        /// Business to call
        /// </summary>
        string BusinessName { get; }

        /// <summary>
        /// Command to call
        /// </summary>
        string Command { get; }
    }

    */

    /// <summary>
    /// socket receive send
    /// </summary>
    public interface ISocket<Type>
    {
        /// <summary>
        /// Business
        /// </summary>
        BusinessInfo Business { get; set; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        Type Data { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        string Callback { get; set; }

        /// <summary>
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <returns></returns>
        byte[] ToBytes();
    }

    /// <summary>
    /// socket receive send
    /// </summary>
    public readonly struct BusinessInfo
    {
        /// <summary>
        /// Null
        /// </summary>
        public readonly static BusinessInfo Null = default;

        /// <summary>
        /// socket receive send
        /// </summary>
        /// <param name="businessName"></param>
        /// <param name="command"></param>
        public BusinessInfo(string businessName, string command)
        {
            BusinessName = businessName;
            Command = command;
            key = (!string.IsNullOrWhiteSpace(businessName) || !string.IsNullOrWhiteSpace(command)) ? $"{businessName?.ToLower()}{command?.ToLower()}" : null;
        }

        /// <summary>
        /// Business to call
        /// </summary>
        public string BusinessName { get; }

        /// <summary>
        /// Command to call
        /// </summary>
        public string Command { get; }

        readonly string key;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) => GetHashCode().Equals(obj.GetHashCode());

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => key.GetHashCode();
    }

    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct ResultObject<Type> : IResult<Type>, ISocket<Type>
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
        /// <param name="hasDataResult"></param>
        public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true, bool hasDataResult = false)
        {
            this.DataType = dataType;
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = checkData && !Equals(null, data);

            this.Callback = null;
            this.Business = default;
            this.GenericDefinition = genericDefinition;
            this.HasDataResult = hasDataResult;
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
            this.Business = default;
            this.DataType = null;
            this.GenericDefinition = null;
            this.HasDataResult = false;
        }

        /// <summary>
        /// The results of the state is greater than or equal to 1: success, equal to 0: system level exceptions, less than 0: business class error.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("S")]
        [Newtonsoft.Json.JsonProperty("S")]
        public int State { get; set; }

        /// <summary>
        /// Success can be null
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        [Newtonsoft.Json.JsonProperty("M")]
        public string Message { get; set; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get => Data; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("D")]
        [Newtonsoft.Json.JsonProperty("D")]
        public Type Data { get; set; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("H")]
        [Newtonsoft.Json.JsonProperty("H")]
        public bool HasData { get; set; }

        /// <summary>
        /// Gets the token of this result, used for callback
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string Callback { get; set; }

        /// <summary>
        /// Data type
        /// </summary>
        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public System.Type DataType { get; set; }

        /// <summary>
        /// Result object generic definition
        /// </summary>
        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public System.Type GenericDefinition { get; }

        /// <summary>
        /// Return data or not
        /// </summary>
        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool HasDataResult { get; }

        /// <summary>
        /// Business info
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public BusinessInfo Business { get; set; }

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
        public byte[] ToBytes() => this.MessagePackSerialize();

        /// <summary>
        /// ProtoBuf format Data
        /// </summary>
        /// <returns></returns>
        public byte[] ToDataBytes() => this.Data.MessagePackSerialize();
    }

    ///// <summary>
    ///// socket receive send
    ///// </summary>
    //public struct SocketObject : ISocket
    //{
    //    /// <summary>
    //    /// Business to call
    //    /// </summary>
    //    public string Business { get; set; }

    //    /// <summary>
    //    /// Commands to call
    //    /// </summary>
    //    public string Command { get; set; }

    //    /// <summary>
    //    /// Specific Byte/Json data objects
    //    /// </summary>
    //    public byte[] Data { get; set; }

    //    /// <summary>
    //    /// Gets the token of this result, used for callback
    //    /// </summary>
    //    public string Callback { get; set; }

    //    /// <summary>
    //    /// ProtoBuf format
    //    /// </summary>
    //    /// <returns></returns>
    //    public byte[] ToBytes() => this.MessagePackSerialize();
    //}

    readonly struct WebSocketReceive
    {
        public WebSocketReceive(IToken token, ISocket<byte[]> result, IBusiness business, WebSocket webSocket)
        {
            Token = token;
            Result = result;
            Business = business;
            //Context = context;
            WebSocket = webSocket;
        }

        public IToken Token { get; }

        public ISocket<byte[]> Result { get; }

        public IBusiness Business { get; }

        //public HttpContext Context { get; }

        public WebSocket WebSocket { get; }
    }

    /// <summary>
    /// Push
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PushAttribute : GroupAttribute
    {
        /// <summary>
        /// Push
        /// </summary>
        /// <param name="key"></param>
        public PushAttribute(string key = null) => Key = key;

        /// <summary>
        /// key
        /// </summary>
        public string Key { get; }
    }

    /// <summary>
    /// Deserialization of binary format
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
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
            this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, IEnumerable<ArgumentAttribute> arguments) => !hasDefinition && !this.ArgMeta.Arg.HasCollection;
        }

        /// <summary>
        /// Check whether the defined value type is the default value, (top-level object commit), Default true
        /// </summary>
        public bool CheckValueType { get; set; } = true;

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

            result = CheckDefinitionValueType(this, value, CheckValueType);
            if (!Equals(null, result)) { return result; }

            try
            {
                return this.ResultCreate(MessagePack.MessagePackSerializer.Deserialize<Type>(value));
            }
            catch (Exception ex) { return this.ResultCreate(State, Message ?? $"Arguments {this.Alias} MessagePack deserialize error. {ex.Message}"); }
        }
    }

    ///// <summary>
    ///// WebSocket grouping
    ///// </summary>
    //public abstract class WebSocketGroupAttribute : ArgumentAttribute
    //{
    //    /// <summary>
    //    /// Socket group
    //    /// </summary>
    //    /// <param name="state"></param>
    //    /// <param name="message"></param>
    //    public WebSocketGroupAttribute(int state, string message) : base(state, message)
    //    {
    //        this.CanNull = false;
    //        this.Description = "WebSocket group";
    //        this.Group = Utils.BusinessWebSocketGroup;
    //        //this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, IEnumerable<ArgumentAttribute> arguments) => !hasDefinition;
    //    }
    //}

    /// <summary>
    /// WebSocket command
    /// </summary>
    public class WebSocketCommandAttribute : CommandAttribute
    {
        /// <summary>
        /// Command attribute on a method, for multiple sources to invoke the method
        /// </summary>
        /// <param name="onlyName"></param>
        public WebSocketCommandAttribute(string onlyName = null) : base(onlyName) => base.Group = Utils.GroupWebSocket;

        /// <summary>
        /// Used for the command group
        /// </summary>
        public new string Group { get => base.Group; }
    }

    /// <summary>
    /// Json command
    /// </summary>
    public class JsonCommandAttribute : CommandAttribute
    {
        /// <summary>
        /// Command attribute on a method, for multiple sources to invoke the method
        /// </summary>
        /// <param name="onlyName"></param>
        public JsonCommandAttribute(string onlyName = null) : base(onlyName) => base.Group = Utils.GroupJson;

        /// <summary>
        /// Used for the command group
        /// </summary>
        public new string Group { get => base.Group; }
    }

    /// <summary>
    /// Simple asp.net HTTP request file
    /// </summary>
    [Use(typeof(Context))]
    [HttpFile]
    public class HttpFile : Dictionary<string, IFormFile>
    {
        /// <summary>
        /// GetFileAsync
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async ValueTask<byte[]> GetFileAsync(string name)
        {
            if (null == name || !this.TryGetValue(name, out IFormFile formFile))
            {
                return null;
            }

            return await formFile.OpenReadStream().StreamCopyByteAsync();
        }
    }

    /// <summary>
    /// Simple asp.net HTTP request file attribute
    /// </summary>
    public class HttpFileAttribute : Core.Annotations.HttpFileAttribute
    {
        /// <summary>
        /// Simple asp.net HTTP request file attribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public HttpFileAttribute(int state = 830, string message = null) : base(state, message) { }

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            Context context = value;

            if (Equals(null, context) || !context.Request.HasFormContentType)
            {
                return this.ResultCreate<Type>(default);
            }

            var httpFile = new HttpFile();

            foreach (var item in context.Request.Form.Files)
            {
                if (!httpFile.ContainsKey(item.Name))
                {
                    httpFile.Add(item.Name, item);
                }
            }

            return this.ResultCreate(httpFile);
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
    /// LogOptions
    /// </summary>
    public class LogOptions
    {
        /// <summary>
        /// Logo
        /// </summary>
        public bool Logo { get; set; }

        /// <summary>
        /// StartupInfo
        /// </summary>
        public bool StartupInfo { get; set; }

        /// <summary>
        /// Log
        /// </summary>
        public Action<LogData> Log { get; set; }

        /// <summary>
        /// LogData
        /// </summary>
        public readonly struct LogData
        {
            /// <summary>
            /// LogData
            /// </summary>
            /// <param name="type"></param>
            /// <param name="message"></param>
            public LogData(LogType type, string message)
            {
                Type = type;
                Message = message;
            }

            /// <summary>
            /// Type
            /// </summary>
            public LogType Type { get; }

            /// <summary>
            /// Message
            /// </summary>
            public string Message { get; }
        }

        /// <summary>
        /// LogType
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// Error
            /// </summary>
            Error = -1,
            /// <summary>
            /// Exception
            /// </summary>
            Exception = 0,
            /// <summary>
            /// Info
            /// </summary>
            Info = 1,
        }
    }

    /// <summary>
    /// Environment
    /// </summary>
    public class Hosting
    {
        /// <summary>
        /// Provides information about the web hosting environment an application is running in.
        /// </summary>
        public Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment { get; internal set; }

        /// <summary>
        /// The urls the hosted application will listen on.
        /// </summary>
        public string[] Addresses { get; internal set; }

        /// <summary>
        /// Configuration file "appsettings.json"
        /// </summary>
        public IConfiguration Config { get; internal set; }

        /// <summary>
        /// HttpClient factory
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; internal set; }

        /// <summary>
        /// Combine(DirectorySeparatorChar + data + AppDomain.CurrentDomain.FriendlyName.log.txt)
        /// </summary>
        public string LocalLogPath { get; set; } = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

        /// <summary>
        /// result type
        /// </summary>
        public Type ResultType { get; internal set; } = typeof(ResultObject<>).GetGenericTypeDefinition();

        internal Action<LogData> log = c => Help.WriteLocal(c.Message, Utils.Hosting.LocalLogPath, console: true);

        internal bool useWebSocket;

        internal readonly System.Collections.Concurrent.BlockingCollection<WebSocketData> webSocketSendQueue = new System.Collections.Concurrent.BlockingCollection<WebSocketData>();

        internal readonly struct WebSocketData
        {
            internal WebSocketData(WebSocket webSocket, ArraySegment<byte> data, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true)
            {
                WebSocket = webSocket;
                Data = data;
                MessageType = messageType;
                EndOfMessage = endOfMessage;
            }

            public WebSocket WebSocket { get; }

            public ArraySegment<byte> Data { get; }

            public WebSocketMessageType MessageType { get; }

            public bool EndOfMessage { get; }
        }

        /// <summary>
        /// Configuration options for the WebSocketMiddleware
        /// </summary>
        internal readonly WebSocketOptions webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120),
            ReceiveBufferSize = 4 * 1024
        };

        internal Action<Server> useServer;

        internal LogOptions logOptions;

        internal readonly RouteCTD routeCTD = new RouteCTD();

        internal Func<Type, string, dynamic, object> multipleParameterDeserialize = (parametersType, group, data) =>
        {
            switch (group)
            {
                case Utils.GroupJson:
                    return Help.TryJsonDeserialize(data, parametersType, Configer.JsonOptionsMultipleParameter);
                case Utils.GroupWebSocket:
                    return MessagePack.MessagePackSerializer.Deserialize(parametersType, data);
                default: return null;
            }
        };

        internal Action<System.Text.Json.JsonSerializerOptions> useJsonOptions;
        internal Action<Newtonsoft.Json.JsonSerializerSettings> useNewtonsoftJson;

        /// <summary>
        /// Socket type
        /// </summary>
        internal Type socketType = typeof(ResultObject<byte[]>);
    }

    /// <summary>
    /// Custom route "c", "t", "d" parameter name.
    /// </summary>
    public class RouteCTD
    {
        /// <summary>
        /// Default value "c". command
        /// </summary>
        public string C { get; set; } = "c";

        /// <summary>
        /// Default value "t". token
        /// </summary>
        public string T { get; set; } = "t";

        /// <summary>
        /// Default value "d". data
        /// </summary>
        public string D { get; set; } = "d";
    }

    /// <summary>
    /// Sets the specified limits to the Microsoft.AspNetCore.Http server.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Provides programmatic configuration of Kestrel-specific features.
        /// </summary>
        public KestrelServerOptions KestrelOptions { get; set; }

        /// <summary>
        /// Sets the specified limits to the Microsoft.AspNetCore.Http.HttpRequest.Form.
        /// </summary>
        public FormOptions FormOptions { get; set; }
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
    /// </summary>
    public interface IBusiness : Core.IBusiness
    {
        /// <summary>
        /// Get the requested token
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        ValueTask<IToken> GetToken(HttpContext context, Token token);

        /// <summary>
        /// Accept a websocket connection. If null token is returned, it means reject, default string.Empty accept.
        /// <para>checked and return a token</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        ValueTask<string> WebSocketAccept(HttpContext context, WebSocket webSocket);

        /// <summary>
        /// Receive a websocket packet, return IReceiveData object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        ValueTask<ISocket<byte[]>> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer);

        /// <summary>
        /// WebSocket dispose
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        ValueTask WebSocketDispose(HttpContext context, WebSocket webSocket);
    }

    /// <summary>
    /// Business base class for ASP.Net Core
    /// <para>fixed group: BusinessJsonGroup = j, BusinessWebSocketGroup = w</para>
    /// </summary>
    [Command(Group = Utils.GroupJson)]
    [JsonArg(Group = Utils.GroupJson)]
    [Command(Group = Utils.GroupWebSocket)]
    [MessagePack(Group = Utils.GroupWebSocket)]
    [Logger(Group = Utils.GroupJson)]
    [Logger(Group = Utils.GroupWebSocket, ValueType = Logger.ValueType.Out)]
    public abstract class BusinessBase : Core.BusinessBase, IBusiness
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
        public virtual async ValueTask<ISocket<byte[]>> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer) => (ISocket<byte[]>)MessagePack.MessagePackSerializer.Deserialize(Utils.Hosting.socketType, buffer);

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
    public class Context : Controller
    {
        /// <summary>
        /// Call
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        //[EnableCors("any")]
        public virtual async ValueTask<dynamic> Call()
        {
            #region route fixed grouping

            var g = Utils.GroupJson;//fixed grouping
            var path = this.Request.Path.Value.TrimStart('/');
            if (!(Configer.Routes.TryGetValue(path, out Configer.Route route) || Configer.Routes.TryGetValue($"{path}/{g}", out route)) || !Utils.bootstrap.BusinessList.TryGetValue(route.Business, out IBusiness business))
            {
                Utils.Hosting.log?.Invoke(new LogData(LogType.Error, $"404 {this.Request.Path.Value}"));
                return this.NotFound();
            }

            string c = null;
            string t = null;
            string d = null;
            string value = null;
            //g = route.Group;
            IDictionary<string, string> parameters = null;
            var ctd = Utils.Hosting.routeCTD;

            switch (this.Request.Method)
            {
                case "GET":
                    parameters = this.Request.Query.ToDictionary(k => k.Key, v =>
                    {
                        var v2 = (string)v.Value;
                        return !string.IsNullOrEmpty(v2) ? v2 : null;
                    });
                    c = route.Command ?? (parameters.TryGetValue(ctd.C, out value) ? value : null);
                    t = parameters.TryGetValue(ctd.T, out value) ? value : null;
                    d = parameters.TryGetValue(ctd.D, out value) ? value : null;
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
                            c = route.Command ?? (parameters.TryGetValue(ctd.C, out value) ? value : null);
                            t = parameters.TryGetValue(ctd.T, out value) ? value : null;
                            d = parameters.TryGetValue(ctd.D, out value) ? value : null;
                        }
                        else
                        {
                            c = route.Command;
                            d = System.Web.HttpUtility.UrlDecode(await this.Request.Body.StreamReadStringAsync(), System.Text.Encoding.UTF8);
                        }
                    }
                    break;
                default:
                    {
                        Utils.Hosting.log?.Invoke(new LogData(LogType.Error, $"404 {this.Request.Path.Value}"));
                        return this.NotFound();
                    }
            }

            #endregion

            #region benchmark

            if ("benchmark" == c)
            {
                var arg = d.TryJsonDeserialize<DocUI.BenchmarkArg>();
                if (default(DocUI.BenchmarkArg).Equals(arg))
                {
                    var argNull = new ArgumentNullException(nameof(arg));
                    Utils.Hosting.log?.Invoke(new LogData(LogType.Error, $"benchmark {argNull.Message}"));
                    return argNull.Message;
                }
                //arg.host = $"{this.Request.Scheme}://localhost:{this.HttpContext.Connection.LocalPort}/{business.Configer.Info.BusinessName}";
                if (null != Utils.httpAddress)
                {
                    //arg.host = $"{Utils.Environment.Addresses[0]}/{business.Configer.Info.BusinessName}";
                    arg.host = $"{Utils.httpAddress}/{business.Configer.Info.BusinessName}";
                }
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
                var errorCmd = Help.ErrorCmd(business, c);
                Utils.Hosting.log?.Invoke(new LogData(LogType.Error, $"ErrorCmd {errorCmd}"));
                return errorCmd;
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
                        //new UseEntry(this.HttpContext), //context
                        new UseEntry(this), //context
                        new UseEntry(token)) :
                    // Framework routing mode
                    await cmd.AsyncCall(
                        //the data of this request, allow null.
                        cmd.HasArgSingle ? new object[] { d } : cmd.GetParametersObjects(Utils.Hosting.multipleParameterDeserialize(cmd.ParametersType, g, d)),
                        //the incoming use object
                        //new UseEntry(this.HttpContext), //context
                        new UseEntry(this), //context
                        new UseEntry(token));

            //var dd = result.ToBytes();

            return result;
        }
    }

    /// <summary>
    /// Business.AspNet
    /// </summary>
    public static class Utils
    {
        internal static BootstrapAll<IBusiness> bootstrap;
        internal static IBusiness businessFirst;

        ///// <summary>
        ///// "context", "httpFile" 
        ///// </summary>
        //internal static readonly string[] contextParameterNames = new string[] { "context", "httpFile" };

        /// <summary>
        /// "Context", "HttpFile", "WebSocket" 
        /// </summary>
        internal static readonly Type[] contextParameterTypes = new Type[] { typeof(Context), typeof(HttpFile), typeof(WebSocket), typeof(HttpContext) };

        internal static string httpAddress = null;

        /// <summary>
        /// Default JSON format grouping
        /// </summary>
        public const string GroupJson = "j";
        /// <summary>
        /// Default WebSocket format grouping
        /// </summary>
        public const string GroupWebSocket = "w";
        ///// <summary>
        ///// Default binary format grouping
        ///// </summary>
        //public const string BusinessUDPGroup = "u";

        /// <summary>
        /// Host environment instance
        /// </summary>
        public static readonly Hosting Hosting = new Hosting
        {
            HttpClientFactory = new ServiceCollection()
                .AddHttpClient("any").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                }).Services
                .BuildServiceProvider().GetService<IHttpClientFactory>()
        };

        ///// <summary>
        ///// Log client
        ///// </summary>
        //public readonly static HttpClient LogClient;

        static Utils()
        {
            //Hosting.Exception = ex => ex?.ExceptionWrite(true, true, Hosting.LocalLogPath);

            //Console.WriteLine($"Date: {DateTimeOffset.Now}");
            //Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            //Console.WriteLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");

            //AppDomain.CurrentDomain.UnhandledException += (sender, e) => Hosting.Exception?.Invoke(e.ExceptionObject as Exception);
            //ThreadPool.SetMinThreads(50, 50);
            //ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            //ThreadPool.GetMaxThreads(out int workerThreads2, out int completionPortThreads2);

            //Console.WriteLine($"Min {workerThreads}, {completionPortThreads}");
            //Console.WriteLine($"Max {workerThreads2}, {completionPortThreads2}");

            MessagePack.MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithResolver(MessagePack.Resolvers.CompositeResolver.Create(new MessagePack.Formatters.IMessagePackFormatter[] { new MessagePack.Formatters.IgnoreFormatter<Type>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodBase>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.PropertyInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.FieldInfo>() }, new MessagePack.IFormatterResolver[] { MessagePack.Resolvers.ContractlessStandardResolver.Instance })).WithCompression(MessagePack.MessagePackCompression.Lz4Block);

            //AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            //Console.WriteLine("System.Net.Http.UseSocketsHttpHandler: false");

            //LogClient = Environment.HttpClientFactory.CreateClient("log");
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        static string Logo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.AppendLine("             ########");
            sb.AppendLine("            ##########");
            sb.AppendLine();
            sb.AppendLine("             ########");
            sb.AppendLine("            ##########");
            sb.AppendLine("          ##############");
            sb.AppendLine("         #######  #######");
            sb.AppendLine("        ######      ######");
            sb.AppendLine("        #####        #####");
            sb.AppendLine("        ####          ####");
            sb.AppendLine("        ####   ####   ####");
            sb.AppendLine("        #####  ####  #####");
            sb.AppendLine("         ################");
            sb.AppendLine("          ##############");
            var log = sb.ToString();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(log);
            Console.ResetColor();
            return log;
        }

        /// <summary>
        /// MessagePack serialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static byte[] MessagePackSerialize<Type>(this Type value, MessagePack.MessagePackSerializerOptions options = null) => MessagePack.MessagePackSerializer.Serialize(value, options);

        /// <summary>
        /// MessagePack serialize
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static byte[] MessagePackSerialize(this object value, Type type, MessagePack.MessagePackSerializerOptions options = null) => MessagePack.MessagePackSerializer.Serialize(type, value, options);

        /// <summary>
        /// MessagePack deserialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Type MessagePackDeserialize<Type>(this byte[] value, MessagePack.MessagePackSerializerOptions options = null) => MessagePack.MessagePackSerializer.Deserialize<Type>(value, options);

        /// <summary>
        /// MessagePack deserialize
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static object MessagePackDeserialize(this byte[] value, Type type, MessagePack.MessagePackSerializerOptions options = null) => MessagePack.MessagePackSerializer.Deserialize(type, value, options);

        /// <summary>
        /// ISocket byte[]
        /// </summary>
        /// <param name="business"></param>
        /// <param name="args"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static ISocket<byte[]> GetSocketObject(this IBusiness business, object[] args = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null)
        {
            var cmd = business.Command.GetCommand(method);

            if (null == cmd)
            {
                return default;
            }

            object arg = null;

            if (1 < args?.Length && !cmd.HasArgSingle)
            {
                arg = Activator.CreateInstance(cmd.ParametersType, args);
            }
            else if (0 < args?.Length && null != cmd.ParametersType)
            {
                arg = args[0];
            }

            var socket = Activator.CreateInstance(Hosting.socketType) as ISocket<byte[]>;

            socket.Business = new BusinessInfo(business.Configer.Info.BusinessName, (cmd.Meta.Attributes.FirstOrDefault(c => c is PushAttribute) as PushAttribute)?.Key ?? method);
            socket.Data = arg?.MessagePackSerialize(cmd.ParametersType);
            //socket.Callback = method;

            return socket;
        }

        #region HttpCall

        /// <summary>
        /// Called in POST "c,t,d" "application/x-www-form-urlencoded" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <param name="d"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<string> HttpCallctd(this HttpClient httpClient, string c, string t, string d, string uri = null, CancellationToken cancellationToken = default) => await HttpCall(httpClient, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("c", c), new KeyValuePair<string, string>("t", t), new KeyValuePair<string, string>("d", d) }, uri, cancellationToken);
        /// <summary>
        /// Called in POST "application/x-www-form-urlencoded" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="keyValues"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<string> HttpCall(this HttpClient httpClient, KeyValuePair<string, string>[] keyValues, string uri = null, CancellationToken cancellationToken = default)
        {
            if (null == httpClient) { throw new ArgumentNullException(nameof(httpClient)); }
            if (null == keyValues) { throw new ArgumentNullException(nameof(keyValues)); }

            using (var content = new FormUrlEncodedContent(keyValues))
            {
                return await httpClient.HttpCall(content, uri: null == uri ? null : new Uri(uri), cancellationToken: cancellationToken);
            }
        }
        /// <summary>
        /// Called in POST "application/json" format
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="data"></param>
        /// <param name="mediaType"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<string> HttpCall(this HttpClient httpClient, string data, string mediaType = "application/json", string uri = null, CancellationToken cancellationToken = default)
        {
            if (null == httpClient) { throw new ArgumentNullException(nameof(httpClient)); }
            if (null == data) { throw new ArgumentNullException(nameof(data)); }

            using (var content = new StringContent(data, System.Text.Encoding.UTF8, mediaType))
            {
                return await httpClient.HttpCall(content, uri: null == uri ? null : new Uri(uri), cancellationToken: cancellationToken);
            }
        }
        /// <summary>
        /// HTTP call
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="content"></param>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<string> HttpCall(this HttpClient httpClient, HttpContent content, HttpMethod method = null, Uri uri = null, CancellationToken cancellationToken = default)
        {
            using (var request = new HttpRequestMessage { Method = method ?? HttpMethod.Post, Content = content })
            {
                if (null != uri)
                {
                    request.RequestUri = uri;
                }

                /* .net 5.0 Microsoft.Extensions.Http.Polly ?
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(3));
                    using (var response = await httpClient.SendAsync(request, cts.Token))
                    {
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync(cts.Token);
                    }
                }
                */

                using (var response = await httpClient.SendAsync(request, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        #endregion

        #region ResultCreate

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="resultTypeDefinition"></param>
        /// <returns></returns>
        public static IResult<Data> ResultCreate<Data>(Data data = default, string message = null, int state = 1, Type resultTypeDefinition = null) => (resultTypeDefinition ?? Hosting.ResultType).ResultCreate(data, message, state);

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="resultTypeDefinition"></param>
        /// <returns></returns>
        public static IResult ResultCreate(int state = 1, string message = null, Type resultTypeDefinition = null) => (resultTypeDefinition ?? Hosting.ResultType).ResultCreate(state, message);

        #endregion

        /// <summary>
        /// Write out the Elasticsearch default log
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async ValueTask<string> Log(this HttpClient httpClient, Logger.LoggerData data, string index = "log", string c = "Write") => await httpClient.HttpCallctd(c, null, new Log { Index = index, Data = data.ToString() }.JsonSerialize());

        static void StartupInfo(string message)
        {
            if (null == Hosting.log)
            {
                Console.WriteLine(message);
            }
            else
            {
                Hosting.log.Invoke(new LogData(LogType.Info, message));
            }
        }

        /// <summary>
        /// Configure Business.Core in the startup class configure method
        /// <para>Injection context parameter type: "Context", "WebSocket", "HttpFile"</para>
        /// </summary>
        /// <param name="app">provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="logOptions">Output all non business exceptions or errors in the application</param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> CreateBusiness(this IApplicationBuilder app, Action<LogOptions> logOptions = null)
        {
            Hosting.logOptions = new LogOptions { StartupInfo = true, Logo = true };
            logOptions?.Invoke(Hosting.logOptions);
            Hosting.log = Hosting.logOptions.Log;// Log;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Hosting.log?.Invoke(new LogData(LogType.Exception, Convert.ToString((e.ExceptionObject as Exception)?.ExceptionWrite())));

            if (Hosting.logOptions.Logo)
            {
                Logo();
            }

            if (Hosting.logOptions.StartupInfo)
            {
                StartupInfo($"Date: {DateTimeOffset.Now}");
                StartupInfo(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
                StartupInfo($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
                StartupInfo($"LocalLogPath: {Hosting.LocalLogPath}");
            }

            //AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            //Console.WriteLine("System.Net.Http.UseSocketsHttpHandler: false");

            if (null == app) { throw new ArgumentNullException(nameof(app)); }

            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

            var addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Select(c =>
            {
                var address = c;
                var address2 = address.ToLower();
                if (address2.StartsWith("http://+:") || address2.StartsWith("http://*:") || address2.StartsWith("https://+:") || address2.StartsWith("https://*:"))
                {
                    address = address.Replace("*", "localhost").Replace("+", "localhost");
                }
                if (null == httpAddress && address2.StartsWith("http://"))
                {
                    httpAddress = address;
                }
                return address;
            }).ToArray();

            Hosting.Addresses = addresses;
            Hosting.Config = app.ApplicationServices.GetService<IConfiguration>();
            Hosting.Environment = app.ApplicationServices.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();

            //Console.WriteLine($"Addresses: {string.Join(" ", Hosting.Addresses)}");
            if (Hosting.logOptions.StartupInfo)
            {
                StartupInfo($"Addresses: {string.Join(" ", Hosting.Addresses)}");
            }

            bootstrap = Bootstrap.CreateAll<IBusiness>();

            bootstrap.Config.ResultType = typeof(ResultObject<>).GetGenericTypeDefinition();

            bootstrap.UseType(contextParameterTypes)
                .IgnoreSet(new Ignore(IgnoreMode.Arg), contextParameterTypes)
                .LoggerSet(new LoggerAttribute(canWrite: false), contextParameterTypes);

            bootstrap.Config.BuildBefore = strap =>
            {
                if (null != strap.Config.UseDoc)
                {
                    strap.Config.UseDoc.OutDir = strap.Config.UseDoc.OutDir ?? "wwwroot";

                    var documentDir = app.UseStaticDir(strap.Config.UseDoc.OutDir);
                    //Console.WriteLine($"Document Directory: {documentDir}");
                    if (Hosting.logOptions.StartupInfo)
                    {
                        StartupInfo($"Document Directory: {documentDir}");
                    }

                    strap.Config.UseDoc.OutDir = documentDir;

                    if (null == strap.Config.UseDoc.Options)
                    {
                        strap.Config.UseDoc.Options = new Options { Group = GroupJson, Debug = true, Benchmark = true };
                    }

                    if (string.IsNullOrWhiteSpace(strap.Config.UseDoc.Options.Group))
                    {
                        strap.Config.UseDoc.Options.Group = GroupJson;
                    }

                    //if (string.IsNullOrWhiteSpace(bootstrap.Config.UseDoc.Config.Host))
                    //{
                    //    if (0 < Environment.Addresses.Length)
                    //    {
                    //        bootstrap.Config.UseDoc.Config.Host = Environment.Addresses[0];
                    //    }
                    //}

                    strap.Config.UseDoc.Options.CamelCase = UseJson(app);

                    //writ url to page
                    DocUI.Write(documentDir, docFileName: Configer.documentFileName);
                }

                Hosting.ResultType = strap.Config.ResultType;
                Hosting.socketType = Hosting.ResultType.MakeGenericType(typeof(byte[]));
            };

            bootstrap.Config.BuildAfter = strap =>
            {
                businessFirst = bootstrap.BusinessList.FirstOrDefault().Value;

                if (null != Hosting.useServer)
                {
                    var contextFactory = app.ApplicationServices.GetService<IHttpContextFactory>();
                    var kestrelServer = app.ApplicationServices.GetService<IServer>() as KestrelServer;

                    Hosting.useServer(new Server { KestrelOptions = kestrelServer?.Options, FormOptions = contextFactory.GetType().GetField("_formOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(contextFactory) as FormOptions });
                }

                //add route
                app.UseMvc(routes =>
                {
                    foreach (var item in Configer.BusinessList)
                    {
                        routes.MapRoute(
                        name: item.Key,
                        template: $"{item.Key}/{{*path}}",
                        defaults: new { controller = "Context", action = "Call" });
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

                if (Hosting.useWebSocket)
                {
                    app.UseWebSockets(Hosting.webSocketOptions);

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

                    Task.Factory.StartNew(async () =>
                    {
                        foreach (var item in Hosting.webSocketSendQueue.GetConsumingEnumerable())
                        {
                            if (WebSocketState.Open == item.WebSocket?.State)
                            {
                                try
                                {
                                    await item.WebSocket?.SendAsync(item.Data, item.MessageType, item.EndOfMessage, CancellationToken.None);
                                }
                                catch (Exception ex)
                                {
                                    Hosting.log?.Invoke(new LogData(LogType.Exception, Convert.ToString(ex.ExceptionWrite())));
                                }
                            }
                        }
                    }, TaskCreationOptions.LongRunning);
                }

                //Task.Factory.StartNew(() =>
                //{
                //    foreach (var item in WebSocketQueue.GetConsumingEnumerable())
                //    {
                //        Task.Run(async () => await WebSocketCall(item).ContinueWith(c => c.Exception?.Console()));
                //    }
                //}, TaskCreationOptions.LongRunning);

                #endregion

                //AppDomain.CurrentDomain.UnhandledException += (sender, e) => Hosting.error?.Invoke(Convert.ToString((e.ExceptionObject as Exception)?.ExceptionWrite()));

                //return app;
            };

            return bootstrap;
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
            //var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            //provider.Mappings[".doc"] = "application/json";
            var options = new FileServerOptions();
            options.StaticFileOptions.FileProvider = new PhysicalFileProvider(dir);
            //options.StaticFileOptions.ContentTypeProvider = provider;
            options.StaticFileOptions.OnPrepareResponse = c =>
            {
                //if (c.File.Exists && string.Equals(".doc", System.IO.Path.GetExtension(c.File.Name)))
                if (c.File.Exists && Configer.documentFileName.Equals(c.File.Name))
                {
                    c.Context.Response.Headers[HeaderNames.CacheControl] = "public, no-cache, no-store";
                    c.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                    c.Context.Response.Headers[HeaderNames.Expires] = "-1";
                }

                c.Context.Response.Headers[HeaderNames.AccessControlAllowOrigin] = "*";
            };

            app.UseDefaultFiles().UseFileServer(options);

            return dir;
        }

        /// <summary>
        /// Configuration greater than contract.
        /// <para>"appsettings.json" -> "WebSocket": { "KeepAliveInterval": 120, "ReceiveBufferSize": 4096, "AllowedOrigins": [] }</para>
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseWebSocket(this BootstrapAll<IBusiness> bootstrap, Action<WebSocketOptions> options = null)
        {
            Hosting.useWebSocket = true;

            options?.Invoke(Hosting.webSocketOptions);

            //Configuration greater than contract
            var cfg = Hosting.Config.GetSection("WebSockets");

            var keepAliveInterval = cfg.GetValue("KeepAliveInterval", Hosting.webSocketOptions.KeepAliveInterval.TotalSeconds);
            Hosting.webSocketOptions.KeepAliveInterval = TimeSpan.FromSeconds(keepAliveInterval);

            Hosting.webSocketOptions.ReceiveBufferSize = cfg.GetValue("ReceiveBufferSize", Hosting.webSocketOptions.ReceiveBufferSize);

            var webSocketAllowedOrigins = cfg.GetSection("AllowedOrigins").Get<string[]>();

            if (null != webSocketAllowedOrigins)
            {
                foreach (var item in webSocketAllowedOrigins)
                {
                    Hosting.webSocketOptions.AllowedOrigins.Add(item);
                }
            }

            return bootstrap;
        }

        /// <summary>
        /// Configuration greater than contract.
        /// <para>"appsettings.json" -> "FormOptions": { "KeyLengthLimit": 2048, "ValueCountLimit": 1024 }</para>
        /// <para>"appsettings.json" -> "Kestrel": { "AllowSynchronousIO": true, "Limits": { "MinRequestBodyDataRate": null } }</para>
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseServer(this BootstrapAll<IBusiness> bootstrap, Action<Server> options = null)
        {
            Hosting.useServer = c =>
            {
                options?.Invoke(c);

                #region FormOptions

                var formOptions = c.FormOptions;
                var cfgFormOptions = Hosting.Config.GetSection("FormOptions");

                formOptions.BufferBody = cfgFormOptions.GetValue("BufferBody", formOptions.BufferBody);

                formOptions.BufferBodyLengthLimit = cfgFormOptions.GetValue("BufferBodyLengthLimit", formOptions.BufferBodyLengthLimit);

                formOptions.KeyLengthLimit = cfgFormOptions.GetValue("KeyLengthLimit", formOptions.KeyLengthLimit);

                formOptions.MemoryBufferThreshold = cfgFormOptions.GetValue("MemoryBufferThreshold", formOptions.MemoryBufferThreshold);

                formOptions.MultipartBodyLengthLimit = cfgFormOptions.GetValue("MultipartBodyLengthLimit", formOptions.MultipartBodyLengthLimit);

                formOptions.MultipartBoundaryLengthLimit = cfgFormOptions.GetValue("MultipartBoundaryLengthLimit", formOptions.MultipartBoundaryLengthLimit);

                formOptions.MultipartHeadersCountLimit = cfgFormOptions.GetValue("MultipartHeadersCountLimit", formOptions.MultipartHeadersCountLimit);

                formOptions.MultipartHeadersLengthLimit = cfgFormOptions.GetValue("MultipartHeadersLengthLimit", formOptions.MultipartHeadersLengthLimit);

                formOptions.ValueCountLimit = cfgFormOptions.GetValue("ValueCountLimit", formOptions.ValueCountLimit);

                formOptions.ValueLengthLimit = cfgFormOptions.GetValue("ValueLengthLimit", formOptions.ValueLengthLimit);

                #endregion

                //================================================//

                #region Kestrel

                //IIS
                if (null != c.KestrelOptions)
                {
                    var kestrelOptions = c.KestrelOptions;
                    var cfgKestrel = Hosting.Config.GetSection("Kestrel");

                    kestrelOptions.AllowSynchronousIO = cfgKestrel.GetValue("AllowSynchronousIO", kestrelOptions.AllowSynchronousIO);

                    //Method not found: 'Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal.SchedulingMode Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions.get_ApplicationSchedulingMode()'.
                    //kestrelOptions.ApplicationSchedulingMode = cfgKestrel.GetValue("ApplicationSchedulingMode", kestrelOptions.ApplicationSchedulingMode);

                    kestrelOptions.AddServerHeader = cfgKestrel.GetValue("AddServerHeader", kestrelOptions.AddServerHeader);

                    #region Limits

                    var cfgKestrelLimits = cfgKestrel.GetSection("Limits");

                    kestrelOptions.Limits.KeepAliveTimeout = cfgKestrelLimits.GetValue("KeepAliveTimeout", kestrelOptions.Limits.KeepAliveTimeout);

                    kestrelOptions.Limits.MaxConcurrentConnections = cfgKestrelLimits.GetValue("MaxConcurrentConnections", kestrelOptions.Limits.MaxConcurrentConnections);

                    kestrelOptions.Limits.MaxConcurrentUpgradedConnections = cfgKestrelLimits.GetValue("MaxConcurrentUpgradedConnections", kestrelOptions.Limits.MaxConcurrentUpgradedConnections);

                    kestrelOptions.Limits.MaxRequestBodySize = cfgKestrelLimits.GetValue("MaxRequestBodySize", kestrelOptions.Limits.MaxRequestBodySize);

                    kestrelOptions.Limits.MaxRequestBufferSize = cfgKestrelLimits.GetValue("MaxRequestBufferSize", kestrelOptions.Limits.MaxRequestBufferSize);

                    kestrelOptions.Limits.MaxRequestHeaderCount = cfgKestrelLimits.GetValue("MaxRequestHeaderCount", kestrelOptions.Limits.MaxRequestHeaderCount);

                    kestrelOptions.Limits.MaxRequestHeadersTotalSize = cfgKestrelLimits.GetValue("MaxRequestHeadersTotalSize", kestrelOptions.Limits.MaxRequestHeadersTotalSize);

                    kestrelOptions.Limits.MaxRequestLineSize = cfgKestrelLimits.GetValue("MaxRequestLineSize", kestrelOptions.Limits.MaxRequestLineSize);

                    kestrelOptions.Limits.MaxResponseBufferSize = cfgKestrelLimits.GetValue("MaxResponseBufferSize", kestrelOptions.Limits.MaxResponseBufferSize);

                    kestrelOptions.Limits.MinRequestBodyDataRate = cfgKestrelLimits.GetValue("MinRequestBodyDataRate", kestrelOptions.Limits.MinRequestBodyDataRate);

                    kestrelOptions.Limits.MinResponseDataRate = cfgKestrelLimits.GetValue("MinResponseDataRate", kestrelOptions.Limits.MinResponseDataRate);

                    kestrelOptions.Limits.RequestHeadersTimeout = cfgKestrelLimits.GetValue("RequestHeadersTimeout", kestrelOptions.Limits.RequestHeadersTimeout);

                    #region Http2

                    var cfgKestrelLimitsHttp2 = cfgKestrelLimits.GetSection("Http2");

                    kestrelOptions.Limits.Http2.HeaderTableSize = cfgKestrelLimitsHttp2.GetValue("RequestHeadersTimeout", kestrelOptions.Limits.Http2.HeaderTableSize);

                    kestrelOptions.Limits.Http2.InitialConnectionWindowSize = cfgKestrelLimitsHttp2.GetValue("InitialConnectionWindowSize", kestrelOptions.Limits.Http2.InitialConnectionWindowSize);

                    kestrelOptions.Limits.Http2.InitialStreamWindowSize = cfgKestrelLimitsHttp2.GetValue("InitialStreamWindowSize", kestrelOptions.Limits.Http2.InitialStreamWindowSize);

                    kestrelOptions.Limits.Http2.MaxFrameSize = cfgKestrelLimitsHttp2.GetValue("MaxFrameSize", kestrelOptions.Limits.Http2.MaxFrameSize);

                    kestrelOptions.Limits.Http2.MaxRequestHeaderFieldSize = cfgKestrelLimitsHttp2.GetValue("MaxRequestHeaderFieldSize", kestrelOptions.Limits.Http2.MaxRequestHeaderFieldSize);

                    kestrelOptions.Limits.Http2.MaxStreamsPerConnection = cfgKestrelLimitsHttp2.GetValue("MaxStreamsPerConnection", kestrelOptions.Limits.Http2.MaxStreamsPerConnection);

                    #endregion

                    #endregion
                }

                #endregion
            };

            return bootstrap;
        }

        /// <summary>
        /// Custom route "c", "t", "d" parameter name.
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseRouteCTD(this BootstrapAll<IBusiness> bootstrap, Action<RouteCTD> options = null)
        {
            options?.Invoke(Hosting.routeCTD);
            return bootstrap;
        }

        /// <summary>
        /// Deserialize of multiple parameters submitted
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseMultipleParameterDeserialize(this BootstrapAll<IBusiness> bootstrap, Func<Type, string, dynamic, object> options = null)
        {
            if (null != options)
            {
                Hosting.multipleParameterDeserialize = options;
            }
            return bootstrap;
        }

        /// <summary>
        /// Configures Microsoft.AspNetCore.Mvc.JsonOptions for the specified builder.
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options">An System.Action to configure the Microsoft.AspNetCore.Mvc.JsonOptions.</param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseJsonOptions(this BootstrapAll<IBusiness> bootstrap, Action<System.Text.Json.JsonSerializerOptions> options = null)
        {
            if (null != options)
            {
                Hosting.useJsonOptions = options;
            }
            return bootstrap;
        }
        /// <summary>
        /// UseNewtonsoftJson
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseNewtonsoftJson(this BootstrapAll<IBusiness> bootstrap, Action<Newtonsoft.Json.JsonSerializerSettings> options = null)
        {
            if (null != options)
            {
                Hosting.useNewtonsoftJson = options;
            }
            return bootstrap;
        }

        static Func<string, string> UseJson(IApplicationBuilder app)
        {
            Func<string, string> camelCase = null;
            //JsonSerializerOptions 77/140
            var jsonHelper = app.ApplicationServices.GetService<Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper>();
            var _htmlSafeJsonSerializerOptions = jsonHelper.GetType().GetField("_htmlSafeJsonSerializerOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (null != _htmlSafeJsonSerializerOptions)
            {
                var jsonSerializerOptions = _htmlSafeJsonSerializerOptions.GetValue(jsonHelper) as System.Text.Json.JsonSerializerOptions;
                if (null != jsonSerializerOptions.PropertyNamingPolicy)
                {
                    jsonSerializerOptions.PropertyNamingPolicy = Help.JsonNamingPolicyCamelCase.Instance;
                }
                Hosting.useJsonOptions?.Invoke(jsonSerializerOptions);
                camelCase = c => jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(c);
            }
            else
            {
                //var _defaultSettingsJsonSerializer = jsonHelper.GetType().GetField("_defaultSettingsJsonSerializer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //if (null != _defaultSettingsJsonSerializer)
                //{
                //    var jsonSerializerOptions = _defaultSettingsJsonSerializer.GetValue(jsonHelper) as Newtonsoft.Json.JsonSerializer;

                //    Hosting.useNewtonsoftJson?.Invoke(jsonSerializerOptions);
                //    jsonSerializerOptions.ContractResolver = null;
                //    //_defaultSettingsJsonSerializer.SetValue(jsonHelper, jsonSerializerOptions);

                //    var resolver = jsonSerializerOptions.ContractResolver as Newtonsoft.Json.Serialization.DefaultContractResolver;
                //    camelCase = c => resolver?.NamingStrategy?.GetPropertyName(c, false);
                //}
                //ResolvedServices = Count = 79
                var resolvedServices = app.ApplicationServices.GetType().GetProperty("ResolvedServices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(app.ApplicationServices) as System.Collections.IDictionary;

                foreach (System.Collections.DictionaryEntry item in resolvedServices)
                {
                    var type = item.Key.GetType().GetProperty("Type").GetValue(item.Key) as Type;

                    if (type.FullName.StartsWith("Microsoft.Extensions.Options.IOptions`1[[Microsoft.AspNetCore.Mvc.MvcNewtonsoftJsonOptions, Microsoft.AspNetCore.Mvc.NewtonsoftJson"))
                    {
                        dynamic value = item.Value;
                        Newtonsoft.Json.JsonSerializerSettings jsonSerializerOptions = value.Value.SerializerSettings;

                        if (jsonSerializerOptions?.ContractResolver is Newtonsoft.Json.Serialization.DefaultContractResolver resolver && null != resolver)
                        {
                            resolver.NamingStrategy = CamelCaseNamingStrategy.Instance;
                        }

                        Hosting.useNewtonsoftJson?.Invoke(jsonSerializerOptions);
                        resolver = jsonSerializerOptions?.ContractResolver as Newtonsoft.Json.Serialization.DefaultContractResolver;

                        camelCase = c => resolver?.NamingStrategy?.GetPropertyName(c, false);
                    }
                }
            }

            return camelCase;
        }

        class CamelCaseNamingStrategy : Newtonsoft.Json.Serialization.CamelCaseNamingStrategy
        {
            public static CamelCaseNamingStrategy Instance { get; } = new CamelCaseNamingStrategy();

            protected override string ResolvePropertyName(string name) => Help.CamelCase(name);
        }

        ///// <summary>
        ///// Use ISocket type object
        ///// </summary>
        ///// <typeparam name="Type"></typeparam>
        ///// <param name="bootstrap"></param>
        ///// <returns></returns>
        //public static BootstrapAll<IBusiness> UseSocketType<Type>(this BootstrapAll<IBusiness> bootstrap)
        //    where Type : ISocket, new()
        //{
        //    Hosting.socketType = typeof(Type);

        //    return bootstrap;
        //}

        #region WebSocket

        /// <summary>
        /// -1
        /// </summary>
        //public static int SocketMaxDegreeOfParallelism = -1;

        ///// <summary>
        ///// WebSockets
        ///// </summary>
        //public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> WebSockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

        static async Task WebSocketCall(WebSocketReceive receive)
        {
            var cmd = receive.Business.Command.GetCommand(receive.Result.Business.Command, GroupWebSocket);

            var result = await cmd.AsyncCall(
            //the data of this request, allow null.
            cmd.HasArgSingle ? new object[] { receive.Result.Data } : cmd.GetParametersObjects(Hosting.multipleParameterDeserialize(cmd.ParametersType, GroupWebSocket, receive.Result.Data)),
            //the incoming use object
            //new UseEntry(receive.HttpContext, "context"), //context
            new UseEntry(receive.WebSocket), //webSocket
            new UseEntry(receive.Token));

            // Socket set callback
            if (!Equals(null, result))
            {
                if (typeof(IResult).IsAssignableFrom(result.GetType()))
                {
                    var result2 = result as IResult;
                    result2.Callback = receive.Result.Callback ?? receive.Result.Business.Command;

                    receive.WebSocket.SendAsync(new ArraySegment<byte>(result2.ResultCreateToDataBytes().ToBytes()));
                }
            }
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

                var remote = string.Format("{0}:{1}", context.Connection.RemoteIpAddress.MapToIPv4().ToString(), context.Connection.RemotePort);

                var buffer = new byte[Hosting.webSocketOptions.ReceiveBufferSize];
                WebSocketReceiveResult socketResult = null;

                do
                {
                    socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    try
                    {
                        var receiveData = await acceptBusiness.WebSocketReceive(context, webSocket, buffer);

                        if (string.IsNullOrWhiteSpace(receiveData.Business.BusinessName) || !bootstrap.BusinessList.TryGetValue(receiveData.Business.BusinessName, out IBusiness business))
                        {
                            webSocket.SendAsync(new ArraySegment<byte>(Hosting.ResultType.ErrorBusiness(receiveData.Business.BusinessName).ToBytes()));
                        }
                        else
                        {
                            //WebSocketQueue.TryAdd(new WebSocketReceive(token, receiveData, business, context, webSocket));
                            Task.Factory.StartNew(async c => await WebSocketCall((WebSocketReceive)c).ContinueWith(c2 =>
                            {
                                if (null != c2.Exception)
                                {
                                    Hosting.log?.Invoke(new LogData(LogType.Exception, Convert.ToString(c2.Exception)));
                                }
                            })
                            , new WebSocketReceive(await business.GetToken(context, new Token //token
                            {
                                Origin = Token.OriginValue.WebSocket,
                                Key = token,
                                //Key = System.Text.Encoding.UTF8.GetString(receiveData.t),
                                Remote = remote,
                                Callback = receiveData.Callback ?? receiveData.Business.Command,
                                Path = context.Request.Path.Value,
                            }), receiveData, business, webSocket));
                        }
                    }
                    catch (Exception ex)
                    {
                        Hosting.log?.Invoke(new LogData(LogType.Exception, Convert.ToString(ex.ExceptionWrite())));
                        var result = Hosting.ResultType.ResultCreate(0, Convert.ToString(ex));
                        webSocket.SendAsync(new ArraySegment<byte>(result.ToBytes()));
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
                Hosting.log?.Invoke(new LogData(LogType.Exception, Convert.ToString(ex.ExceptionWrite())));
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
                await acceptBusiness.WebSocketDispose(context, webSocket);
            }
        }

        #region WebSocketSendAsync

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, params string[] id) => await SendAsync(webSockets, bytes, WebSocketMessageType.Binary, true, -1, id);

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="sendMaxDegreeOfParallelism"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, int sendMaxDegreeOfParallelism = -1, params string[] id) => await SendAsync(webSockets, bytes, WebSocketMessageType.Binary, true, sendMaxDegreeOfParallelism, id);

        /// <summary>
        /// Send socket message
        /// </summary>
        /// <param name="webSockets"></param>
        /// <param name="bytes"></param>
        /// <param name="messageType"></param>
        /// <param name="sendMaxDegreeOfParallelism"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask SendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, int sendMaxDegreeOfParallelism = -1, params string[] id) => await SendAsync(webSockets, bytes, messageType, true, sendMaxDegreeOfParallelism, id);

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
        public static async ValueTask SendAsync(this IDictionary<string, WebSocket> webSockets, byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true, int sendMaxDegreeOfParallelism = -1, params string[] id)
        {
            if (null == bytes) { return; }

            if (null == id || 0 == id.Length)
            {
                Parallel.ForEach(webSockets, new ParallelOptions { MaxDegreeOfParallelism = sendMaxDegreeOfParallelism }, c =>
                {
                    if (WebSocketState.Open != c.Value.State) { return; }

                    c.Value.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage);
                });
            }
            else if (1 == id.Length)
            {
                var c = id[0];

                if (string.IsNullOrWhiteSpace(c)) { return; }

                if (!webSockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                if (webSocket.State != WebSocketState.Open) { return; }

                webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage);
            }
            else
            {
                Parallel.ForEach(id, new ParallelOptions { MaxDegreeOfParallelism = sendMaxDegreeOfParallelism }, c =>
                {
                    if (string.IsNullOrWhiteSpace(c)) { return; }

                    if (!webSockets.TryGetValue(c, out WebSocket webSocket)) { return; }

                    if (webSocket.State != WebSocketState.Open) { return; }

                    webSocket.SendAsync(new ArraySegment<byte>(bytes), messageType, endOfMessage);
                });
            }
        }

        /// <summary>
        /// WebSocket SendAsync
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="data"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        public static void SendAsync(this WebSocket webSocket, ArraySegment<byte> data, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true) => Hosting.webSocketSendQueue.TryAdd(new Hosting.WebSocketData(webSocket, data, messageType, endOfMessage));

        #endregion

        #endregion
    }
}
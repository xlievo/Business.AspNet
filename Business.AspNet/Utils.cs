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
using System.Xml.Serialization;

namespace Business.AspNet
{
    #region Socket Support

    ///// <summary>
    ///// socket receive send
    ///// </summary>
    //public interface ISocket<Type>
    //{
    //    /// <summary>
    //    /// Business
    //    /// </summary>
    //    BusinessInfo Business { get; set; }

    //    /// <summary>
    //    /// Specific Byte/Json data objects
    //    /// </summary>
    //    Type Data { get; }

    //    /// <summary>
    //    /// Gets the token of this result, used for callback
    //    /// </summary>
    //    string Callback { get; set; }

    //    /// <summary>
    //    /// ProtoBuf,MessagePack or Other
    //    /// </summary>
    //    /// <returns></returns>
    //    byte[] ToBytes(bool dataBytes = true);
    //}

    /// <summary>
    /// socket receive send
    /// </summary>
    public interface IResultObject : IResult
    {
        /// <summary>
        /// Business
        /// </summary>
        BusinessInfo Business { get; set; }
    }

    /// <summary>
    /// socket receive send
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public interface IResultObject<Type> : IResultObject, IResult<Type> { }

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
    public struct ResultObject<Type> : IResultObject<Type>
    {
        /// <summary>
        /// ResultObject
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ResultObject<Type>(byte[] value) => value.ToResultObject<ResultObject<Type>>();

        /// <summary>
        /// Activator.CreateInstance
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <param name="genericDefinition"></param>
        /// <param name="checkData"></param>
        /// <param name="hasData"></param>
        /// <param name="hasDataResult"></param>
        public ResultObject(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasData = false, bool hasDataResult = false)
        {
            this.DataType = dataType;
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = checkData ? !Equals(null, data) : hasData;

            this.Callback = callback;
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
        /// <param name="hasData"></param>
        public ResultObject(Type data, int state, string message, bool hasData)
        {
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = hasData;

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
        public int State { get; }

        /// <summary>
        /// Success can be null
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        [Newtonsoft.Json.JsonProperty("M")]
        public string Message { get; }

        /// <summary>
        /// Specific dynamic data objects
        /// </summary>
        dynamic IResult.Data { get => Data; }

        /// <summary>
        /// Specific Byte/Json data objects
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("D")]
        [Newtonsoft.Json.JsonProperty("D")]
        public Type Data { get; }

        /// <summary>
        /// Whether there is value
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("H")]
        [Newtonsoft.Json.JsonProperty("H")]
        public bool HasData { get; }

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
        public System.Type DataType { get; }

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
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes(bool dataBytes = true) => dataBytes ? Utils.ResultCreate(GenericDefinition ?? typeof(ResultObject<>), HasData ? Data?.MessagePackSerialize() : default, Message, State, Callback, false, HasData, HasDataResult, Business).ToBytes(false) : this.MessagePackSerialize();
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
        public WebSocketReceive(IToken token, IResultObject<byte[]> result, IBusiness business, WebSocket webSocket)
        {
            Token = token;
            Result = result;
            Business = business;
            WebSocket = webSocket;
            //Context = context;
        }

        public IToken Token { get; }

        public IResultObject<byte[]> Result { get; }

        public IBusiness Business { get; }

        public WebSocket WebSocket { get; }

        //public HttpContext Context { get; }
    }

    /// <summary>
    /// WebSocketAcceptReply, ok, illegal
    /// </summary>
    public readonly struct WebSocketAcceptReply
    {
        /// <summary>
        /// WebSocketAcceptReply
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <param name="closeStatus"></param>
        public WebSocketAcceptReply(string token = null, string message = "ok", WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            Token = token;
            Message = message;//ok, illegal
            CloseStatus = closeStatus;
        }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// CloseStatus
        /// </summary>
        public WebSocketCloseStatus CloseStatus { get; }
    }

    #region UDP
    /*
    /// <summary>
    /// NAT
    /// </summary>
    public readonly struct NAT
    {
        /// <summary>
        /// NAT
        /// </summary>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <param name="address"></param>
        public NAT(int port, string token, string address = null)
        {
            Port = port;
            Token = token;
            Address = address;
        }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; }
    }

    /// <summary>
    /// IPEndPoint
    /// </summary>
    public readonly struct IPEndPoint
    {
        /// <summary>
        /// IPEndPoint
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public IPEndPoint(System.Net.IPAddress address, int port) : this(address?.GetAddressBytes(), port) { }

        /// <summary>
        /// IPEndPoint
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public IPEndPoint(byte[] address, int port)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Port = port;

            key = $"{BitConverter.ToString(Address)}:{Port}";
        }

        /// <summary>
        /// Address
        /// </summary>
        public byte[] Address { get; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; }

        readonly string key;

        /// <summary>
        /// ToEndPoint
        /// </summary>
        /// <returns></returns>
        public System.Net.IPEndPoint ToEndPoint() => new System.Net.IPEndPoint(new System.Net.IPAddress(Address), Port);
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

        /// <summary>
        /// Returns the IP address and port number of the specified endpoint.
        /// </summary>
        /// <returns>A string containing the IP address and the port number of the specified endpoint (for example, 192.168.1.2:80).</returns>
        public override string ToString() => ToEndPoint().ToString();
    }

    */
    #endregion

    /// <summary>
    /// Push method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PushAttribute : GroupAttribute
    {
        ///// <summary>
        ///// Push
        ///// </summary>
        ///// <param name="key"></param>
        //public PushAttribute(string key = null) => Key = key;

        ///// <summary>
        ///// key
        ///// </summary>
        //public string Key { get; }
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
            this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, IEnumerable<ArgumentAttribute> arguments, bool ignoreArg) => !hasDefinition && !this.ArgMeta.Arg.HasCollection || ignoreArg;
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
        public override ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return new ValueTask<IResult>(result); }

            result = CheckDefinitionValueType(this, value, CheckValueType);
            if (!Equals(null, result)) { return new ValueTask<IResult>(result); }

            try
            {
                return new ValueTask<IResult>(this.ResultCreate(Utils.MessagePackDeserialize<Type>(value)));
            }
            catch (Exception ex)
            {
                return new ValueTask<IResult>(this.ResultCreate(State, Message ?? $"Arguments {this.Alias} MessagePack deserialize error. {ex.Message}"));
            }
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
        public override ValueTask<IResult> Proces<Type>(dynamic value)
        {
            Context context = value;

            if (Equals(null, context) || !context.Request.HasFormContentType)
            {
                return new ValueTask<IResult>(this.ResultCreate<Type>(default));
            }

            var httpFile = new HttpFile();

            foreach (var item in context.Request.Form.Files)
            {
                if (!httpFile.ContainsKey(item.Name))
                {
                    httpFile.Add(item.Name, item);
                }
            }

            return new ValueTask<IResult>(this.ResultCreate(httpFile));
        }
    }

    #endregion

    /// <summary>
    /// NewtonsoftJsonArg
    /// </summary>
    public class NewtonsoftJsonArgAttribute : JsonArgAttribute
    {
        /// <summary>
        /// NewtonsoftJsonArg
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public NewtonsoftJsonArgAttribute(int state = -12, string message = null) : base(state, message) { }

        /// <summary>
        /// Settings
        /// </summary>
        readonly Newtonsoft.Json.JsonSerializerSettings newtonsoftJsonOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-ddTHH:mm:ss",
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = new List<Newtonsoft.Json.JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return new ValueTask<IResult>(result); }

            //Check whether the defined value type is the default value, (top-level object commit)
            result = CheckDefinitionValueType(this, value, CheckValueType);
            if (!Equals(null, result)) { return new ValueTask<IResult>(result); }

            try
            {
                return new ValueTask<IResult>(this.ResultCreate(Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value, newtonsoftJsonOptions)));
            }
            catch (Exception ex)
            {
                return new ValueTask<IResult>(this.ResultCreate(State, Message ?? $"Arguments {this.Alias} MessagePack deserialize error. {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// XML.Deserialize
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Method | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Property | System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class XmlArgAttribute : ArgumentAttribute
    {
        /// <summary>
        /// XmlArgAttribute
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <param name="rootElementName">Controls XML serialization of the attribute target as an XML root element.</param>
        public XmlArgAttribute(int state = -14, string message = null, string rootElementName = "xml") : base(state, message)
        {
            this.CanNull = false;
            this.Description = "Xml parsing";
            //this.ArgMeta.Filter |= FilterModel.NotDefinition;
            this.ArgMeta.Skip = (bool hasUse, bool hasDefinition, AttributeBase.MetaData.DeclaringType declaring, System.Collections.Generic.IEnumerable<ArgumentAttribute> arguments, bool ignoreArg) => (!hasDefinition && !this.ArgMeta.Arg.HasCollection) || this.ArgMeta.Arg.Parameters || ignoreArg;

            this.RootElementName = rootElementName;
        }

        /// <summary>
        /// Controls XML serialization of the attribute target as an XML root element.
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Check whether the defined value type is the default value, (top-level object commit), Default true
        /// </summary>
        public bool CheckValueType { get; set; } = true;

        /// <summary>
        /// Proces
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValueTask<IResult> Proces<Type>(dynamic value)
        {
            var result = CheckNull(this, value);
            if (!result.HasData) { return result; }

            //Check whether the defined value type is the default value, (top-level object commit)
            result = CheckDefinitionValueType(this, value, CheckValueType);
            if (!Equals(null, result)) { return result; }

            try
            {
                using (var reader = new System.IO.StringReader(value))
                {
                    var xmlSerializer = new System.Xml.Serialization.XmlSerializer(this.ArgMeta.MemberType, new XmlRootAttribute(RootElementName));
                    return new ValueTask<IResult>(this.ResultCreate(xmlSerializer.Deserialize(reader)));
                }
            }
            catch (System.Exception ex) { return new ValueTask<IResult>(this.ResultCreate(State, Message ?? $"Arguments {this.Alias} Xml deserialize error. {ex.Message}")); }
        }
    }

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
        /// Defines a mechanism for retrieving a service object; that is, an object that provides custom support to other objects.
        /// </summary>
        public IServiceProvider ServiceProvider { get; internal set; }

        /// <summary>
        /// Combine(DirectorySeparatorChar + data + AppDomain.CurrentDomain.FriendlyName.log.txt)
        /// </summary>
        public string LocalLogPath { get; set; } = System.IO.Path.Combine(System.IO.Path.DirectorySeparatorChar.ToString(), "data", $"{AppDomain.CurrentDomain.FriendlyName}.log.txt");

        /// <summary>
        /// result type
        /// </summary>
        public Type ResultType { get; internal set; } = typeof(ResultObject<>).GetGenericTypeDefinition();

        /// <summary>
        /// Log output
        /// </summary>
        public Action<LogData> Log = c => Help.Console(c.Message);// Help.WriteLocal(c.Message, Utils.Hosting.LocalLogPath, console: true);

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
                    return Utils.MessagePackDeserialize(data, parametersType);
                default: return null;
            }
        };

        internal Action<System.Text.Json.JsonSerializerOptions> useJsonOptions;
        internal Action<Newtonsoft.Json.JsonSerializerSettings> useNewtonsoftJsonOptions;
        internal MessagePack.MessagePackSerializerOptions useMessagePackOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithResolver(MessagePack.Resolvers.CompositeResolver.Create(new MessagePack.Formatters.IMessagePackFormatter[] { new MessagePack.Formatters.IgnoreFormatter<Type>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodBase>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.PropertyInfo>(), new MessagePack.Formatters.IgnoreFormatter<System.Reflection.FieldInfo>() }, new MessagePack.IFormatterResolver[] { MessagePack.Resolvers.ContractlessStandardResolver.Instance }));

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
        /// <returns></returns>
        ValueTask<WebSocketAcceptReply> WebSocketAccept(HttpContext context);

        /// <summary>
        /// Receive a websocket packet, return IReceiveData object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        ValueTask<IResultObject<byte[]>> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer);

        /// <summary>
        /// WebSocket dispose
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        ValueTask WebSocketDispose(HttpContext context, string token);
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
        public BusinessBase() => this.Logger = new Logger((Logger.LoggerData x) =>
        {
            Utils.Hosting.Log?.Invoke(new LogData(LogType.Info, x.ToString()));
            //Help.Console(x.ToString());
            return default;
        });

        /// <summary>
        /// Get the requested token
        /// </summary>
        /// <returns></returns>
        [Ignore]
        public virtual ValueTask<IToken> GetToken(HttpContext context, Token token) => new ValueTask<IToken>(Task.FromResult<IToken>(token));

        /// <summary>
        /// Accept a websocket connection. If null token is returned, it means reject, default string.Empty accept.
        /// <para>checked and return a token</para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [Ignore]
        public virtual ValueTask<WebSocketAcceptReply> WebSocketAccept(HttpContext context) => new ValueTask<WebSocketAcceptReply>(new WebSocketAcceptReply(string.Empty));

        /// <summary>
        /// Receive a websocket packet, return IReceiveData object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [Ignore]
        public virtual ValueTask<IResultObject<byte[]>> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer) => new ValueTask<IResultObject<byte[]>>((IResultObject<byte[]>)buffer.MessagePackDeserialize(Utils.Hosting.socketType));

        /// <summary>
        /// WebSocket dispose
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [Ignore]
        public virtual ValueTask WebSocketDispose(HttpContext context, string token) => default;
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
                Utils.Hosting.Log?.Invoke(new LogData(LogType.Error, $"404 {this.Request.Path.Value}"));
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
                        Utils.Hosting.Log?.Invoke(new LogData(LogType.Error, $"404 {this.Request.Path.Value}"));
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
                    Utils.Hosting.Log?.Invoke(new LogData(LogType.Error, $"benchmark {argNull.Message}"));
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
                Utils.Hosting.Log?.Invoke(new LogData(LogType.Error, $"ErrorCmd {errorCmd}"));
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
        public static readonly Hosting Hosting = new Hosting();

        ///// <summary>
        ///// Log client
        ///// </summary>
        //public readonly static HttpClient LogClient;

        static Utils()
        {
            if (NewtonsoftJsonOptions?.ContractResolver is Newtonsoft.Json.Serialization.DefaultContractResolver resolver && null != resolver)
            {
                resolver.NamingStrategy = NewtonsoftCamelCaseNamingStrategy.Instance;
            }
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

        #region MessagePack

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
        /// ToResult
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        public static IResultObject<Type> ToResult<Type>(this byte[] value, bool dataBytes = true)
        {
            if (dataBytes)
            {
                var result = (IResultObject<byte[]>)value.MessagePackDeserialize(Hosting.socketType);

                return ResultCreate(Hosting.ResultType, result.HasData ? result.Data.MessagePackDeserialize<Type>() : default, result.Message, result.State, result.Callback, false, result.HasData, result.HasDataResult, result.Business);
            }

            return (IResultObject<Type>)value.MessagePackDeserialize(Hosting.ResultType.MakeGenericType(typeof(Type)));
        }

        /// <summary>
        /// ToResultObject
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="value"></param>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        public static Result ToResultObject<Result>(this byte[] value, bool dataBytes = true)
            where Result : IResultObject
        {
            var type = typeof(Result);

            if (dataBytes)
            {
                var genericType = type.GetGenericTypeDefinition();
                var genericArg = type.GenericTypeArguments[0];

                var result = (IResultObject<byte[]>)value.MessagePackDeserialize(genericType.MakeGenericType(typeof(byte[])));

                return (Result)ResultCreate(genericType, genericArg, result.HasData ? result.Data.MessagePackDeserialize(genericArg) : default, result.Message, result.State, result.Callback, false, result.HasData, result.HasDataResult, result.Business);
            }

            return (Result)value.MessagePackDeserialize(type);
        }

        #endregion

        ///// <summary>
        ///// Send webSocket object
        ///// </summary>
        ///// <param name="business"></param>
        ///// <param name="args"></param>
        ///// <param name="id"></param>
        ///// <param name="method"></param>
        //public static void SendAsync(this IBusiness business, object[] args = null, string[] id = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null) => SendAsync(business, WebSockets, args, id, method);

        ///// <summary>
        ///// ISocket byte[]
        ///// </summary>
        ///// <param name="business"></param>
        ///// <param name="args"></param>
        ///// <param name="method"></param>
        ///// <returns></returns>
        //public static ISocket<byte[]> GetSocketData(this IBusiness business, object[] args = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null)
        //{
        //    var cmd = business.Command.GetCommand(method);

        //    if (null == cmd)
        //    {
        //        return default;
        //    }

        //    object arg = null;

        //    if (1 < args?.Length && !cmd.HasArgSingle)
        //    {
        //        arg = Activator.CreateInstance(cmd.ParametersType, args);
        //    }
        //    else if (0 < args?.Length && null != cmd.ParametersType)
        //    {
        //        arg = args[0];
        //    }

        //    var socket = Activator.CreateInstance(Hosting.socketType) as ISocket<byte[]>;

        //    //socket.Business = new BusinessInfo(business.Configer.Info.BusinessName, (cmd.Meta.Attributes.FirstOrDefault(c => c is PushAttribute) as PushAttribute)?.Key ?? method);
        //    socket.Business = new BusinessInfo(business.Configer.Info.BusinessName, method);
        //    socket.Data = arg?.MessagePackSerialize(cmd.ParametersType);
        //    //socket.Callback = method;

        //    return socket;
        //}

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
        /// <param name="resultTypeDefinition"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="checkData"></param>
        /// <param name="hasData"></param>
        /// <param name="hasDataResult"></param>
        /// <param name="businessInfo"></param>
        /// <returns></returns>
        public static IResultObject<Data> ResultCreate<Data>(Type resultTypeDefinition, Data data = default, string message = null, int state = 1, string callback = null, bool checkData = true, bool hasData = false, bool hasDataResult = true, BusinessInfo businessInfo = default) => ResultCreate(resultTypeDefinition, typeof(Data), data, message, state, callback, checkData, hasData, hasDataResult, businessInfo) as IResultObject<Data>;

        /// <summary>
        /// Used to create the IResult returns object
        /// </summary>
        /// <param name="resultTypeDefinition"></param>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="checkData"></param>
        /// <param name="hasData"></param>
        /// <param name="hasDataResult"></param>
        /// <param name="businessInfo"></param>
        /// <returns></returns>
        public static IResultObject ResultCreate(Type resultTypeDefinition, Type dataType, object data = null, string message = null, int state = 1, string callback = null, bool checkData = true, bool hasData = false, bool hasDataResult = true, BusinessInfo businessInfo = default)
        {
            var result = ResultFactory.ResultCreate(resultTypeDefinition, dataType, data, message, state, callback, checkData, hasData, hasDataResult) as IResultObject;
            result.Business = businessInfo;
            return result;
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
        public static async ValueTask<string> Log(this HttpClient httpClient, Logger.LoggerData data, string index = "log", string c = "Write") => await httpClient.HttpCallctd(c, null, new Log { Index = index, Data = data.ToString() }.JsonSerialize());

        static void StartupInfo(string message)
        {
            if (null == Hosting.Log)
            {
                Console.WriteLine(message);
            }
            else
            {
                Hosting.Log.Invoke(new LogData(LogType.Info, message));
            }
        }

        /// <summary>
        /// Configure Business.Core in the startup class configure method
        /// <para>Injection context parameter type: "Context", "WebSocket", "HttpFile"</para>
        /// </summary>
        /// <param name="app">provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="logOptions">logOptions">Output all non business exceptions or errors in the application</param>
        /// <param name="constructorArguments">constructorArguments</param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> CreateBusiness(this IApplicationBuilder app, Action<LogOptions> logOptions = null, params object[] constructorArguments)
        {
            Hosting.ServiceProvider = app.ApplicationServices;

            Hosting.logOptions = new LogOptions { StartupInfo = true, Logo = true };
            logOptions?.Invoke(Hosting.logOptions);
            if (null != Hosting.logOptions.Log)
            {
                Hosting.Log = Hosting.logOptions.Log;// Log;
            }
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Hosting.Log?.Invoke(new LogData(LogType.Exception, Convert.ToString((e.ExceptionObject as Exception)?.ExceptionWrite())));

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

            bootstrap = Bootstrap.CreateAll<IBusiness>(constructorArguments, type => app.ApplicationServices.GetService(type));

            bootstrap.Config.ResultType = typeof(ResultObject<>).GetGenericTypeDefinition();

            bootstrap.UseType(contextParameterTypes)
                .IgnoreSet(new Ignore(IgnoreMode.Arg), contextParameterTypes)
                .LoggerSet(new LoggerAttribute(canWrite: false), contextParameterTypes);

            bootstrap.Config.BuildBefore = strap =>
            {
                Hosting.ResultType = strap.Config.ResultType.GetGenericTypeDefinition();
                Hosting.socketType = Hosting.ResultType.MakeGenericType(typeof(byte[]));
                MessagePack.MessagePackSerializer.DefaultOptions = Hosting.useMessagePackOptions;

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

                    if (null == strap.Config.UseDoc.Options.Config) { strap.Config.UseDoc.Options.Config = new Dictionary<string, object>(); }
                    if (MessagePack.MessagePackCompression.None != MessagePack.MessagePackSerializer.DefaultOptions.Compression)
                    {
                        strap.Config.UseDoc.Options.Config.Add("MessagePackCompression", MessagePack.MessagePackSerializer.DefaultOptions.Compression.GetName());
                    }

                    //writ url to page
                    DocUI.Write(documentDir, docFileName: Configer.documentFileName);
                }
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
                                    Hosting.Log?.Invoke(new LogData(LogType.Exception, Convert.ToString(ex.ExceptionWrite())));
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
                Hosting.useNewtonsoftJsonOptions = options;
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
                            resolver.NamingStrategy = NewtonsoftCamelCaseNamingStrategy.Instance;
                        }

                        Hosting.useNewtonsoftJsonOptions?.Invoke(jsonSerializerOptions);
                        resolver = jsonSerializerOptions?.ContractResolver as Newtonsoft.Json.Serialization.DefaultContractResolver;

                        camelCase = c => resolver?.NamingStrategy?.GetPropertyName(c, false);
                    }
                }
            }

            return camelCase;
        }

        /// <summary>
        /// UseMessagePack
        /// </summary>
        /// <param name="bootstrap"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BootstrapAll<IBusiness> UseMessagePack(this BootstrapAll<IBusiness> bootstrap, Func<MessagePack.MessagePackSerializerOptions, MessagePack.MessagePackSerializerOptions> options = null)
        {
            var result = options?.Invoke(Hosting.useMessagePackOptions);
            if (null != result)
            {
                Hosting.useMessagePackOptions = result;
            }
            return bootstrap;
        }

        /*
        /// <summary>
        /// ToEndPoint
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public static IPEndPoint ToEndPoint(this System.Net.IPEndPoint endPoint)
        {
            if (endPoint is null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }

            return new IPEndPoint(endPoint.Address, endPoint.Port);
        }
        */
        //static System.Collections.Concurrent.ConcurrentDictionary<IPEndPoint, string> EndPoints = new System.Collections.Concurrent.ConcurrentDictionary<IPEndPoint, string>();

        ///// <summary>
        ///// udp port to listen
        ///// </summary>
        ///// <param name="bootstrap"></param>
        ///// <param name="port"></param>
        ///// <param name="natPort"></param>
        ///// <returns></returns>
        //public static BootstrapAll<IBusiness> UseLiteNetLibHost(this BootstrapAll<IBusiness> bootstrap, int port = 65000, int natPort = 65001)
        //{
        //    NatPunch.Instance.Run(port, natPort);

        //    return bootstrap;
        //}

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

                    receive.WebSocket.SendAsync(result2.ToBytes());
                }
            }
        }

        static async ValueTask Keep(HttpContext context, WebSocket webSocket)
        {
            if (null == businessFirst)
            {
                return;
            }

            var acceptBusiness = businessFirst;
            WebSocketAcceptReply reply = default;

            try
            {
                //string token = null;
                //var hasBusiness = true;
                //var a = context.Request.Headers["business"].ToString();
                //if (!string.IsNullOrWhiteSpace(a))
                //{
                //    hasBusiness = bootstrap.BusinessList.TryGetValue(a, out acceptBusiness);
                //}

                //if (hasBusiness)
                //{
                //    token = await acceptBusiness.WebSocketAccept(context, webSocket);
                //}
                reply = await acceptBusiness.WebSocketAccept(context);

                if (string.IsNullOrEmpty(reply.Token))
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        //no await!
                        await webSocket.CloseAsync(reply.CloseStatus, reply.Message);
                    }

                    return;
                }

                webSocket.SendObjectAsync(reply.Message, dataBytes: false); //accept ok! client checked

                WebSocketContainer.WebSockets.TryAdd(reply.Token, webSocket);

                var remote = string.Format("{0}:{1}", context.Connection.RemoteIpAddress.MapToIPv4().ToString(), context.Connection.RemotePort);

                var buffer = new byte[Hosting.webSocketOptions.ReceiveBufferSize];
                WebSocketReceiveResult socketResult;

                do
                {
                    socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (WebSocketMessageType.Close == socketResult.MessageType)
                    {
                        if (WebSocketState.CloseReceived == webSocket.State)
                        {
                            await webSocket.CloseOutputAsync(socketResult.CloseStatus.Value, socketResult.CloseStatusDescription, CancellationToken.None);
                        }

                        Hosting.Log?.Invoke(new LogData(LogType.Exception, $"Closed in server by the client. [{socketResult.CloseStatus.Value}] [Token:{reply.Token}]"));

                        continue;
                    }

                    try
                    {
                        var receiveData = await acceptBusiness.WebSocketReceive(context, webSocket, buffer);

                        if (string.IsNullOrWhiteSpace(receiveData.Business.BusinessName) || !bootstrap.BusinessList.TryGetValue(receiveData.Business.BusinessName, out IBusiness business))
                        {
                            webSocket.SendAsync(Hosting.ResultType.ErrorBusiness(receiveData.Business.BusinessName).ToBytes());
                            continue;
                        }

                        Task.Factory.StartNew(async c => await WebSocketCall((WebSocketReceive)c).ContinueWith(c2 =>
                        {
                            if (null != c2.Exception)
                            {
                                Hosting.Log?.Invoke(new LogData(LogType.Exception, $"{Convert.ToString(c2.Exception)}{Environment.NewLine}   [Token:{reply.Token}]"));
                            }
                        })
                        , new WebSocketReceive(await business.GetToken(context, new Token //token
                        {
                            Origin = Token.OriginValue.WebSocket,
                            Key = reply.Token,
                            //Key = System.Text.Encoding.UTF8.GetString(receiveData.t),
                            Remote = remote,
                            Callback = receiveData.Callback ?? receiveData.Business.Command,
                            Path = context.Request.Path.Value,
                        }), receiveData, business, webSocket));
                    }
                    catch (Exception ex)
                    {
                        Hosting.Log?.Invoke(new LogData(LogType.Exception, $"{Convert.ToString(ex.ExceptionWrite())}{Environment.NewLine}   [Token:{reply.Token}]"));
                        webSocket.SendAsync(Hosting.ResultType.ResultCreate(0, Convert.ToString(ex)).ToBytes());
                    }

                    //if (WebSocketState.Open != webSocket.State)
                    //{
                    //    break;
                    //}
                } while (!socketResult.CloseStatus.HasValue);

                //if (WebSocketState.Open == webSocket.State)
                //{
                //    //client close
                //    webSocket.CloseAsync(socketResult?.CloseStatus.Value ?? WebSocketCloseStatus.Empty, socketResult?.CloseStatusDescription);
                //}
            }
            catch (Exception ex)
            {
                Hosting.Log?.Invoke(new LogData(LogType.Exception, $"{Convert.ToString(ex.ExceptionWrite())}{Environment.NewLine}   [Token:{reply.Token}]"));
                //var result = ResultType.ResultCreate(0, Convert.ToString(ex));
                //await SocketSendAsync(result.ToBytes(), id);
                //if (WebSocketState.Open == webSocket.State)
                //{
                //    //server close
                //    webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, Convert.ToString(ex));
                //}
            }
            finally
            {
                if (null != reply.Token)
                {
                    WebSocketContainer.WebSockets.TryRemove(reply.Token, out _);
                }

                try
                {
                    await acceptBusiness.WebSocketDispose(context, reply.Token);
                }
                catch (Exception ex)
                {
                    Hosting.Log?.Invoke(new LogData(LogType.Exception, $"{Convert.ToString(ex.ExceptionWrite())}{Environment.NewLine}   [Token:{reply.Token}]"));
                }
            }
        }

        #region WebSocket

        /// <summary>
        /// WebSocket container
        /// </summary>
        public class WebSocketContainer : System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>
        {
            /// <summary>
            /// WebSocket container
            /// </summary>
            public static readonly WebSocketContainer WebSockets = new WebSocketContainer();

            /// <summary>
            /// Send socket object
            /// </summary>
            /// <typeparam name="Data"></typeparam>
            /// <param name="data"></param>
            /// <param name="callback"></param>
            /// <param name="dataBytes"></param>
            /// <param name="id"></param>
            public void SendObjectAsync<Data>(Data data, string callback = null, bool dataBytes = true, params string[] id) => SendAsync(Hosting.ResultType.ResultCreate(data, callback: callback).ToBytes(dataBytes), id);

            /// <summary>
            /// Send socket message
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public void SendAsync(byte[] bytes, params string[] id) => SendAsync(bytes, WebSocketMessageType.Binary, true, -1, id);

            /// <summary>
            /// Send socket message
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="sendMaxDegreeOfParallelism"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public void SendAsync(byte[] bytes, int sendMaxDegreeOfParallelism = -1, params string[] id) => SendAsync(bytes, WebSocketMessageType.Binary, true, sendMaxDegreeOfParallelism, id);

            /// <summary>
            /// Send socket message
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="messageType"></param>
            /// <param name="sendMaxDegreeOfParallelism"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public void SendAsync(byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, int sendMaxDegreeOfParallelism = -1, params string[] id) => SendAsync(bytes, messageType, true, sendMaxDegreeOfParallelism, id);

            /// <summary>
            /// Send socket message
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="messageType"></param>
            /// <param name="endOfMessage"></param>
            /// <param name="sendMaxDegreeOfParallelism"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public void SendAsync(byte[] bytes, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true, int sendMaxDegreeOfParallelism = -1, params string[] id)
            {
                if (null == bytes) { return; }

                if (null == id || 0 == id.Length)
                {
                    Parallel.ForEach(this, new ParallelOptions { MaxDegreeOfParallelism = sendMaxDegreeOfParallelism }, c =>
                    {
                        if (WebSocketState.Open != c.Value.State) { return; }

                        c.Value.SendAsync(bytes, messageType, endOfMessage);
                    });
                }
                else if (1 == id.Length)
                {
                    var c = id[0];

                    if (string.IsNullOrWhiteSpace(c)) { return; }

                    if (!TryGetValue(c, out WebSocket webSocket)) { return; }

                    if (webSocket.State != WebSocketState.Open) { return; }

                    webSocket.SendAsync(bytes, messageType, endOfMessage);
                }
                else
                {
                    Parallel.ForEach(id, new ParallelOptions { MaxDegreeOfParallelism = sendMaxDegreeOfParallelism }, c =>
                    {
                        if (string.IsNullOrWhiteSpace(c)) { return; }

                        if (!TryGetValue(c, out WebSocket webSocket)) { return; }

                        if (webSocket.State != WebSocketState.Open) { return; }

                        webSocket.SendAsync(bytes, messageType, endOfMessage);
                    });
                }
            }
        }

        /// <summary>
        /// WebSocket SendAsync object
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="webSocket"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <param name="dataBytes"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static bool SendObjectAsync<Data>(this WebSocket webSocket, Data data = default, string callback = null, bool dataBytes = true, WebSocketMessageType messageType = WebSocketMessageType.Binary) => SendAsync(webSocket, Hosting.ResultType.ResultCreate(data?.MessagePackSerialize(), callback: callback).ToBytes(dataBytes), messageType);

        /// <summary>
        /// WebSocket SendAsync
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="data"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        public static bool SendAsync(this WebSocket webSocket, byte[] data = null, WebSocketMessageType messageType = WebSocketMessageType.Binary, bool endOfMessage = true) => Hosting.webSocketSendQueue.TryAdd(new Hosting.WebSocketData(webSocket, null == data ? new ArraySegment<byte>(new byte[0]) : new ArraySegment<byte>(data), messageType, endOfMessage));

        /// <summary>
        /// Send webSocket object
        /// </summary>
        /// <param name="business"></param>
        /// <param name="args"></param>
        /// <param name="id"></param>
        /// <param name="method"></param>
        public static void SendAsync(this IBusiness business, object[] args = null, string[] id = null, [System.Runtime.CompilerServices.CallerMemberName] string method = null)
        {
            var cmd = business.Command.GetCommand(method);

            if (null == cmd)
            {
                return;
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

            //var socket = ResultCreate(Hosting.ResultType, arg?.MessagePackSerialize(cmd.ParametersType), businessInfo: new BusinessInfo(business.Configer.Info.BusinessName, method));
            //var socket = Activator.CreateInstance(Hosting.socketType) as ISocket<byte[]>;

            ////socket.Business = new BusinessInfo(business.Configer.Info.BusinessName, (cmd.Meta.Attributes.FirstOrDefault(c => c is PushAttribute) as PushAttribute)?.Key ?? method);
            //socket.Business = new BusinessInfo(business.Configer.Info.BusinessName, method);
            //socket.Data = arg?.MessagePackSerialize(cmd.ParametersType);
            ////socket.Callback = method;

            WebSocketContainer.WebSockets.SendAsync(ResultCreate(Hosting.ResultType, arg?.MessagePackSerialize(cmd.ParametersType), businessInfo: new BusinessInfo(business.Configer.Info.BusinessName, method)).ToBytes(false), id);

            //SendObjectAsync(arg?.MessagePackSerialize(cmd.ParametersType), null, new BusinessInfo(business.Configer.Info.BusinessName, method), id);
        }

        /// <summary>
        /// Closes the WebSocket connection as an asynchronous operation using the close handshake defined in the WebSocket protocol specification section 7.
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="closeStatus"></param>
        /// <param name="statusDescription"></param>
        /// <returns></returns>
        public static async ValueTask CloseAsync(this WebSocket webSocket, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string statusDescription = null) => await webSocket.CloseOutputAsync(0 == closeStatus ? WebSocketCloseStatus.NormalClosure : closeStatus, statusDescription, CancellationToken.None);

        #endregion

        #endregion

        #region Newtonsoft.Json

        /// <summary>
        /// Newtonsoft.Json CamelCaseNamingStrategy
        /// </summary>
        public class NewtonsoftCamelCaseNamingStrategy : Newtonsoft.Json.Serialization.CamelCaseNamingStrategy
        {
            /// <summary>
            /// Instance
            /// </summary>
            public static NewtonsoftCamelCaseNamingStrategy Instance { get; } = new NewtonsoftCamelCaseNamingStrategy();

            /// <summary>
            /// ResolvePropertyName
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            protected override string ResolvePropertyName(string name) => Help.CamelCase(name);
        }

        /// <summary>
        /// Responsible for parsing the overall request data
        /// </summary>
        public static Newtonsoft.Json.JsonSerializerSettings NewtonsoftJsonOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-ddTHH:mm:ss",
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = new List<Newtonsoft.Json.JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        /// <summary>
        /// TryJsonDeserialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Type TryNewtonsoftJsonDeserialize<Type>(this string value, Newtonsoft.Json.JsonSerializerSettings options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Type>(value, options ?? NewtonsoftJsonOptions);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// TryJsonDeserialize
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static object TryNewtonsoftJsonDeserialize(this string value, Type type, Newtonsoft.Json.JsonSerializerSettings options = null)
        {
            if (string.IsNullOrWhiteSpace(value) || type is null)
            {
                return default;
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(value, type, options ?? NewtonsoftJsonOptions);
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// JsonSerialize
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string NewtonsoftJsonSerialize<Type>(this Type value, Newtonsoft.Json.JsonSerializerSettings options = null)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, options ?? NewtonsoftJsonOptions);
        }

        #endregion
    }
}
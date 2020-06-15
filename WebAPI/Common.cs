using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebAPI.Annotations;

namespace WebAPI
{
    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct MyResultObject<Type> : IResultSocket<Type>
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
        public MyResultObject(System.Type dataType, Type data, int state = 1, string message = null, System.Type genericDefinition = null, bool checkData = true, bool hasDataResult = false)
        {
            this.DataType = dataType;
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = checkData ? !Equals(null, data) : false;

            this.Callback = null;
            this.Business = null;
            this.Command = null;
            this.GenericDefinition = genericDefinition;
            this.HasDataResult = hasDataResult;
        }

        /// <summary>
        /// MessagePack.MessagePackSerializer.Serialize(this)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="state"></param>
        /// <param name="message"></param>
        public MyResultObject(Type data, int state = 1, string message = null)
        {
            this.Data = data;
            this.State = state;
            this.Message = message;
            this.HasData = !Equals(null, data);

            this.Callback = null;
            this.Business = null;
            this.Command = null;
            this.DataType = null;
            this.GenericDefinition = null;
            this.HasDataResult = false;
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
        //[System.Text.Json.Serialization.JsonPropertyName("B")]
        public string Callback { get; set; }

        /// <summary>
        /// Business to call
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Business { get; set; }

        /// <summary>
        /// Commands to call
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Command { get; set; }

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
        /// Return data or not
        /// </summary>
        [MessagePack.IgnoreMember]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasDataResult { get; }

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

    [TokenCheck]
    [Logger(canWrite: false)]
    public struct Token : IToken
    {
        [System.Text.Json.Serialization.JsonPropertyName("K")]
        public string Key { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("R")]
        public string Remote { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("P")]
        public string Path { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string Callback { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Business.AspNet.Token.OriginValue Origin { get; set; }
    }

    [SessionCheck]
    [Use(typeof(Token))]
    public struct Session
    {
        public string Account { get; set; }
    }

    public class MyJsonArgAttribute : JsonArgAttribute
    {
        public MyJsonArgAttribute(int state = -12, string message = null) : base(state, message)
        {
            this.Description = "MyJson parsing";

            options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            options.Converters.Add(new Help.DateTimeConverter());
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        }
    }

    [MyJsonArg(Group = Utils.GroupJson)]
    public abstract class BusinessBase : Business.AspNet.BusinessBase
    {
        /// <summary>
        /// Log client
        /// </summary>
        public readonly static HttpClient LogClient = Utils.Hosting.HttpClientFactory.CreateClient("log");

        static BusinessBase()
        {
            LogClient.Timeout = TimeSpan.FromSeconds(3);
            LogClient.BaseAddress = new Uri("http://xx:8000/Log");
        }

        public BusinessBase()
        {
            this.Logger = new Logger(async (Logger.LoggerData log) =>
            {
                //var result = await LogClient.Log(log);

                //Console.WriteLine(log.ToString());
                //Help.Console(log.ToString());
            });
        }

        public sealed override async ValueTask<IToken> GetToken(HttpContext context, Business.AspNet.Token token) => new Token
        {
            Origin = token.Origin,
            Key = token.Key,
            Remote = token.Remote,
            Callback = token.Callback,
            Path = token.Path,
        };

        /********** Optional WebSocket: WebSocketAccept WebSocketReceive WebSocketDispose **********/

        /// <summary>
        /// WebSocket dictionary
        /// </summary>
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket> WebSockets = new System.Collections.Concurrent.ConcurrentDictionary<string, WebSocket>();

        public sealed override async ValueTask<string> WebSocketAccept(HttpContext context, WebSocket webSocket)
        {
            // checked and return a token
            if (!context.Request.Headers.TryGetValue("t", out Microsoft.Extensions.Primitives.StringValues t))
            {
                return null;//prevent
            }

            WebSockets.TryAdd(context.Connection.Id, webSocket);
#if DEBUG
            Console.WriteLine($"WebSockets Add:{context.Connection.Id} Connections:{WebSockets.Count}");
#endif
            return t.ToString();
        }

        public sealed override ValueTask<IResultSocket<byte[]>> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer) => base.WebSocketReceive(context, webSocket, buffer);

        public sealed override ValueTask WebSocketDispose(HttpContext context, WebSocket webSocket)
        {
            WebSockets.TryRemove(context.Connection.Id, out _);
#if DEBUG
            Console.WriteLine($"WebSockets Remove:{context.Connection.Id} Connectionss:{WebSockets.Count}");
#endif

            return base.WebSocketDispose(context, webSocket);
        }
    }
}
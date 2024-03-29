﻿using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Result;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebAPI50.Annotations;

namespace WebAPI50
{
    /// <summary>
    /// result
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public struct MyResultObject<Type> : IResultObject<Type>
    {
        /// <summary>
        /// MyResultObject
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator MyResultObject<Type>(byte[] value) => value.ToResultObject<MyResultObject<Type>>();

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
        public MyResultObject(System.Type dataType, Type data, int state = 1, string message = null, string callback = null, System.Type genericDefinition = null, bool checkData = true, bool hasData = false, bool hasDataResult = false)
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
        public MyResultObject(Type data, int state, string message, bool hasData)
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

        //public MyResultObject(int state, string message, bool hasData, Type data, string callback, System.Type dataType, System.Type genericDefinition, bool hasDataResult, BusinessInfo business)
        //{
        //    State = state;
        //    Message = message;
        //    HasData = hasData;
        //    Data = data;
        //    Callback = callback;
        //    DataType = dataType;
        //    GenericDefinition = genericDefinition;
        //    HasDataResult = hasDataResult;
        //    Business = business;
        //}

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
        /// Whether there is value
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("H")]
        [Newtonsoft.Json.JsonProperty("H")]
        public bool HasData { get; }

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
        /// Gets the token of this result, used for callback
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        //[System.Text.Json.Serialization.JsonPropertyName("B")]
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
        /// Business
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public BusinessInfo Business { get; set; }

        /// <summary>
        /// Json format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Utils.NewtonsoftJsonSerialize(this); //this.JsonSerialize();

        /// <summary>
        /// ProtoBuf,MessagePack or Other
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes(bool dataBytes = true) => dataBytes ? (HasDataResult ? Utils.ResultCreate(GenericDefinition ?? typeof(MyResultObject<>), HasData ? Data?.MessagePackSerialize() : default, Message, State, Callback, false, HasData, HasDataResult, Business).ToBytes(false) : ResultFactory.ResultCreate(GenericDefinition, State, Message, Callback).ToBytes(false)) : this.MessagePackSerialize();
    }

    [TokenCheck]
    [Logger(canWrite: false)]
    public struct Token : IToken
    {
        [System.Text.Json.Serialization.JsonPropertyName("K")]
        [Newtonsoft.Json.JsonProperty("K")]
        public string Key { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("R")]
        [Newtonsoft.Json.JsonProperty("R")]
        public Remote Remote { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("P")]
        [Newtonsoft.Json.JsonProperty("P")]
        public string Path { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string Callback { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Business.AspNet.Token.OriginValue Origin { get; set; }
    }

    /// <summary>
    /// SessionSessionSessionSessionSessionSession
    /// </summary>
    [SessionCheck]
    [Use(typeof(Token))]
    public struct Session : IToken
    {
        public string Account { get; set; }
        public string Key { get; set; }
        public Remote Remote { get; set; }
        public string Callback { get; set; }
    }

    public class MyJsonArgAttribute : NewtonsoftJsonArgAttribute
    {
        public MyJsonArgAttribute(int state = -12, string message = null) : base(state, message)
        {
            this.Description = "MyJson parsing";
        }

        public override async ValueTask<IResult> Proces<Type>(dynamic value)
        {
            return await base.Proces<Type>(value as object);
        }
    }

    //[MyJsonArg(Group = Utils.GroupJson)]
    [Annotations.JsonArg]
    //[NewtonsoftJsonArg]
    public abstract class BusinessBase : Business.AspNet.BusinessBase
    {
        ///// <summary>
        ///// Log client
        ///// </summary>
        //public readonly static HttpClient LogClient = Utils.Hosting.HttpClientFactory.CreateClient("log");

        static BusinessBase()
        {
            //LogClient.Timeout = TimeSpan.FromSeconds(3);
            //LogClient.BaseAddress = new Uri("http://xx:8000/Log");
        }

        //public BusinessBase()
        //{
        //    this.Logger = new Logger(async (Logger.LoggerData log) =>
        //    {
        //        log.Log();
        //        //Utils.Hosting.Log?.Invoke(LogType.Info, log.ToString());
        //        //var result = await LogClient.Log(log);

        //        //Console.WriteLine(log.ToString());
        //        //Help.Console(log.ToString());
        //    });
        //}

        public sealed override async ValueTask<IToken> GetToken(HttpContext context, Business.AspNet.Token token) => new Token
        {
            Origin = token.Origin,
            Key = token.Key,
            Remote = token.Remote,
            Callback = token.Callback,
            Path = token.Path,
        };
    }

    public class WebSocketManagement : Business.AspNet.WebSocketManagement
    {
        [Injection]
        IMemoryCache cache;

        [Injection]
        string test111 = string.Empty;

        /********** Optional WebSocket: WebSocketAccept WebSocketReceive WebSocketDispose **********/

        public sealed override async ValueTask<WebSocketAcceptReply> WebSocketAccept(HttpContext context)
        {
            test111.Log();
            cache.Set(test111, test111);
            var test = cache.Get<string>(test111);
            //var bing = await httpClient.GetStringAsync("https://www.bing.com/");

            // checked and return a token
            if (!context.Request.Query.TryGetValue("t", out Microsoft.Extensions.Primitives.StringValues t) || string.IsNullOrWhiteSpace(t))
            {
                return default;//prevent
            }

#if DEBUG
            Console.WriteLine($"WebSockets Add:{t} Connections:{WebSockets.Count + 1}");
#endif
            return new WebSocketAcceptReply(t, "ok!!!");
        }

        public override ValueTask WebSocketDispose(HttpContext context, string token)
        {
#if DEBUG
            Console.WriteLine($"WebSockets Remove:{token} Connectionss:{WebSockets.Count}");
#endif
            return default;
        }

        public override ValueTask<IResultObject<byte[]>> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer)
        {
            return base.WebSocketReceive(context, webSocket, buffer);
        }
    }
}
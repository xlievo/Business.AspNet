using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebAPI.Annotations;

namespace WebAPI
{
    [TokenCheck]
    [Use]
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
    [Use(true, Token = true)]
    public struct Session
    {
        public string Account { get; set; }
    }

    public abstract class BusinessBase : Business.AspNet.BusinessBase
    {
        /// <summary>
        /// Log client
        /// </summary>
        public readonly static HttpClient LogClient = Utils.Environment.HttpClientFactory.CreateClient("log");

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

                Console.WriteLine(log.ToString());
                //Help.Console(x.ToString());
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

        public sealed override ValueTask<IReceiveData> WebSocketReceive(HttpContext context, WebSocket webSocket, byte[] buffer) => base.WebSocketReceive(context, webSocket, buffer);

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

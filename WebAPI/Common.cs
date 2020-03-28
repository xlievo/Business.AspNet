using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Utils;
using System;
using WebAPI.Annotations;

namespace WebAPI
{
    [TokenCheck]
    [Use]
    [Logger(canWrite: false)]
    public struct Token : Business.Core.Auth.IToken
    {
        [System.Text.Json.Serialization.JsonPropertyName("K")]
        public string Key { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("R")]
        public string Remote { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("P")]
        public string Path { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string Callback { get; set; }
    }

    [SessionCheck]
    [Use(true, Token = true)]
    public struct Session
    {
        public string Account { get; set; }
    }

    public abstract class BusinessBase : Business.AspNet.BusinessBase
    {
        static BusinessBase()
        {
            Utils.LogClient.BaseAddress = new Uri("http://47.115.31.62:8000/Log");
        }

        public BusinessBase()
        {
            this.GetToken = async (context, token) => new Token
            {
                Key = token.Key,
                Remote = token.Remote,
                Callback = token.Callback,
                Path = context.Request.Path.Value,
            };

            this.Logger = new Logger(async (Logger.LoggerData x) =>
            {
                //var result = await Utils.LogClient.Log(x);

                Help.Console(x.ToString());
            });
        }
    }
}

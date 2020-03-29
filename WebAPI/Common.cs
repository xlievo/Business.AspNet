using Business.AspNet;
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Utils;
using System;
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
        static BusinessBase()
        {
            Utils.LogClient.BaseAddress = new Uri("http://xx:8000/Log");
        }

        public BusinessBase()
        {
            this.GetToken = async (context, token) => new Token
            {
                Origin = token.Origin,
                Key = token.Key,
                Remote = token.Remote,
                Callback = token.Callback,
                Path = token.Path,
            };

            this.Logger = new Logger(async (Logger.LoggerData x) =>
            {
                //var result = await Utils.LogClient.Log(x);

                Help.Console(x.ToString());
            });
        }
    }
}

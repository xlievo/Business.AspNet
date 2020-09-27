using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Result;
using Business.AspNet;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using Microsoft.Extensions.Logging;

#region AspNet

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //Configure cross domain policy
        services.AddCors(options =>
        {
            options.AddPolicy("any", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });
        services.AddMvc(option => option.EnableEndpointRouting = false)
            .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);

        services.AddHttpClient();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors("any");//API static documents need cross domain support

        app.CreateBusiness().UseDoc().Build();
    }
}

#endregion

[TokenCheck(message: "token illegal!")]//This is your token verification
[Logger(canWrite: false)]//Do not output log
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

//This is your token verification, must be inherited ArgumentAttribute
public class TokenCheck : ArgumentAttribute
{
    //Good state specifications are important
    public TokenCheck(int state = -80, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces(dynamic value)
    {
        Token token = value;

        var key = token.Key;

        //..1: check token key
        if (string.IsNullOrWhiteSpace(key))
        {
            return this.ResultCreate(this.State, this.Message); //error
        }

        //..2: check token logic

        return this.ResultCreate(); //ok
    }
}

/// <summary>
/// MyLogicArg!
/// </summary>
public struct MyLogicArg
{
    /// <summary>
    /// AAA
    /// </summary>
    public string A { get; set; }
    /// <summary>
    /// BBB
    /// </summary>
    public string B { get; set; }
}

//Maybe you need a custom base class? To unify the processing of logs and token
public class MyBusiness : Business.AspNet.BusinessBase
{
    readonly IHttpClientFactory _clientFactory;

    public MyBusiness(ILogger<MyBusiness> logger, IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;

        this.Logger = new Logger(async (IEnumerable<Logger.LoggerData> log) =>
        {
            foreach (var item in log)
            {
                logger.LogInformation(item.ToString());
            }
        }
        , new Logger.BatchOptions
        {
            Interval = TimeSpan.FromSeconds(6), //Return the accumulated log within 6 seconds
            MaxNumber = 2000 //It also returns when 2000 logs are accumulated
        });
    }

    //Override, using custom token In order to be able to process token data
    public sealed override ValueTask<IToken> GetToken(HttpContext context, Business.AspNet.Token token) =>
        new ValueTask<IToken>(Task.FromResult<IToken>(new Token
        {
            Origin = token.Origin,
            Key = token.Key,
            Remote = token.Remote,
            Callback = token.Callback,
            Path = token.Path
        }));

    //My first business logic
    //Logical method must be public virtual!
    //If inherited Business.AspNet.BusinessBase Base class, you just need to concentrate on writing logical methods!
    public virtual async Task<IResult<MyLogicArg>> MyLogic(Token token, MyLogicArg arg)
    {
        return this.ResultCreate(arg);
    }
}
# Business.AspNet
This is the middleware library from Business.Core to ASP.NET
## Install
[![NuGet Version](https://img.shields.io/nuget/v/Business.AspNet.svg?style=flat)](https://www.nuget.org/packages/Business.AspNet/)
[![NuGet](https://img.shields.io/nuget/dt/Business.AspNet.svg)](https://www.nuget.org/packages/Business.AspNet)
## Step 1: Create a new asp.net core web empty project and register Middleware
```C#
using Business.AspNet;

public void ConfigureServices(IServiceCollection services)
{
    //Configure cross domain policy
    services.AddCors(options =>
    {
        options.AddPolicy("any", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });
    services.AddMvc(option => option.EnableEndpointRouting = false)
        .SetCompatibilityVersion(CompatibilityVersion.Latest);
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    //app.UseBusiness();
	
    app.UseCors("any");//Document UI needs to be cross domain
	
    //If you want to configure documents
    app.UseBusiness(Business.Core.Bootstrap.CreateAll<BusinessBase>()
        .UseDoc(new Business.Core.Document.Config
        {
            Debug = true,
            Benchmark = true,
            Navigtion = true
        }));
}
```
## Step 2: declare your business class, Create a new class and copy the following
```C#
using Business.Core;
using Business.Core.Annotations;
using Business.Core.Auth;
using Business.Core.Result;
using Business.AspNet;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

[TokenCheck]//This is your token verification
[Use]
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

//This is your token verification
public class TokenCheck : ArgumentAttribute
{
    public TokenCheck(int state = -80, string message = null) : base(state, message) { }

    public override async ValueTask<IResult> Proces(dynamic value)
    {
        var key = value.Key as string;

        //..1: check token key
        if (string.IsNullOrWhiteSpace(key))
        {
            //return this.ResultCreate(this.State, this.Message);
        }
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

public class MyBusiness : Business.AspNet.BusinessBase
{
    public MyBusiness()
    {
        this.Logger = new Logger(async (Logger.LoggerData log) =>
        {
            //Output log
            Console.WriteLine(log.ToString());
        });
    }
	
    //Override, using custom token In order to be able to process token data
    public sealed override async ValueTask<IToken> GetToken(HttpContext context, Business.AspNet.Token token) 
       => new Token
    {
        Origin = token.Origin,
        Key = token.Key,
        Remote = token.Remote,
        Callback = token.Callback,
        Path = token.Path
    };
	
    //My first business logic
    public virtual async Task<IResult<MyLogicArg>> MyLogic(Token token, MyLogicArg arg)
    {
        return this.ResultCreate(arg);
    }
}
```
## Step 3: start your asp.net project and navigate to http://localhost:5000/doc/index.html

It only needs 2 steps, less than 100 lines of code. With the minimum configuration, you can get the whole framework without any other operations!
To learn more about him, refer to the https://github.com/xlievo/Business.AspNet/tree/master/WebAPI use case

If you have any questions, you can email me xlievo@live.com and I will try my best to answer them

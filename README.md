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
    services.AddMvc(option => option.EnableEndpointRouting = false)
        .SetCompatibilityVersion(CompatibilityVersion.Latest);
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseBusiness();
	
    //If you want to configure documents
    app.UseBusiness(Business.Core.Bootstrap.CreateAll<Business.AspNet.BusinessBase>()
    .UseDoc(new Business.Core.Document.Config
    {
        Debug = true,
        Benchmark = true,
        Navigtion = true
    }));
}
```
## Step 2: declare your business class
```C#
[TokenCheck]//This is your token verification
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

public struct MyBusinessArg
{
    public string A { get; set; }

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
	
    //Override, using custom token
    public sealed override async ValueTask<IToken> GetToken(HttpContext context, Business.AspNet.Token token)
        => new Token
    {
        Origin = token.Origin,
        Key = token.Key,
        Remote = token.Remote,
        Callback = token.Callback,
        Path = token.Path
    };
	
	public virtual async Task<IResult<MyBusinessArg>> MyLogic(Token token, MyBusinessArg arg)
    {
        return this.ResultCreate(arg);
    }
}
```
## Step 3: start your asp.net project and navigate to http://localhost:5000/doc/index.html

It only needs 2 steps, less than 100 lines of code. With the minimum configuration, you can get the whole framework without any other operations!
To learn more about him, refer to the https://github.com/xlievo/Business.AspNet/tree/master/WebAPI use case

If you have any questions, you can email me xlievo@live.com and I will try my best to answer them

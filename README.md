# Business.AspNet
This is the middleware library from Business.Core to ASP.NET

## Step 1: register Middleware
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
## Step 2: declare your business base class
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

public abstract class BusinessBase : Business.AspNet.BusinessBase
{
    public BusinessBase()
    {
        this.Logger = new Logger(async (Logger.LoggerData log) =>
        {
            Console.WriteLine(log.ToString());
        });
    }

    public sealed override async ValueTask<IToken> GetToken(HttpContext context, Business.AspNet.Token token)
        => new Token
    {
        Origin = token.Origin,
        Key = token.Key,
        Remote = token.Remote,
        Callback = token.Callback,
        Path = token.Path
    };
}
```
## Step 3: start using your business class
```C#
public struct MyBusinessArg
{
    public string A { get; set; }

    public string B { get; set; }
}

public class BusinessMember : BusinessBase
{
    public virtual async Task<IResult<MyBusinessArg>> MyBusiness(Token token, MyBusinessArg arg)
    {
        return this.ResultCreate(arg);
    }
}
```
## Step 4: start your asp.net project and navigate to http://localhost:5000/doc/index.html

It only needs 4 steps, less than 100 lines of code. With the minimum configuration, you can get the whole framework without any other operations!
If you have any questions, you can email me xlievo@live.com and I will try my best to answer them

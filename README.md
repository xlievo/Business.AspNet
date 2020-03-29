# Business.AspNet
This is the middleware library from Business.Core to ASP.NET

```C#
using Business.AspNet;

public void ConfigureServices(IServiceCollection services)
{
	services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Latest);
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseBusiness();
	
	//If you want to configure documents
	
	app.UseBusiness(Business.Core.Bootstrap.CreateAll<Business.AspNet.BusinessBase>().UseDoc(new Business.Core.Document.Config
	{
		Debug = true,
		Benchmark = true,
		Navigtion = true
	}));
}
```
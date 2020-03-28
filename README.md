# Business.AspNet
This is the middleware library from Business.Core to ASP.NET

```C#
using Business.AspNet;

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.InitBusiness();
}
```
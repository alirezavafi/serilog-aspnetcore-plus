# Serilog.AspNetCore.Plus
An improved version of Serilog.AspNetCore package based on my experience in years ago. This package add following features:

- Adds following features with default log setup:
  - Adds User Claims Enricher. This enricher logs all user claims.
  - Adds CorrelationId Enricher. This enricher adds Correlation-Id to all log events. This helps finding relevant logs using Correlation-Id.
  - Adds Default EventId Enricher. This enricher uses MurmurHash to generate Short Ids based on message template.
  - Adds LogContext Enricher. This enricher allows to capture data on relevant log entries.
  - Attaches MachineName and CurrentUserName (OS-Level) to logs.
  - Adds Json.Net/System.Text.Json destructure to serialize json texts correctly.
- Allow to capture request/response with following features:
  - Conditional request/response capturing based on success or error responses.
  - Captures jsons as structured object that allows filtering logs on request/response object properties.
  - Masks sensitive information.
  - Captures additional data like Parsed User Agent Data.
- Adds logsettings.json and logsettings.{Environment}.json optionally to manage log configuration in separate settings file for simplicity. 

### Instructions

**First**, install the _Serilog.AspNetCore.Plus_ [NuGet package](https://www.nuget.org/packages/Serilog.AspNetCore.Plus) into your app.

```shell
dotnet add package Serilog.AspNetCore.Plus
```

**Next**, in your application's _Program.cs_ file, configure Serilog first.  A `try`/`catch` block will ensure any configuration issues are appropriately logged:

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(); // <-- Add this line
    
    var app = builder.Build();

    app.MapGet("/", () => "Hello World!");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

The `builder.Host.UseSerilogPlus()` call will redirect all log events through your Serilog pipeline.

**Or** you can initialize logger directly using:
```csharp
    Log.Logger = new LoggerConfiguration()
                    .SetSerilogPlusDefaultConfiguration() // <-- Add awesome staff like EventId, UserAgent and other useful enrichers
                    .WriteTo.File(new CompactJsonFormatter(),"log.json")
                    .CreateLogger();
```

**Finally**, clean up by removing the remaining configuration for the default logger:

 * Remove the `"Logging"` section from _appsettings.*.json_ files (this can be replaced with [Serilog configuration](https://github.com/serilog/serilog-settings-configuration) as shown in [the _Sample_ project](https://github.com/serilog/serilog-aspnetcore/blob/dev/samples/Sample/Program.cs), if required)
 * Remove `UseApplicationInsights()` (this can be replaced with the [Serilog AI sink](https://github.com/serilog/serilog-sinks-applicationinsights), if required)

That's it! With the level bumped up a little you will see log output resembling:

```
[22:14:44.646 DBG] RouteCollection.RouteAsync
    Routes: 
        Microsoft.AspNet.Mvc.Routing.AttributeRoute
        {controller=Home}/{action=Index}/{id?}
    Handled? True
[22:14:44.647 DBG] RouterMiddleware.Invoke
    Handled? True
[22:14:45.706 DBG] /lib/jquery/jquery.js not modified
[22:14:45.706 DBG] /css/site.css not modified
[22:14:45.741 DBG] Handled. Status code: 304 File: /css/site.css
```

**Tip:** to see Serilog output in the Visual Studio output window when running under IIS, either select _ASP.NET Core Web Server_ from the _Show output from_ drop-down list, or replace `WriteTo.Console()` in the logger configuration with `WriteTo.Debug()`.


### Request logging

In your application's _Startup.cs_, add the middleware with `UseSerilogPlus()`:

```csharp
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSerilogPlusRequestLogging(); // <-- Add this line

            // or for more options add this:
            // app.UseSerilogPlusRequestLogging(p =>
            // {
            //     p.LogMode = LogMode.LogAll;
            //     p.RequestHeaderLogMode = LogMode.LogFailures;
            //     p.RequestBodyLogMode = LogMode.LogFailures;
            //     p.RequestBodyLogTextLengthLimit = 5000;
            //     p.ResponseHeaderLogMode = LogMode.LogNone;
            //     p.ResponseBodyLogMode = LogMode.LogNone;
            //     p.ResponseBodyLogTextLengthLimit = 5000;
            //     p.MaskFormat = "*****"; 
            //     p.MaskedProperties.Clear();
            //     p.MaskedProperties.Add("*password*");
            //     p.MaskedProperties.Add("*token*");
            // });
            
            // ...
            // Other app configuration
        }
```

It's important that the `UseSerilogPlusRequestLogging()` call appears _before_ handlers such as MVC. The middleware will not time or log components that appear before it in the pipeline. (This can be utilized to exclude noisy handlers from logging, such as `UseStaticFiles()`, by placing `UseSerilogPlusRequestLogging()` after them.)

### Sample Logged Item

```json
{
    "@t": "2021-03-04T21:01:36.3305267Z",
    "@m": "HTTP Request Completed { Request: { ClientIp: \"127.0.0.1\", Method: \"GET\", Scheme: \"https\", Host: \"localhost:5001\", Path: \"/home/list\", QueryString: \"\", Query: [], BodyString: null, Body: null }, Response: { StatusCode: 200, ElapsedMilliseconds: 1110, BodyString: \"[\r\n  {\r\n    \\\"date\\\": \\\"2021-03-06T00:31:35.3561032+03:30\\\",\r\n    \\\"passwordNumber\\\": \\\"*** MASKED ***\\\",\r\n    \\\"temperatureF\\\": 109,\r\n    \\\"token\\\": \\\"*** MASKED ***\\\",\r\n    \\\"summary\\\": \\\"Hot\\\"\r\n  },\r\n  {\r\n    \\\"date\\\": \\\"2021-03-07T00:31:35.3567648+03:30\\\",\r\n    \\\"passwordNumber\\\": \\\"*** MASKED ***\\\",\r\n    \\\"temperatureF\\\": 121,\r\n    \\\"token\\\": \\\"*** MASKED ***\\\",\r\n    \\\"summary\\\": \\\"Chilly\\\"\r\n  },\r\n  {\r\n    \\\"date\\\": \\\"2021-03-08T00:31:35.3567697+03:30\\\",\r\n    \\\"passwordNumber\\\": \\\"*** MASKED ***\\\",\r\n    \\\"temperatureF\\\": 29,\r\n    \\\"token\\\": \\\"*** MASKED ***\\\",\r\n    \\\"summary\\\": \\\"Hot\\\"\r\n  },\r\n  {\r\n    \\\"date\\\": \\\"2021-03-09T00:31:35.3567715+03:30\\\",\r\n    \\\"passwordNumber\\\": \\\"*** MASKED ***\\\",\r\n    \\\"temperatureF\\\": 7,\r\n    \\\"token\\\": \\\"*** MASKED ***\\\",\r\n    \\\"summary\\\": \\\"Scorching\\\"\r\n  },\r\n  {\r\n    \\\"date\\\": \\\"2021-03-10T00:31:35.3567728+03:30\\\",\r\n    \\\"passwordNumber\\\": \\\"*** MASKED ***\\\",\r\n    \\\"temperatureF\\\": 73,\r\n    \\\"token\\\": \\\"*** MASKED ***\\\",\r\n    \\\"summary\\\": \\\"Chilly\\\"\r\n  }\r\n]\", Body: [{ date: \"2021-03-06T00:31:35.3561032+03:30\", passwordNumber: \"*** MASKED ***\", temperatureF: 109, token: \"*** MASKED ***\", summary: \"Hot\" }, { date: \"2021-03-07T00:31:35.3567648+03:30\", passwordNumber: \"*** MASKED ***\", temperatureF: 121, token: \"*** MASKED ***\", summary: \"Chilly\" }, { date: \"2021-03-08T00:31:35.3567697+03:30\", passwordNumber: \"*** MASKED ***\", temperatureF: 29, token: \"*** MASKED ***\", summary: \"Hot\" }, { date: \"2021-03-09T00:31:35.3567715+03:30\", passwordNumber: \"*** MASKED ***\", temperatureF: 7, token: \"*** MASKED ***\", summary: \"Scorching\" }, { date: \"2021-03-10T00:31:35.3567728+03:30\", passwordNumber: \"*** MASKED ***\", temperatureF: 73, token: \"*** MASKED ***\", summary: \"Chilly\" }] } }",
    "@i": "42abf3a2",
    "Data": {
        "Request": {
            "ClientIp": "127.0.0.1",
            "Method": "GET",
            "Scheme": "https",
            "Host": "localhost:5001",
            "Path": "/home/list",
            "QueryString": "",
            "Query": [],
            "BodyString": null,
            "Body": null
        },
        "Response": {
            "StatusCode": 200,
            "ElapsedMilliseconds": 1110,
            "BodyString": "[\r\n  {\r\n    \"date\": \"2021-03-06T00:31:35.3561032+03:30\",\r\n    \"passwordNumber\": \"*** MASKED ***\",\r\n    \"temperatureF\": 109,\r\n    \"token\": \"*** MASKED ***\",\r\n    \"summary\": \"Hot\"\r\n  },\r\n  {\r\n    \"date\": \"2021-03-07T00:31:35.3567648+03:30\",\r\n    \"passwordNumber\": \"*** MASKED ***\",\r\n    \"temperatureF\": 121,\r\n    \"token\": \"*** MASKED ***\",\r\n    \"summary\": \"Chilly\"\r\n  },\r\n  {\r\n    \"date\": \"2021-03-08T00:31:35.3567697+03:30\",\r\n    \"passwordNumber\": \"*** MASKED ***\",\r\n    \"temperatureF\": 29,\r\n    \"token\": \"*** MASKED ***\",\r\n    \"summary\": \"Hot\"\r\n  },\r\n  {\r\n    \"date\": \"2021-03-09T00:31:35.3567715+03:30\",\r\n    \"passwordNumber\": \"*** MASKED ***\",\r\n    \"temperatureF\": 7,\r\n    \"token\": \"*** MASKED ***\",\r\n    \"summary\": \"Scorching\"\r\n  },\r\n  {\r\n    \"date\": \"2021-03-10T00:31:35.3567728+03:30\",\r\n    \"passwordNumber\": \"*** MASKED ***\",\r\n    \"temperatureF\": 73,\r\n    \"token\": \"*** MASKED ***\",\r\n    \"summary\": \"Chilly\"\r\n  }\r\n]",
            "Body": [
                {
                    "date": "2021-03-06T00:31:35.3561032+03:30",
                    "passwordNumber": "*** MASKED ***",
                    "temperatureF": 109,
                    "token": "*** MASKED ***",
                    "summary": "Hot"
                },
                {
                    "date": "2021-03-07T00:31:35.3567648+03:30",
                    "passwordNumber": "*** MASKED ***",
                    "temperatureF": 121,
                    "token": "*** MASKED ***",
                    "summary": "Chilly"
                },
                {
                    "date": "2021-03-08T00:31:35.3567697+03:30",
                    "passwordNumber": "*** MASKED ***",
                    "temperatureF": 29,
                    "token": "*** MASKED ***",
                    "summary": "Hot"
                },
                {
                    "date": "2021-03-09T00:31:35.3567715+03:30",
                    "passwordNumber": "*** MASKED ***",
                    "temperatureF": 7,
                    "token": "*** MASKED ***",
                    "summary": "Scorching"
                },
                {
                    "date": "2021-03-10T00:31:35.3567728+03:30",
                    "passwordNumber": "*** MASKED ***",
                    "temperatureF": 73,
                    "token": "*** MASKED ***",
                    "summary": "Chilly"
                }
            ]
        }
    },
    "RequestId": "0HM6VA2OSU9N1:00000001",
    "RequestPath": "/home/list",
    "ConnectionId": "0HM6VA2OSU9N1",
    "EnvironmentUserName": "DESKTOP\\Alireza",
    "MachineName": "DESKTOP",
    "EventId": "C2110DE4"
}
```

### Two-stage initialization

The example at the top of this page shows how to configure Serilog immediately when the application starts. This has the benefit of catching and reporting exceptions thrown during set-up of the ASP.NET Core host.

The downside of initializing Serilog first is that services from the ASP.NET Core host, including the `appsettings.json` configuration and dependency injection, aren't available yet.

To address this, Serilog supports two-stage initialization. An initial "bootstrap" logger is configured immediately when the program starts, and this is replaced by the fully-configured logger once the host has loaded.

To use this technique, first replace the initial `CreateLogger()` call with `CreateBoostrapLogger()`:

```csharp
using Serilog;

public class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger(); // <-- Change this line!
```

Then, pass a callback to `UseSerilog()` that creates the final logger:

```csharp
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
```

It's important to note that the final logger **completely replaces** the bootstrap logger: if you want both to log to the console, for instance, you'll need to specify `WriteTo.Console()` in both places, as the example shows.

#### Consuming `appsettings.json` configuration

**Using two-stage initialization**, insert the `ReadFrom.Configuration(context.Configuration)` call shown in the example above. The JSON configuration syntax is documented in [the _Serilog.Settings.Configuration_ README](https://github.com/serilog/serilog-settings-configuration).

#### Injecting services into enrichers and sinks

**Using two-stage initialization**, insert the `ReadFrom.Services(services)` call shown in the example above. The `ReadFrom.Services()` call will configure the logging pipeline with any registered implementations of the following services:

 * `IDestructuringPolicy`
 * `ILogEventEnricher`
 * `ILogEventFilter`
 * `ILogEventSink`
 * `LoggingLevelSwitch`

#### Enabling `Microsoft.Extensions.Logging.ILoggerProvider`s

Serilog sends events to outputs called _sinks_, that implement Serilog's `ILogEventSink` interface, and are added to the logging pipeline using `WriteTo`. _Microsoft.Extensions.Logging_ has a similar concept called _providers_, and these implement `ILoggerProvider`. Providers are what the default logging configuration creates under the hood through methods like `AddConsole()`.

By default, Serilog ignores providers, since there are usually equivalent Serilog sinks available, and these work more efficiently with Serilog's pipeline. If provider support is needed, it can be optionally enabled.

To have Serilog pass events to providers, **using two-stage initialization** as above, pass `writeToProviders: true` in the call to `UseSerilog()`:

```csharp
    .UseSerilog(
        (hostingContext, services, loggerConfiguration) => /* snip! */,
        writeToProviders: true)
```

### JSON output

The `Console()`, `Debug()`, and `File()` sinks all support JSON-formatted output natively, via the included _Serilog.Formatting.Compact_ package.

To write newline-delimited JSON, pass a `CompactJsonFormatter` or `RenderedCompactJsonFormatter` to the sink configuration method:

```csharp
    .WriteTo.Console(new RenderedCompactJsonFormatter())
```

### Writing to the Azure Diagnostics Log Stream

The Azure Diagnostic Log Stream ships events from any files in the `D:\home\LogFiles\` folder. To enable this for your app, add a file sink to your `LoggerConfiguration`, taking care to set the `shared` and `flushToDiskInterval` parameters:

```csharp
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            // Add this line:
            .WriteTo.File(
                @"D:\home\LogFiles\Application\myapp.txt",
                fileSizeLimitBytes: 1_000_000,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();
```

### Pushing properties to the `ILogger<T>`

If you want to add extra properties to all log events in a specific part of your code, you can add them to the **`ILogger<T>`** in **Microsoft.Extensions.Logging** with the following code. For this code to work, make sure you have added the `.Enrich.FromLogContext()` to the `.UseSerilog(...)` statement, as specified in the samples above.

```csharp
// Microsoft.Extensions.Logging ILogger<T>
// Yes, it's required to use a dictionary. See https://nblumhardt.com/2016/11/ilogger-beginscope/
using (logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = "svrooij",
    ["OperationType"] = "update",
}))
{
   // UserId and OperationType are set for all logging events in these brackets
}
```

The code above results in the same outcome as if you would push properties in the **ILogger** in Serilog.

```csharp
// Serilog ILogger
using (logger.PushProperty("UserId", "svrooij"))
using (logger.PushProperty("OperationType", "update"))
{
    // UserId and OperationType are set for all logging events in these brackets
}
```

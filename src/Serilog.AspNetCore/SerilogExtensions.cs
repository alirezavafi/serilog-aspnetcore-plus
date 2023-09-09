using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Debugging;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Serilog;

/// <summary>
/// Serilog configuration for asp.net core middlewares
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configure host to use preconfigured and practiced Serilog
    /// </summary>
    /// <param name="host"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder UseSerilogPlus(this IHostBuilder host,
        Action<LoggerConfiguration> options)
    {
        return host.UseSerilogPlus((ctx, cfg) => options?.Invoke(cfg));
    }

    /// <summary>
    /// Configure host to use preconfigured and practiced Serilog
    /// </summary>
    /// <param name="host"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHostBuilder UseSerilogPlus(this IHostBuilder host, Action<HostBuilderContext, LoggerConfiguration>? options = null)
    {
        host.ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("logsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"logsettings.{env.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);
        });
        host.UseSerilog((context, config) =>
        {
            var loggerConfiguration = config.SetSerilogPlusDefaultConfiguration() 
                .ReadFrom.Configuration(context.Configuration);
            var logPath = context.Configuration["Serilog:DefaultLogLocation"]?.ToString() ?? "App_Data/Logs";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            options?.Invoke(context, loggerConfiguration);
            var file = File.CreateText($"{logPath}/internal-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");
            SelfLog.Enable(TextWriter.Synchronized(file));
        });
        return host;
    }

    /// <summary>
    /// Configure host to use preconfigured and practiced Serilog
    /// </summary>
    /// <param name="host"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    [Obsolete("Prefer UseSerilogPlus() on IHostBuilder")]
    public static IWebHostBuilder UseSerilogPlus(this IWebHostBuilder host,
        Action<WebHostBuilderContext, LoggerConfiguration>? options = null)
    {
        host.ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("logsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"logsettings.{env.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);
        });
        host.UseSerilog((builder, config) =>
        {
            var loggerConfiguration = config.SetSerilogPlusDefaultConfiguration()
                .ReadFrom.Configuration(builder.Configuration);
            var logPath = builder.Configuration["Serilog:DefaultLogLocation"]?.ToString() ?? "App_Data/Logs";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            options?.Invoke(builder, loggerConfiguration);
            var file = File.CreateText($"{logPath}/internal-{DateTime.Now.Ticks}.log");
            SelfLog.Enable(TextWriter.Synchronized(file));
        });
        return host;
    }

    /// <summary>
    /// Configure host to use preconfigured and practiced Serilog
    /// </summary>
    /// <param name="host"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    [Obsolete("Prefer UseSerilogPlus() on IHostBuilder")]
    public static IWebHostBuilder UseSerilogPlus(this IWebHostBuilder host,
        Action<LoggerConfiguration>? options = null)
    {
        host.UseSerilogPlus((ctx, opt) => options?.Invoke(opt));
        return host;
    }
}
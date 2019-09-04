#if NETCOREAPP || NETSTANDARD

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swifter.Json;
using Swifter.Tools;
using System;

public static partial class SwifterExtensions
{
    /// <summary>
    /// 为 AspNet 添加来自 Swifter.Json 高性能的 Json 格式支持。
    /// </summary>
    /// <param name="services">Service 集合</param>
    /// <param name="jsonFormatterOptions">Json 格式器配置</param>
    /// <param name="configuration">配置回调</param>
    public static void ConfigureJsonFormatter(this IServiceCollection services, JsonFormatterOptions jsonFormatterOptions = JsonFormatterOptions.Default, Action<JsonFormatter> configuration = null)
    {
        static void RemoveJsonFormatter<T>(FormatterCollection<T> formatters)
        {
            for (int i = formatters.Count - 1; i >= 0; --i)
            {
                if (formatters[i]?.GetType().FullName.IndexOf("Json", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    formatters.RemoveAt(i);
                }
            }
        }

        var jsonFormatter = new JsonFormatter(jsonFormatterOptions);

        configuration?.Invoke(jsonFormatter);

        AspNetJsonExecutor.Create(jsonFormatter).AddToSingleton(services);

        services.Configure<MvcOptions>(mvcOptions =>
        {
            RemoveJsonFormatter(mvcOptions.InputFormatters);
            RemoveJsonFormatter(mvcOptions.OutputFormatters);

            var formatter = new AspNetJsonFormatter(jsonFormatter);

            mvcOptions.InputFormatters.Add(formatter);
            mvcOptions.OutputFormatters.Add(formatter);
        });
    }

    /// <summary>
    /// 为 Mvc 添加来自 Swifter.Json 的高性能 Json 格式支持。
    /// </summary>
    /// <param name="mvcBuilder">Mvc Builder</param>
    /// <param name="options">Json 格式器配置</param>
    /// <param name="configuration">配置回调</param>
    /// <returns></returns>
    public static IMvcBuilder ConfigureJsonFormatter(this IMvcBuilder mvcBuilder, JsonFormatterOptions options = JsonFormatterOptions.Default, Action<JsonFormatter> configuration = null)
    {
        mvcBuilder.Services.ConfigureJsonFormatter(options, configuration);

        return mvcBuilder;
    }

    /// <summary>
    /// 为 Mvc 添加来自 Swifter.Json 的高性能 Json 格式支持。
    /// </summary>
    /// <param name="mvcCoreBuilder">Mvc Builder</param>
    /// <param name="options">Json 格式器配置</param>
    /// <param name="configuration">配置回调</param>
    /// <returns></returns>
    public static IMvcCoreBuilder ConfigureJsonFormatter(this IMvcCoreBuilder mvcCoreBuilder, JsonFormatterOptions options = JsonFormatterOptions.Default, Action<JsonFormatter> configuration = null)
    {
        mvcCoreBuilder.Services.ConfigureJsonFormatter(options, configuration);


        return mvcCoreBuilder;
    }

}

#endif
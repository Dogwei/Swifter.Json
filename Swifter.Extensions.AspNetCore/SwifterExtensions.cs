using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swifter.Json;
using Swifter.Reflection;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Swifter 为 AspNetCore 提供的扩展方法。
/// </summary>
public static class SwifterExtensions
{
    const string JSONUTF8ContentType = "application/json; charset=utf-8";

    /// <summary>
    /// 为 AspNet 添加来自 Swifter.Json 高性能的 Json 格式支持。
    /// </summary>
    /// <param name="services">Service 集合</param>
    /// <param name="jsonFormatterOptions">Json 格式器配置</param>
    /// <param name="configuration">配置回调</param>
    public static void ConfigureJsonFormatter(this IServiceCollection services, JsonFormatterOptions jsonFormatterOptions = JsonFormatterOptions.Default, Action<JsonFormatter> configuration = null)
    {
        void RemoveJsonFormatter<T>(FormatterCollection<T> formatters)
        {
            for (int i = formatters.Count - 1; i >= 0; --i)
            {
                if (formatters[i]?.GetType().FullName.IndexOf("Json", StringComparison.CurrentCultureIgnoreCase) + 1 != 0)
                {
                    formatters.RemoveAt(i);
                }
            }
        }

        var types = TypeHelper.GetTypes("Microsoft.AspNetCore.Mvc.JsonResult");

        var jsonFormatter = new JsonFormatter(jsonFormatterOptions);

        configuration?.Invoke(jsonFormatter);

        foreach (var item in types)
        {
            if (typeof(IActionResult).IsAssignableFrom(item))
            {
                try
                {
                    var executorType = typeof(AspNetJsonExecutor<>).MakeGenericType(item);
                    var serviceType = typeof(IActionResultExecutor<>).MakeGenericType(item);

                    var executor = Activator.CreateInstance(executorType, jsonFormatter);

                    services.AddSingleton(serviceType, executor);
                }
                catch (Exception)
                {
                }
            }
        }

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

    static string GetNonEmptyString(string st1, string st2, string st3)
    {
        return string.IsNullOrEmpty(st1) ? string.IsNullOrEmpty(st2) ? st3 : st2 : st1;
    }

    sealed class AspNetJsonExecutor<TJsonResult> : IActionResultExecutor<TJsonResult> where TJsonResult : IActionResult
    {
        static readonly XPropertyInfo valueProperty;
        static readonly XPropertyInfo settingsProperty;
        static readonly XPropertyInfo statusProperty;
        static readonly XPropertyInfo contentTypeProperty;

        static AspNetJsonExecutor()
        {
            var xTypeInfo = XTypeInfo.Create<TJsonResult>(
                XBindingFlags.Public |
                XBindingFlags.Instance |
                XBindingFlags.Property);

            valueProperty = xTypeInfo.GetProperty("Value") ?? xTypeInfo.GetProperty("Data") ?? throw new NotSupportedException("Value");
            settingsProperty = xTypeInfo.GetProperty("SerializerSettings") ?? throw new NotSupportedException("SerializerSettings");
            statusProperty = xTypeInfo.GetProperty("StatusCode") ?? throw new NotSupportedException("StatusCode");
            contentTypeProperty = xTypeInfo.GetProperty("ContentType") ?? throw new NotSupportedException("ContentType");
        }

        readonly JsonFormatter jsonFormatter;

        public AspNetJsonExecutor(JsonFormatter jsonFormatter)
        {
            this.jsonFormatter = jsonFormatter;
        }

        public async Task ExecuteAsync(ActionContext context, TJsonResult result)
        {
            var value = valueProperty.GetValue(result);
            var settings = settingsProperty.GetValue(result);
            var status = statusProperty.GetValue(result);
            var contentType = Convert.ToString(contentTypeProperty.GetValue(result));

            var response = context.HttpContext.Response;

            if (!(settings is JsonFormatter jsonFormatter))
            {
                jsonFormatter = this.jsonFormatter;
            }

            contentType = GetNonEmptyString(contentType, response.ContentType, JSONUTF8ContentType);

            var encoding = MediaType.GetEncoding(contentType);

            response.ContentType = contentType;

            if (status is int status_code)
            {
                response.StatusCode = status_code;
            }

            var writer = new StreamWriter(response.Body, encoding);

            await jsonFormatter.SerializeAsync(value, writer);

            await writer.FlushAsync();
        }
    }

    sealed class AspNetJsonFormatter : IInputFormatter, IOutputFormatter
    {
        static AspNetJsonFormatter()
        {
            FastObjectRW.DefaultOptions =
                FastObjectRWOptions.IgnoreCase |
                FastObjectRWOptions.Property |
                FastObjectRWOptions.Field |
                FastObjectRWOptions.IndexId64 |
                FastObjectRWOptions.BasicTypeDirectCallMethod |
                FastObjectRWOptions.InheritedMembers |
                FastObjectRWOptions.SkipDefaultValue;
        }


        readonly JsonFormatter jsonFormatter;

        public AspNetJsonFormatter(JsonFormatter jsonFormatter)
        {
            this.jsonFormatter = jsonFormatter;
        }

        public bool CanRead(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;

            var contentType = request.ContentType;

            if (!string.IsNullOrEmpty(contentType))
            {
                var mediaType = new MediaType(contentType);

                return mediaType.SubType == "json";
            }

            return false;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;

            var contentType = request.ContentType;

            var encoding = MediaType.GetEncoding(contentType) ?? Encoding.UTF8;

            var reader = context.ReaderFactory(request.Body, encoding);

            var result = await jsonFormatter.DeserializeAsync(reader, context.ModelType);

            if (result == null && !context.TreatEmptyInputAsDefaultValue)
            {
                return InputFormatterResult.NoValue();
            }
            else
            {
                return InputFormatterResult.Success(result);
            }
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            var response = context.HttpContext.Response;

            var contentType = GetNonEmptyString(context.ContentType.Value, response.ContentType, JSONUTF8ContentType);

            var mediaType = new MediaType(contentType);

            return mediaType.SubType == "json";
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            var contentType = GetNonEmptyString(context.ContentType.Value, response.ContentType, JSONUTF8ContentType);

            response.ContentType = contentType;

            var encoding = MediaType.GetEncoding(contentType);

            var writer = context.WriterFactory(response.Body, encoding);

            await jsonFormatter.SerializeAsync(context.Object, writer);

            await writer.FlushAsync();
        }
    }
}
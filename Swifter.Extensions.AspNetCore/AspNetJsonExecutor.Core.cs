#if NETCOREAPP

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swifter.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using static SwifterExtensions;

sealed class AspNetJsonExecutor: IActionResultExecutor<JsonResult>
{
    public JsonFormatter DefaultJsonFormatter { get; private set; }

    public static AspNetJsonExecutor Create(JsonFormatter defaultJsonFormatter)
    {
        return new AspNetJsonExecutor { DefaultJsonFormatter = defaultJsonFormatter };
    }

    public void AddToSingleton(IServiceCollection services)
    {
        services.AddSingleton<IActionResultExecutor<JsonResult>>(this);
    }

    public async Task ExecuteAsync(ActionContext context, JsonResult result)
    {
        var response = context.HttpContext.Response;

        var jsonFormatter = (result.SerializerSettings as JsonFormatter) ?? DefaultJsonFormatter;

        var contentType = GetNonEmptyString(result.ContentType, response.ContentType, JSONContentType);
        var encoding = MediaType.GetEncoding(contentType) ?? jsonFormatter.Encoding;

        if (JSONContentType.Equals(contentType, StringComparison.InvariantCultureIgnoreCase))
        {
            contentType = $"{JSONContentType};charset={encoding.HeaderName}";
        }

        response.ContentType = contentType;

        if (result.StatusCode != null)
        {
            response.StatusCode = result.StatusCode.Value;
        }

        if (encoding == jsonFormatter.Encoding || encoding.Equals(jsonFormatter.Encoding))
        {
            var s = response.Body;

            await jsonFormatter.SerializeAsync(result.Value, s);

            await s.FlushAsync();
        }
        else
        {
            var sw = new StreamWriter(response.Body, encoding);

            await jsonFormatter.SerializeAsync(result, sw);

            await sw.FlushAsync();
        }
    }
}

#endif

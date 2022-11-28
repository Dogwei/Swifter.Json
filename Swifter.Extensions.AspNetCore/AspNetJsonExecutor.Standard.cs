#if NETSTANDARD

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swifter.Json;
using Swifter.Tools;
using System;
using System.IO;
using System.Threading.Tasks;
using static SwifterExtensions;

sealed class AspNetJsonExecutor : JsonResultExecutor, IActionResultExecutor<JsonResult>
{
    public JsonFormatter DefaultJsonFormatter { get; private set; }

    AspNetJsonExecutor() : base(default, default, default, default)
    {

    }


    public static AspNetJsonExecutor Create(JsonFormatter defaultJsonFormatter)
    {
        var executor = (AspNetJsonExecutor)TypeHelper.Allocate(typeof(AspNetJsonExecutor));

        executor.DefaultJsonFormatter = defaultJsonFormatter;

        return executor;
    }

    public void AddToSingleton(IServiceCollection services)
    {
        services.AddSingleton<IActionResultExecutor<JsonResult>>(this);
        services.AddSingleton<JsonResultExecutor>(this);
    }

    public override async Task ExecuteAsync(ActionContext context, JsonResult result)
    {
        var response = context.HttpContext.Response;

        var jsonFormatter = DefaultJsonFormatter;

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
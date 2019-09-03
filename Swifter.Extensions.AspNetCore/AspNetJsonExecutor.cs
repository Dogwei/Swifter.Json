#if NETCOREAPP || NETSTANDARD

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Swifter.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using static SwifterExtensions;

sealed class AspNetCoreJsonExecutor : IActionResultExecutor<JsonResult>
{
    public readonly JsonFormatter DefaultJsonFormatter;

    public AspNetCoreJsonExecutor(JsonFormatter defaultJsonFormatter)
    {
        DefaultJsonFormatter = defaultJsonFormatter;
    }

    public async Task ExecuteAsync(ActionContext context, JsonResult result)
    {
        var response = context.HttpContext.Response;

        var jsonFormatter =

#if NETCOREAPP 
            (result.SerializerSettings as JsonFormatter) ?? 
#endif
            DefaultJsonFormatter;

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
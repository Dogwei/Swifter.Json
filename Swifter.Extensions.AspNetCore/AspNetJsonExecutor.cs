using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Swifter.Json;
using Swifter.Reflection;
using System;
using System.IO;
using System.Threading.Tasks;
using static SwifterExtensions;

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

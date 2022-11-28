﻿#if NETCOREAPP || NETSTANDARD

using Microsoft.AspNetCore.Mvc.Formatters;
using Swifter.Json;
using System;
using System.Threading.Tasks;
using static SwifterExtensions;

sealed class AspNetJsonFormatter : IInputFormatter, IOutputFormatter
{
    readonly JsonFormatter jsonFormatter;

    public MediaTypeCollection SupportedMediaTypes { get; }

    public AspNetJsonFormatter(JsonFormatter jsonFormatter)
    {
        this.jsonFormatter = jsonFormatter;

        SupportedMediaTypes = new MediaTypeCollection();
    }

    public bool IsSupportedMediaType(MediaType mediaType)
    {
        return mediaType.SubType.EndsWith("json", StringComparison.InvariantCultureIgnoreCase);
    }

    public bool CanRead(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;

        var contentType = request.ContentType;

        return !string.IsNullOrEmpty(contentType) && IsSupportedMediaType(new MediaType(contentType));
    }

    public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;

        var contentType = request.ContentType;

        var encoding = MediaType.GetEncoding(contentType) ?? jsonFormatter.Encoding;

        object result;

        if (encoding == jsonFormatter.Encoding||encoding.Equals(jsonFormatter.Encoding))
        {
            result = await jsonFormatter.DeserializeAsync(request.Body, context.ModelType);
        }
        else
        {
            var tr = context.ReaderFactory(request.Body, encoding);

            result = await jsonFormatter.DeserializeAsync(tr, context.ModelType);
        }

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

        var contentType = GetNonEmptyString(context.ContentType.Value, response.ContentType, JSONContentType);

        return IsSupportedMediaType(new MediaType(contentType));
    }

    public async Task WriteAsync(OutputFormatterWriteContext context)
    {
        var response = context.HttpContext.Response;

        var contentType = GetNonEmptyString(context.ContentType.Value, response.ContentType, JSONContentType);

        var encoding = MediaType.GetEncoding(contentType) ?? jsonFormatter.Encoding;

        if (JSONContentType.Equals(contentType, StringComparison.InvariantCultureIgnoreCase))
        {
            contentType = $"{JSONContentType};charset={encoding.HeaderName}";
        }

        response.ContentType = contentType;

        if (encoding == jsonFormatter.Encoding || encoding.Equals(jsonFormatter.Encoding))
        {
            var s = response.Body;

            await jsonFormatter.SerializeAsync(context.Object, s);

            await s.FlushAsync();
        }
        else
        {
            var tw = context.WriterFactory(response.Body, encoding);

            await jsonFormatter.SerializeAsync(context.Object, tw);

            await tw.FlushAsync();
        }
    }
}

#endif
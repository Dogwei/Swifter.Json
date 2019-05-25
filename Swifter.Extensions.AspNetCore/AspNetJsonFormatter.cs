using Microsoft.AspNetCore.Mvc.Formatters;
using Swifter.Json;
using Swifter.RW;
using System.Text;
using System.Threading.Tasks;
using static SwifterExtensions;

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

#if NETFRAMEWORK

using Swifter.Json;
using System;
using System.Text;
using System.Web.Mvc;

using static SwifterExtensions;
/// <summary>
/// Swifter.Json 提供的快速 Json 返回值。
/// </summary>
public class SwifterJsonResult : JsonResult
{
    /// <summary>
    /// JsonFormatter 配置项。
    /// </summary>
    public JsonFormatterOptions Options { get; set; } = JsonFormatterOptions.Default;

    /// <summary>
    /// 调用 Swifter.Json 快速序列化程序。
    /// </summary>
    /// <param name="context">控制器上下文</param>
    public override void ExecuteResult(ControllerContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        var response = context.HttpContext.Response;

        var contentType = GetNonEmptyString(ContentType, response.ContentType, JSONContentType);
        var encoding = ContentEncoding ?? response.ContentEncoding ?? Encoding.UTF8;

        if (JSONContentType.Equals(contentType, StringComparison.InvariantCultureIgnoreCase))
        {
            contentType = $"{JSONContentType};charset={encoding.HeaderName}";
        }

        response.ContentType = contentType;

        if (Data == null)
        {
            return;
        }

        JsonFormatter.SerializeObject(Data, response.OutputStream, encoding, Options);
    }
}

#endif
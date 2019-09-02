
using Swifter.RW;

namespace Swifter.Formatters
{
    /// <summary>
    /// 表示格式化读取器，继承此接口以得到格式化强大的扩展功能。
    /// </summary>
    public interface IFormatterReader: IValueReader, ITargetedBind
    {
    }
}

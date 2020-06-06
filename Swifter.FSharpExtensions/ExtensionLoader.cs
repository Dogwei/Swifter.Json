using Swifter.FSharpExtensions;
using Swifter.RW;

namespace Swifter
{
    /// <summary>
    /// FSharp 扩展加载器。
    /// </summary>
    static class ExtensionLoader
    {
        static ExtensionLoader()
        {
            ValueInterface.AddMaper(new FSharpInterfaceMaper());
        }
    }
}

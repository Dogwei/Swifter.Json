
using Swifter.FSharpExtensions;
using Swifter.RW;

namespace Swifter
{
    /// <summary>
    /// FSharp 扩展加载器。
    /// </summary>
    static class ExtensionLoader
    {
        public static void Load()
        {
            ValueInterface.AddMaper(new FSharpInterfaceMaper());
        }
    }
}
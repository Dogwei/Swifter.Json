namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 表示忽略对指定程序集的访问检查的特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        /// <summary>
        /// 初始化特性。
        /// </summary>
        /// <param name="assemblyName">要忽略访问检查的程序集</param>
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        /// <summary>
        /// 要忽略访问检查的程序集。
        /// </summary>
        public string AssemblyName { get; }
    }
}
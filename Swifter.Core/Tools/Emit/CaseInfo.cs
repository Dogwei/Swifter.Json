using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示 Switch 的 Case 块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CaseInfo<T>
    {
        /// <summary>
        /// 获取 Case 块的值。
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// 获取 Case 块的指令标签。
        /// </summary>
        public readonly Label Label;

        /// <summary>
        /// 辅助变量。
        /// </summary>
        internal object? Tag;

        /// <summary>
        /// 实例化 Case 块。
        /// </summary>
        /// <param name="value">Case 块的值</param>
        /// <param name="label">Case 块的指令标签</param>
        public CaseInfo(T value, Label label)
        {
            Value = value;
            Label = label;
        }
    }
}
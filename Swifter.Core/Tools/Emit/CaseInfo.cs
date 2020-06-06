using System;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示 Switch 的 Case 块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CaseInfo<T> : IComparable<CaseInfo<T>>
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
        /// 获取或设置值的 HashCode 值。
        /// </summary>
        internal int HashCode;

        /// <summary>
        /// 辅助变量。
        /// </summary>
        internal object Tag;

        /// <summary>
        /// 实例化 Case 块。
        /// </summary>
        /// <param name="value">Case 块的值</param>
        /// <param name="label">ase 块的指令标签</param>
        public CaseInfo(T value, Label label)
        {
            Value = value;
            Label = label;
        }

        /// <summary>
        /// 与另一个 Case 块信息比较 HashCode 的大小。
        /// </summary>
        /// <param name="other">Case 块信息</param>
        /// <returns>返回大于 0 则比它大，小于 0 则比它小，否则一样大</returns>
        public int CompareTo(CaseInfo<T> other)
        {
            return HashCode.CompareTo(other.HashCode);
        }
    }
}
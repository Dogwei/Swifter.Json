using System;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 表示一个变量值。
    /// </summary>
    public sealed class VariableValue : IValue
    {
        private readonly Func<IValue> Getter;

        /// <summary>
        /// 获取该变量的当前值。
        /// </summary>
        public IValue Value => Getter();

        internal VariableValue(Func<IValue> getter)
        {
            Getter = getter;
        }
    }
}
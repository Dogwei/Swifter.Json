using System;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    partial class EmitHelper
    {
        private const int DifferenceSwitchMaxDepth = 2;

        /// <summary>
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="emitLoadItem">生成加载 Switch 项的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        public static void Switch(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, string> emitLoadItem,
            CaseInfo<string>[] cases,
            Label defaultLabel,
            bool ignoreCase)
        {

            var comparer = new StringDifferenceComparer(ignoreCase);

            try
            {
                DifferenceSwitch(
                    ilGen,
                    emitLoadValue,
                    emitLoadItem,
                    comparer,
                    cases,
                    defaultLabel);
            }
            catch
            {
                HashSwitch(
                    ilGen,
                    emitLoadValue,
                    emitLoadItem,
                    comparer,
                    cases,
                    defaultLabel);
            }
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="emitLoadItem">生成加载 Switch 项的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        public static void Switch(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, Ps<char>> emitLoadItem,
            CaseInfo<Ps<char>>[] cases,
            Label defaultLabel,
            bool ignoreCase)
        {
            var comparer = new Utf16sDifferenceComparer(ignoreCase);

            try
            {
                DifferenceSwitch(
                    ilGen,
                    emitLoadValue,
                    emitLoadItem,
                    comparer,
                    cases,
                    defaultLabel);
            }
            catch
            {
                HashSwitch(
                    ilGen,
                    emitLoadValue,
                    emitLoadItem,
                    comparer,
                    cases,
                    defaultLabel);
            }
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="emitLoadItem">生成加载 Switch 项的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        public static void Switch(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, Ps<Utf8Byte>> emitLoadItem,
            CaseInfo<Ps<Utf8Byte>>[] cases,
            Label defaultLabel,
            bool ignoreCase)
        {
            var comparer = new Utf8sDifferenceComparer(ignoreCase);

            try
            {
                DifferenceSwitch(
                    ilGen,
                    emitLoadValue,
                    emitLoadItem,
                    comparer,
                    cases,
                    defaultLabel);
            }
            catch
            {
                HashSwitch(
                    ilGen,
                    emitLoadValue,
                    emitLoadItem,
                    comparer,
                    cases,
                    defaultLabel);
            }
        }

        /// <summary>
        /// 生成 Switch(int) 代码块。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            CaseInfo<int>[] cases,
            Label defaultLabel)
        {
            cases = (CaseInfo<int>[])cases.Clone();

            Array.Sort(cases, (x, y) => x.Value.CompareTo(y.Value));

            if (IsSequence(cases, out var offset))
            {
                SwitchSequence(ilGen, emitLoadValue, cases, defaultLabel, offset);
            }
            else
            {
                SwitchNumber(
                    ilGen,
                    emitLoadValue,
                    (ilGen, value) => ilGen.LoadConstant(value),
                    cases,
                    defaultLabel,
                    0,
                    (cases.Length - 1) / 2,
                    cases.Length - 1);
            }
        }

        /// <summary>
        /// 生成 Switch(IntPtr) 代码块。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            CaseInfo<IntPtr>[] cases,
            Label defaultLabel)
        {
            cases = Unsafe.As<CaseInfo<IntPtr>[]>(cases.Clone());

            Array.Sort(cases, (Before, After) => ((long)Before.Value).CompareTo((long)After.Value));

            SwitchNumber(
                ilGen,
                emitLoadValue,
                (il, value) => { il.LoadConstant((long)value); il.ConvertPointer(); },
                cases,
                defaultLabel,
                0,
                (cases.Length - 1) / 2,
                cases.Length - 1
                );
        }

        /// <summary>
        /// 生成 Switch(long) 代码块。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            CaseInfo<long>[] cases,
            Label defaultLabel)
        {
            cases = (CaseInfo<long>[])cases.Clone();

            Array.Sort(cases, (before, after) => before.Value.CompareTo(after.Value));

            SwitchNumber(
                ilGen,
                emitLoadValue,
                (il, value) => { il.LoadConstant(value); },
                cases,
                defaultLabel,
                0,
                (cases.Length - 1) / 2,
                cases.Length - 1
                );
        }

        /// <summary>
        /// 生成指定类型的 Switch 代码块，使用差异位匹配算法。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="comparer">差异位比较器</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="emitLoadItem">生成加载指定 Case 块值的指定的委托</param>
        public static void DifferenceSwitch<T>(ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, T> emitLoadItem,
            IDifferenceComparer<T> comparer,
            CaseInfo<T>[] cases,
            Label defaultLabel)
        {

            var lengthGroup = new LengthCaseGroup<T>(cases, comparer);

            if (lengthGroup.GetDepth() > DifferenceSwitchMaxDepth)
            {
                throw new ArgumentException("Groups too deep.");
            }

            lengthGroup.Emit(ilGen, emitLoadValue, emitLoadItem, defaultLabel);
        }

        /// <summary>
        /// 生成指定类型的 Switch 代码块，使用 Hash 匹配算法。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLoadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="comparer">哈希比较器</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="emitLoadItem">生成加载指定 Case 块值的指定的委托</param>
        public static void HashSwitch<T>(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, T> emitLoadItem,
            IHashComparer<T> comparer,
            CaseInfo<T>[] cases,
            Label defaultLabel)
        {
            var hashCaces = cases
                .GroupBy(item => comparer.GetHashCode(item.Value))
                .Select(group => new CaseInfo<int>(group.Key, ilGen.DefineLabel()) { Tag = group.ToArray() })
                .ToArray();

            var hashLocal = ilGen.DeclareLocal(typeof(int));

            emitLoadValue(ilGen);
            comparer.EmitGetHashCode(ilGen);
            ilGen.StoreLocal(hashLocal);

            ilGen.Switch(ilGen => ilGen.LoadLocal(hashLocal), hashCaces, defaultLabel);

            foreach (var hashCase in hashCaces)
            {
                ilGen.MarkLabel(hashCase.Label);

                foreach (var itemCase in (CaseInfo<T>[])hashCase.Tag!)
                {
                    emitLoadValue(ilGen);
                    emitLoadItem(ilGen, itemCase.Value);
                    comparer.EmitEquals(ilGen);
                    ilGen.BranchTrue(itemCase.Label);
                }

                ilGen.Branch(defaultLabel);
            }
        }

        private static bool IsSequence<T>(CaseInfo<T>[] cases, out T offset) where T : unmanaged
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    break;
                default:
                    goto False;
            }

            if (cases.Length > 0)
            {
                offset = cases[0].Value;

                for (uint i = 0; i < cases.Length; i++)
                {
                    if (Convert.ToInt64(offset) + i != Convert.ToInt64(cases[i].Value))
                    {
                        goto False;
                    }
                }

                return true;
            }

            False:
            offset = default;

            return false;
        }

        private static void SwitchSequence<T>(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            CaseInfo<T>[] cases,
            Label defaultLabel,
            T offset = default) where T : unmanaged
        {
            emitLoadValue(ilGen);

            if (!TypeHelper.IsEmptyValue(offset))
            {
                var _offset = Convert.ToInt64(offset);

                if (_offset >= int.MinValue && _offset <= int.MaxValue)
                {
                    ilGen.LoadConstant((int)_offset);
                }
                else
                {
                    ilGen.LoadConstant(_offset);
                }

                ilGen.Subtract();

                ilGen.ConvertInt32();
            }

            ilGen.Switch(cases.Map(item => item.Label));

            ilGen.Branch(defaultLabel);
        }

        private static void SwitchNumber<T>(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, T> emitLoadItem,
            CaseInfo<T>[] cases,
            Label defaultLabel,
            int begin,
            int index,
            int end)
        {

            if (begin > end)
            {
                ilGen.Branch(defaultLabel);

                return;
            }

            if (begin == end)
            {
                emitLoadValue(ilGen);
                emitLoadItem(ilGen, cases[begin].Value);
                ilGen.BranchIfEqual(cases[begin].Label);

                ilGen.Branch(defaultLabel);

                return;
            }

            if (begin + 1 == end)
            {
                emitLoadValue(ilGen);
                emitLoadItem(ilGen, cases[begin].Value);
                ilGen.BranchIfEqual(cases[begin].Label);

                emitLoadValue(ilGen);
                emitLoadItem(ilGen, cases[end].Value);
                ilGen.BranchIfEqual(cases[end].Label);

                ilGen.Branch(defaultLabel);

                return;
            }

            var greaterLabel = ilGen.DefineLabel();

            emitLoadValue(ilGen);
            emitLoadItem(ilGen, cases[index].Value);
            ilGen.BranchIfGreater(greaterLabel);

            SwitchNumber(ilGen, emitLoadValue, emitLoadItem, cases, defaultLabel, begin, (begin + index) / 2, index);

            ilGen.MarkLabel(greaterLabel);

            SwitchNumber(ilGen, emitLoadValue, emitLoadItem, cases, defaultLabel, index + 1, (index + 1 + end) / 2, end);
        }
    }
}
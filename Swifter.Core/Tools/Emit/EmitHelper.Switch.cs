using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    partial class EmitHelper
    {
        /// <summary>
        /// 生成 Switch 算法时不做 Equals 验证，这是一个极端的性能优化，不建议开启。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool SwitchDoNotVerify = false;

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

            Array.Sort(cases, (x, y) => { return x.Value - y.Value; });

            if (IsSequence(cases, out var offset))
            {
                SwitchSequence(ilGen, emitLoadValue, cases, defaultLabel, offset);

                return;
            }

            SwitchNumber(
                ilGen, 
                emitLoadValue,
                (il, value) => { il.LoadConstant(value); },
                cases,
                defaultLabel,
                0, (cases.Length - 1) / 2, 
                cases.Length - 1);
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
            cases = Underlying.As<CaseInfo<IntPtr>[]>(cases.Clone());

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

        private static void DifferenceCasesProcess<TString>(
            ILGenerator iLGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator, TString> emitLoadItem,
            IDifferenceComparer<TString> comparer,
            Label defaultLabel,
            CaseInfo<int>[] differenceCases)
        {
            foreach (var item in differenceCases)
            {
                iLGen.MarkLabel(item.Label);

                if (item.Tag is SingleGroup<TString> singleGroup)
                {
                    if (SwitchDoNotVerify)
                    {
                        iLGen.Branch(singleGroup.Value.Label);
                    }
                    else
                    {
                        emitLoadValue(iLGen);
                        emitLoadItem(iLGen, singleGroup.Value.Value);
                        comparer.EmitEquals(iLGen);
                        iLGen.BranchTrue(singleGroup.Value.Label);

                        iLGen.Branch(defaultLabel);
                    }
                }
                else if (item.Tag is DifferenceGroup<TString> differenceGroup)
                {
                    var charCases = new CaseInfo<int>[differenceGroup.Groups.Count];

                    for (int i = 0; i < charCases.Length; i++)
                    {
                        charCases[i] = new CaseInfo<int>(differenceGroup.Groups[i].chr, iLGen.DefineLabel()) { Tag = differenceGroup.Groups[i].group };
                    }

                    Switch(iLGen, (ilGen2) =>
                    {
                        emitLoadValue(ilGen2);
                        ilGen2.LoadConstant(differenceGroup.Index);
                        comparer.EmitElementAt(iLGen);
                    }, charCases, defaultLabel);

                    DifferenceCasesProcess(iLGen, emitLoadValue, emitLoadItem, comparer, defaultLabel, charCases);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
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

            var lengthGroup = new LengthGroup<T>(cases, comparer);

            if (lengthGroup.GetDepth() > DifferenceSwitchMaxDepth)
            {
                throw new ArgumentException("Groups too deep.");
            }

            var lengthCases = new CaseInfo<int>[lengthGroup.Groups.Count];

            for (int i = 0; i < lengthCases.Length; i++)
            {
                lengthCases[i] = new CaseInfo<int>(lengthGroup.Groups[i].length, ilGen.DefineLabel()) { Tag = lengthGroup.Groups[i].group };
            }

            Switch(ilGen, (ilGen2) =>
            {
                emitLoadValue(ilGen2);
                comparer.EmitGetLength(ilGen2);
            }, lengthCases, defaultLabel);

            DifferenceCasesProcess(ilGen, emitLoadValue, emitLoadItem, comparer, defaultLabel, lengthCases);
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
            cases = (CaseInfo<T>[])cases.Clone();

            foreach (var item in cases)
            {
                item.HashCode = comparer.GetHashCode(item.Value);
            }

            Array.Sort(cases);

            var groupedCases = new Dictionary<int, List<CaseInfo<T>>>();

            foreach (var item in cases)
            {
                groupedCases.TryGetValue(item.HashCode, out var items);

                if (items is null)
                {
                    items = new List<CaseInfo<T>>
                    {
                        item
                    };

                    groupedCases.Add(item.HashCode, items);
                }
                else
                {
                    items.Add(item);
                }
            }

            var hashCodeLocal = ilGen.DeclareLocal(typeof(int));

            emitLoadValue(ilGen);
            comparer.EmitGetHashCode(ilGen);
            ilGen.StoreLocal(hashCodeLocal);

            SwitchObject(
                ilGen,
                emitLoadValue,
                il => il.LoadLocal(hashCodeLocal),
                emitLoadItem,
                comparer,
                groupedCases.ToList(),
                defaultLabel,
                0,
                (groupedCases.Count - 1) / 2,
                groupedCases.Count - 1);
        }

        private static bool IsSequence<T>(CaseInfo<T>[] cases, out T offset)
        {
            if (cases.Length> 0)
            {
                offset = cases[0].Value;

                for (uint i = 0; i < cases.Length; i++)
                {
                    if (XConvert<long>.Convert(offset) + i != XConvert<long>.Convert(cases[i].Value))
                    {
                        return false;
                    }
                }

                return true;
            }

            offset = default;

            return false;
        }

        private static void SwitchSequence<T>(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            CaseInfo<T>[] cases,
            Label defaultLabel,
            T offset = default)
        {
            emitLoadValue(ilGen);

            if (!TypeHelper.IsEmptyValue(offset))
            {
                if (Underlying.SizeOf<T>() <= sizeof(int))
                {
                    ilGen.LoadConstant(XConvert<int>.Convert(offset));
                }
                else
                {
                    ilGen.LoadConstant(XConvert<long>.Convert(offset));
                }

                ilGen.Subtract();
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

            if (begin == end)
            {
                emitLoadValue(ilGen);
                emitLoadItem(ilGen, cases[begin].Value);
                ilGen.BranchIfEqual(cases[begin].Label);

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

        private static void SwitchObject<T>(this ILGenerator ilGen,
            Action<ILGenerator> emitLoadValue,
            Action<ILGenerator> emitLoadHashCode,
            Action<ILGenerator, T> emitLoadItem,
            IHashComparer<T> comparer,
            List<KeyValuePair<int, List<CaseInfo<T>>>> cases,
            Label defaultLabel,
            int begin,
            int index,
            int end)
        {
            if (begin > end)
            {
                return;
            }

            if (begin == end)
            {
                emitLoadHashCode(ilGen);
                ilGen.LoadConstant(cases[begin].Key);
                ilGen.BranchIfNotEqualUnsigned(defaultLabel);

                if (SwitchDoNotVerify && cases[begin].Value.Count == 1)
                {
                    ilGen.Branch(cases[begin].Value[0].Label);
                }
                else
                {
                    foreach (var item in cases[begin].Value)
                    {
                        emitLoadValue(ilGen);
                        emitLoadItem(ilGen, item.Value);
                        comparer.EmitEquals(ilGen);
                        ilGen.BranchTrue(item.Label);
                    }
                }

                ilGen.Branch(defaultLabel);

                return;
            }

            if (begin + 1 == end)
            {

                var endLabel = ilGen.DefineLabel();

                emitLoadHashCode(ilGen);
                ilGen.LoadConstant(cases[begin].Key);
                ilGen.BranchIfNotEqualUnsigned(endLabel);

                if (SwitchDoNotVerify && cases[begin].Value.Count == 1)
                {
                    ilGen.Branch(cases[begin].Value[0].Label);
                }
                else
                {
                    foreach (var item in cases[begin].Value)
                    {
                        emitLoadValue(ilGen);
                        emitLoadItem(ilGen, item.Value);
                        comparer.EmitEquals(ilGen);
                        ilGen.BranchTrue(item.Label);
                    }
                }

                ilGen.MarkLabel(endLabel);

                emitLoadHashCode(ilGen);
                ilGen.LoadConstant(cases[end].Key);
                ilGen.BranchIfNotEqualUnsigned(defaultLabel);

                if (SwitchDoNotVerify && cases[end].Value.Count == 1)
                {
                    ilGen.Branch(cases[end].Value[0].Label);
                }
                else
                {
                    foreach (var item in cases[end].Value)
                    {
                        emitLoadValue(ilGen);
                        emitLoadItem(ilGen, item.Value);
                        comparer.EmitEquals(ilGen);
                        ilGen.BranchTrue(item.Label);
                    }
                }

                ilGen.Branch(defaultLabel);

                return;
            }

            var greaterLabel = ilGen.DefineLabel();

            emitLoadHashCode(ilGen);
            ilGen.LoadConstant(cases[index].Key);
            ilGen.BranchIfGreater(greaterLabel);

            SwitchObject(
                ilGen,
                emitLoadValue,
                emitLoadHashCode,
                emitLoadItem,
                comparer,
                cases,
                defaultLabel,
                begin,
                (begin + index) / 2,
                index);

            ilGen.MarkLabel(greaterLabel);

            SwitchObject(
                ilGen,
                emitLoadValue,
                emitLoadHashCode,
                emitLoadItem,
                comparer,
                cases,
                defaultLabel,
                index + 1,
                (index + 1 + end) / 2,
                end);
        }
    }
}
using System;
using System.Linq;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal class DifferenceCaseGroup<T> : ICaseGroup<T>
    {
        public readonly (int chr, ICaseGroup<T> group)[] Groups;

        public readonly int Index;

        public readonly IDifferenceComparer<T> Comparer;

        public DifferenceCaseGroup(CaseInfo<T>[] cases, IDifferenceComparer<T> comparer)
        {
            var length = cases.Min(str => comparer.GetLength(str.Value));

            var (index, groups) = Enumerable
                .Range(0, length)
                .Select(index => (index, cases.GroupBy(str => comparer.ElementAt(str.Value, index))))
                .OrderByDescending(item => item.Item2.Count())
                .First();

            Groups = groups
                .Select(item => (
                    item.Key,
                    item.Count() == 1
                        ? (ICaseGroup<T>)new SingleCaseGroup<T>(item.First(), comparer)
                        : new DifferenceCaseGroup<T>(item.ToArray(), comparer)))
                .ToArray();

            Index = index;

            Comparer = comparer;

            if (Groups.Length <= 1)
            {
                throw new ArgumentException("Repeated string");
            }
        }

        public void Emit(ILGenerator ilGen, Action<ILGenerator> emitLoadValue, Action<ILGenerator, T> emitLoadItem, Label defaultLabel)
        {
            var cases = Groups.Select(group => new CaseInfo<int>(group.chr, ilGen.DefineLabel()) { Tag = group.group }).ToArray();

            ilGen.Switch(ilGen =>
            {
                emitLoadValue(ilGen);
                ilGen.LoadConstant(Index);
                Comparer.EmitElementAt(ilGen);
            }, cases, defaultLabel);

            foreach (var item in cases)
            {
                ilGen.MarkLabel(item.Label);

                ((ICaseGroup<T>)item.Tag!).Emit(ilGen, emitLoadValue, emitLoadItem, defaultLabel);
            }
        }

        public int GetDepth() => Groups.Max(item => item.group.GetDepth()) + 1;
    }
}
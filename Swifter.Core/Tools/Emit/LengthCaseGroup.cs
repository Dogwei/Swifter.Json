using System;
using System.Linq;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal class LengthCaseGroup<T> : ICaseGroup<T>
    {
        public readonly (int length, ICaseGroup<T> group)[] Groups;
        public readonly IDifferenceComparer<T> Comparer;

        public LengthCaseGroup(CaseInfo<T>[] cases, IDifferenceComparer<T> comparer)
        {
            Groups = cases
                .GroupBy(item => comparer.GetLength(item.Value))
                .Select(item => (item.Key, item.Count() == 1 ? (ICaseGroup<T>)new SingleCaseGroup<T>(item.First(), comparer) : new DifferenceCaseGroup<T>(item.ToArray(), comparer)))
                .ToArray();

            Comparer = comparer;
        }

        public int GetDepth() => Groups.Max(item => item.group.GetDepth()) + 1;

        public void Emit(ILGenerator ilGen, Action<ILGenerator> emitLoadValue, Action<ILGenerator, T> emitLoadItem, Label defaultLabel)
        {
            var cases = Groups.Select(group => new CaseInfo<int>(group.length, ilGen.DefineLabel()) { Tag = group.group }).ToArray();

            var lengthLocal = ilGen.DeclareLocal(typeof(int));
            emitLoadValue(ilGen);
            Comparer.EmitGetLength(ilGen);
            ilGen.StoreLocal(lengthLocal);

            ilGen.Switch(ilGen => ilGen.LoadLocal(lengthLocal), cases, defaultLabel);

            foreach (var item in cases)
            {
                ilGen.MarkLabel(item.Label);

                ((ICaseGroup<T>)item.Tag!).Emit(ilGen, emitLoadValue, emitLoadItem, defaultLabel);
            }
        }
    }
}
using System;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal class SingleCaseGroup<T> : ICaseGroup<T>
    {
        public readonly CaseInfo<T> CaseInfo;

        public readonly IDifferenceComparer<T> Comparer;

        public SingleCaseGroup(CaseInfo<T> caseInfo, IDifferenceComparer<T> comparer)
        {
            CaseInfo = caseInfo;

            Comparer = comparer;
        }

        public void Emit(ILGenerator ilGen, Action<ILGenerator> emitLoadValue, Action<ILGenerator, T> emitLoadItem, Label defaultLabel)
        {
            emitLoadValue(ilGen);
            emitLoadItem(ilGen, CaseInfo.Value);
            Comparer.EmitEquals(ilGen);
            ilGen.BranchTrue(CaseInfo.Label);

            ilGen.Branch(defaultLabel);
        }

        public int GetDepth() => 0;
    }
}
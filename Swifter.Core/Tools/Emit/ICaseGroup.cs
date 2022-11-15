using System;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal interface ICaseGroup<T>
    {
        int GetDepth();

        void Emit(ILGenerator ilGen, Action<ILGenerator> emitLoadValue, Action<ILGenerator, T> emitLoadItem, Label defaultLabel);
    }
}
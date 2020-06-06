using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Test.WPF.Serializers
{
    public interface ISerializer
    {
        string Name { get; }
        Type SymbolsType { get; }
    }

    public abstract class BaseSerializer<TSymbols>: ISerializer
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public abstract TSymbols Serialize<TObject>(TObject obj);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public abstract TObject Deserialize<TObject>(TSymbols symbols);

        public Type SymbolsType => typeof(TSymbols);

        public virtual string Name => GetType().Name;
    }
}

using System;

namespace Swifter.RW
{
    internal static class StaticArrayRW<TArray> where TArray : class
    {
        public static readonly IArrayRWCreater<TArray> Creater;

        static StaticArrayRW()
        {
            if (typeof(TArray).IsArray)
            {
                Creater = (IArrayRWCreater<TArray>)Activator.CreateInstance(GetCreaterType());
            }
            else
            {
                throw new ArgumentException($"'{typeof(TArray).FullName}' is not a Array type.");
            }
        }

        public static Type GetCreaterType()
        {
            var elementType = typeof(TArray).GetElementType();
            var rank = typeof(TArray).GetArrayRank();

            switch (rank)
            {
                case 1:
                    return typeof(OneRankArrayRWCreater<>).MakeGenericType(elementType);
            }

            return typeof(MultRankArrayRWCreater<,>).MakeGenericType(typeof(TArray), elementType);
        }
    }
}
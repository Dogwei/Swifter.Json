using System;

namespace Swifter.RW
{
    internal static class StaticArrayRW<TArray> where TArray : class
    {
        public static readonly IArrayRWCreater<TArray> Creater;

        static StaticArrayRW()
        {
            var type = typeof(TArray);

            if (type.IsArray)
            {
                int rank = type.GetArrayRank();

                var elementType = type.GetElementType();

                Type internalType;

                switch (rank)
                {
                    case 1:
                        internalType = typeof(OneRankArrayRWCreater<>).MakeGenericType(elementType);
                        break;
                    case 2:
                        internalType = typeof(TwoRankArrayRWCreater<>).MakeGenericType(elementType);
                        break;
                    default:
                        internalType = typeof(MultiRankArrayRWCreater<,>).MakeGenericType(type, elementType);
                        break;
                }

                Creater = (IArrayRWCreater<TArray>)Activator.CreateInstance(internalType);
            }
            else
            {
                throw new ArgumentException($"'{typeof(TArray).FullName}' is not a Array type.");
            }
        }
    }
}
using Swifter.Tools;
using System;

namespace Swifter.Reflection
{
    sealed class SerializationArrayInfo
    {
        public readonly Type Type;

        public int Length;

        public int[]? Lengths;
        public int[]? LowerBounds;

        public SerializationArrayInfo(Array array)
        {
            Type = array.GetType();

            Length = array.Length;

            if (!Type.IsSZArray())
            {
                var rank = array.Rank;

                Lengths = new int[rank];

                for (int i = 0; i < rank; i++)
                {
                    Lengths[i] = array.GetLength(i);

                    var lowerBound = array.GetLowerBound(i);

                    if (lowerBound is not 0)
                    {
                        if (LowerBounds is null)
                        {
                            LowerBounds = new int[rank];
                        }

                        LowerBounds[i] = lowerBound;
                    }
                }
            }
        }

        public SerializationArrayInfo(Type type)
        {
            Type = type;
        }

        public Array CreateInstance()
        {
            if (Lengths is not null)
            {
                if (LowerBounds is not null)
                {
                    return Array.CreateInstance(Type.GetElementType()!, Lengths, LowerBounds);
                }
                else
                {
                    return ArrayHelper.CreateInstance(Type.GetElementType()!, Lengths);
                }
            }
            else
            {
                return Array.CreateInstance(Type.GetElementType()!, Length);
            }
        }
    }
}
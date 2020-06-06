

#if NET20 || NET35

using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable 1591

namespace System
{
    [Serializable]
    public struct ValueTuple
    {
    }

    [Serializable]
    public struct ValueTuple<T1>
    {
        public T1 Item1;
        
        public ValueTuple(T1 item1)
        {
            Item1 = item1;
        }
    }
    
    [Serializable]
    public struct ValueTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;
        
        public ValueTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
    
    [Serializable]
    public struct ValueTuple<T1, T2, T3, T4>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3, T4, T5>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3,T4,T5,T6>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3,T4,T5,T6,T7>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;
        public TRest Rest;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Rest = rest;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 指示应将所使用的成员上的值元组视为具有元素名称的元组。
    /// </summary>
    public sealed class TupleElementNamesAttribute : Attribute
    {
        /// <summary>
        /// 指示在类型构造的深度优先前序遍历中，哪个值元组元素应具有元素名称。
        /// </summary>
        public IList<string> TransformNames { get; }

        /// <summary>
        /// 初始化 System.Runtime.CompilerServices.TupleElementNamesAttribute 类的新实例。
        /// </summary>
        /// <param name="transformNames">一个字符串数组，该数组指示在类型构造的深度优先前序遍历中，哪个值元组事件应具有元素名称。</param>
        public TupleElementNamesAttribute(string[] transformNames)
        {
            TransformNames = transformNames;
        }
    }
}

#endif
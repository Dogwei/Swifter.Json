#if Dynamic

using System;
using System.Dynamic;

namespace Swifter.Data
{
    partial class DbRowObject : DynamicObject
    {
        static void AssertOneArguments(int length)
        {
            if (length != 1)
            {
                Throw();
            }

            static void Throw()
            {
                throw new NotSupportedException("One Arguments");
            }
        }


        bool GetIndexMap(object objectIndex, out int index)
        {
            index = -1;

            if (objectIndex == null)
            {
                return false;
            }

            if (objectIndex is int int32Index)
            {
                index = int32Index;

                return true;
            }

            if (objectIndex is string stringIndex && Map.FindIndex(stringIndex) is int indexOfName && indexOfName >= 0)
            {
                index = indexOfName;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试获取指定名称的成员的值。
        /// </summary>
        /// <param name="binder">包含名称信息</param>
        /// <param name="result">返回该成员的值</param>
        /// <returns>返回是否存在该成员</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var index = Map.FindIndex(binder.Name);

            if (index >= 0)
            {
                result = Values[index];

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        /// 尝试设置指定名称的成员的值。不存在将不会设置。
        /// </summary>
        /// <param name="binder">包含名称信息</param>
        /// <param name="value">值</param>
        /// <returns>返回是否存在该成员</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var index = Map.FindIndex(binder.Name);

            if (index >= 0)
            {
                Values[index] = value;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试获取指定索引的值。
        /// </summary>
        /// <param name="binder">包含索引信息</param>
        /// <param name="indexes">索引</param>
        /// <param name="result">返回该索引处的值</param>
        /// <returns>返回是否存在该索引</returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            AssertOneArguments(indexes.Length);

            if (GetIndexMap(indexes[0], out var index))
            {
                result = Values[index];

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        /// 尝试设置指定索引的值。不存在将不会设置。
        /// </summary>
        /// <param name="binder">包含索引信息</param>
        /// <param name="indexes">索引</param>
        /// <param name="value">值</param>
        /// <returns>返回是否存在该索引</returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            AssertOneArguments(indexes.Length);

            if (GetIndexMap(indexes[0], out var index))
            {
                Values[index] = value;

                return true;
            }

            return false;

        }
    }
}

#endif
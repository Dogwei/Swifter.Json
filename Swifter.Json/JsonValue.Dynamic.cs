
#if NET20 || NET30 || NET35


#else

using System;
using System.Dynamic;

namespace Swifter.Json
{
    sealed partial class JsonValue : DynamicObject
    {
        static void AssertIgnoreCase(bool value)
        {
            if (value)
            {
                Throw();
            }

            void Throw()
            {
                throw new NotSupportedException("Not support ignore case.");
            }
        }

        static void AssertOneArguments(int length)
        {
            if (length != 1)
            {
                Throw();
            }

            void Throw()
            {
                throw new NotSupportedException("Indexer must be one arguments.");
            }
        }

        /// <summary>
        /// Dynamic 对象尝试获取成员值。
        /// </summary>
        /// <param name="binder">成员信息</param>
        /// <param name="result">返回结果值</param>
        /// <returns>是否获取成功</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            AssertIgnoreCase(binder.IgnoreCase);

            var success = Object.TryGetValue(binder.Name, out result);

            result = AsValue(result);

            return success;
        }

        /// <summary>
        /// Dynamic 对象尝试获取索引器的值。
        /// </summary>
        /// <param name="binder">索引器信息</param>
        /// <param name="indexes">参数</param>
        /// <param name="result">返回结果值</param>
        /// <returns>是否获取成功</returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            AssertOneArguments(indexes.Length);

            var key = indexes[0];

            if (key is int int32Key)
            {
                result = Array[int32Key];

                result = AsValue(result);

                return true;
            }

            if (key is string stringKey)
            {
                var success = Object.TryGetValue(stringKey, out result);

                result = AsValue(result);

                return success;
            }

            result = null;

            return false;
        }

        private static object AsValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            var jsonValue = new JsonValue(value);

            if (jsonValue.IsValue)
            {
                return jsonValue.Value;
            }

            return jsonValue;
        }
    }
}

#endif
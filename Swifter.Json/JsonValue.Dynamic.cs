#if Dynamic

using System;
using System.Dynamic;

namespace Swifter.Json
{
    partial class JsonValue : DynamicObject
    {
        /// <summary>
        /// Dynamic 对象尝试获取成员值。
        /// </summary>
        /// <param name="binder">成员信息</param>
        /// <param name="result">返回结果值</param>
        /// <returns>是否获取成功</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = AsValue(this[binder.Name]);

            return result != null;
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
            if (indexes.Length != 1)
            {
                var key = indexes[0];

                if (key is int index)
                {
                    result = AsValue(this[index]);

                    return true;
                }

                if (key is string name)
                {
                    result = AsValue(this[name]);

                    return result != null;
                }
            }

            result = null;

            return false;
        }

        private static object AsValue(JsonValue value) => value != null && value.IsValue ? value.Value : value;
    }
}

#endif
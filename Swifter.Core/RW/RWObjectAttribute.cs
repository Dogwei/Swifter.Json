using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swifter.RW
{
    /// <summary>
    /// 对象读写器的特性形式配置项。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
    public class RWObjectAttribute : Attribute
    {
        /// <summary>
        /// 在创建对象读写器时的处理方法。
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="fields">对象字段集合</param>
        public virtual void OnCreate(Type type, ref List<IObjectField> fields)
        {
        }

        /// <summary>
        /// 在加载成员时的处理方法
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="attributes">成员的特性</param>
        public virtual void OnLoadMember(Type type, MemberInfo memberInfo, ref List<RWFieldAttribute> attributes)
        {

        }

        /// <summary>
        /// 是否忽略大小写。
        /// </summary>
        public RWBoolean IgnoreCace { get; set; }

        /// <summary>
        /// 是否字段未找到时发生异常。
        /// </summary>
        public RWBoolean NotFoundException { get; set; }

        /// <summary>
        /// 是否字段不能读取值时发生异常。
        /// </summary>
        public RWBoolean CannotGetException { get; set; }

        /// <summary>
        /// 是否字段不能写入值时发生异常。
        /// </summary>
        public RWBoolean CannotSetException { get; set; }

        /// <summary>
        /// 是否包含属性。
        /// </summary>
        public RWBoolean IncludeProperties { get; set; }

        /// <summary>
        /// 是否包含字段。
        /// </summary>
        public RWBoolean IncludeFields { get; set; }

        /// <summary>
        /// 是否包含继承的成员。
        /// </summary>
        public RWBoolean IncludeInherited { get; set; }

        /// <summary>
        /// 是否在 OnReadAll 中跳过具有类型默认值的成员。
        /// </summary>
        public RWBoolean SkipDefaultValue { get; set; }

        /// <summary>
        /// 是否在 OnReadAll 时只读取已定义 RWField(包括继承的类) 特性的成员。
        /// </summary>
        public RWBoolean MembersOptIn { get; set; }
    }
}
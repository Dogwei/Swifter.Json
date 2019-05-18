using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XTypeInfo 类型信息。
    /// 此类型信息主要提供该类型的成员的缓存。
    /// </summary>
    public sealed class XTypeInfo
    {
        internal const XBindingFlags RW = (XBindingFlags)0x10000000;


        private static Type[] ParametersToTypes(ParameterInfo[] parameters)
        {
            return ArrayHelper.Filter(parameters, item => true, item => item.ParameterType);
        }

        private static BindingFlags AsBindingFlags(XBindingFlags flags)
        {
            var result = BindingFlags.DeclaredOnly;

            if ((flags & XBindingFlags.Static) != 0)
            {
                result |= BindingFlags.Static;
            }

            if ((flags & XBindingFlags.Instance) != 0)
            {
                result |= BindingFlags.Instance;
            }

            if ((flags & XBindingFlags.Public) != 0)
            {
                result |= BindingFlags.Public;
            }

            if ((flags & XBindingFlags.NonPublic) != 0)
            {
                result |= BindingFlags.NonPublic;
            }

            return result;
        }

        /// <summary>
        /// 创建 XTypeInfo 类型信息。
        /// </summary>
        /// <param name="type">需要创建 XTypeInfo 类型信息的类型</param>
        /// <param name="flags">绑定参数</param>
        /// <returns>返回一个 XTypeInfo 类型信息</returns>
        public static XTypeInfo Create(Type type, XBindingFlags flags = XBindingFlags.Default) => XCache.Get(type)[flags];

        /// <summary>
        /// 创建 XTypeInfo 类型信息。
        /// </summary>
        /// <typeparam name="T">需要创建 XTypeInfo 类型信息的类型</typeparam>
        /// <param name="flags">绑定参数</param>
        /// <returns>返回一个 XTypeInfo 类型信息</returns>
        public static XTypeInfo Create<T>(XBindingFlags flags = XBindingFlags.Default) => XCache.Get<T>()[flags];


        private static void GetItems(Type type, XBindingFlags flags, List<XFieldInfo> fields, List<XPropertyInfo> properties, List<XIndexerInfo> indexers, List<XMethodInfo> methods)
        {
            if (type.BaseType != null)
            {
                GetItems(type.BaseType, flags, fields, properties, indexers, methods);
            }

            if ((flags & XBindingFlags.Field) != 0)
            {
                foreach (var item in type.GetFields(AsBindingFlags(flags)))
                {
                    fields.Add(XFieldInfo.Create(item, flags));
                }
            }

            if ((flags & (XBindingFlags.Property | XBindingFlags.Indexer)) != 0)
            {
                foreach (var item in type.GetProperties(AsBindingFlags(flags)))
                {
                    var parameters = item.GetIndexParameters();

                    if (parameters != null && parameters.Length != 0)
                    {
                        if ((flags & XBindingFlags.Indexer) != 0)
                        {
                            indexers.Add(XIndexerInfo.Create(item, flags));
                        }
                    }
                    else
                    {
                        if ((flags & XBindingFlags.Property) != 0)
                        {
                            properties.Add(XPropertyInfo.Create(item, flags));
                        }
                    }
                }
            }

            if ((flags & XBindingFlags.Method) != 0)
            {
                foreach (var item in type.GetMethods(AsBindingFlags(flags)))
                {
                    methods.Add(XMethodInfo.Create(item, flags));
                }
            }
        }

        private static void Switch(ref XBindingFlags flags, XBindingFlags target, RWBoolean boolean)
        {
            switch (boolean)
            {
                case RWBoolean.None:
                    break;
                case RWBoolean.Yes:
                    flags |= target;
                    break;
                case RWBoolean.No:
                    flags &= ~target;
                    break;
            }
        }

        /*
         * 
         * 为什么没有构造函数信息？
         * 
         * 因为系统的 System.Activator 已经非常优秀且方便了，所以没必要或者说我做不到更好的了。
         * 
         * Why no construct method informations?
         * 
         * Because System.Activator have very good, I can't do better.
         * 
         */

        internal readonly NameCache<XFieldInfo> fieldsCache;
        internal readonly NameCache<XPropertyInfo> propertiesCache;
        internal readonly HashCache<RuntimeParamsSign, XIndexerInfo> indexersCache;
        internal readonly HashCache<RuntimeMethodSign, XMethodInfo> methodsCache;
        internal readonly RWNameCache<IXFieldRW> rwFieldsCache;



        internal readonly XFieldInfo[] fields;
        internal readonly XPropertyInfo[] properties;
        internal readonly XIndexerInfo[] indexers;
        internal readonly XMethodInfo[] methods;
        internal readonly IXFieldRW[] rwFields;

        internal readonly Type type;
        internal readonly XBindingFlags flags;

        internal XTypeInfo(Type type, XBindingFlags flags)
        {
            RWObjectAttribute[] rwAttributes = null;

            if ((flags & RW) != 0)
            {
                rwAttributes = type.GetDefinedAttributes<RWObjectAttribute>(true);
            }

            if (rwAttributes != null && rwAttributes.Length != 0)
            {
                foreach (var item in rwAttributes)
                {
                    Switch(ref flags, XBindingFlags.RWIgnoreCase, item.IgnoreCace);

                    Switch(ref flags, XBindingFlags.RWNotFoundException, item.NotFoundException);

                    Switch(ref flags, XBindingFlags.RWCannotGetException, item.CannotGetException);

                    Switch(ref flags, XBindingFlags.RWCannotSetException, item.CannotSetException);

                    Switch(ref flags, XBindingFlags.Property, item.IncludeProperties);

                    Switch(ref flags, XBindingFlags.Field, item.IncludeFields);

                    Switch(ref flags, XBindingFlags.RWSkipDefaultValue, item.SkipDefaultValue);

                    Switch(ref flags, XBindingFlags.RWMembersOptIn, item.MembersOptIn);
                }
            }

            this.type = type;
            this.flags = flags;

            var fields = new List<XFieldInfo>();
            var properties = new List<XPropertyInfo>();
            var indexers = new List<XIndexerInfo>();
            var methods = new List<XMethodInfo>();
            var rwFields = new List<IXFieldRW>();

            GetItems(type, flags, fields, properties, indexers, methods);

            foreach (var item in fields)
            {
                if (item is IXFieldRW rwField)
                {
                    var attributes = item.FieldInfo.GetCustomAttributes(typeof(RWFieldAttribute), true);

                    if (attributes != null && attributes.Length != 0)
                    {
                        foreach (RWFieldAttribute attribute in attributes)
                        {
                            XAttributedFieldRW attributedFieldRW = (rwField, attribute);

                            if (attributedFieldRW.CanRead || attributedFieldRW.CanWrite)
                            {
                                rwFields.Add(attributedFieldRW);
                            }
                        }
                    }
                    else
                    {
                        rwFields.Add(rwField);
                    }
                }
            }

            foreach (var item in properties)
            {
                if (item is IXFieldRW rwField)
                {
                    var attributes = item.PropertyInfo.GetCustomAttributes(typeof(RWFieldAttribute), true);

                    if (attributes != null && attributes.Length != 0)
                    {
                        // Attributed property allow access non-Public accessor.
                        item.Initialize(item.propertyInfo, item.flags | XBindingFlags.NonPublic);

                        foreach (RWFieldAttribute attribute in attributes)
                        {
                            XAttributedFieldRW attributedFieldRW = (rwField, attribute);

                            if (attributedFieldRW.CanRead || attributedFieldRW.CanWrite)
                            {
                                rwFields.Add(attributedFieldRW);
                            }
                        }
                    }
                    else
                    {
                        rwFields.Add(rwField);
                    }
                }
            }

            rwFields.Sort((x, y) => x.Order.CompareTo(y.Order));

            if (rwAttributes != null && rwAttributes.Length != 0)
            {
                var temp = rwFields.Cast<IObjectField>().ToList();

                foreach (var item in rwAttributes)
                {
                    item.OnCreate(type, ref temp);
                }

                rwFields = temp.Cast<IXFieldRW>().ToList();
            }

            this.fields = fields.ToArray();
            this.properties = properties.ToArray();
            this.indexers = indexers.ToArray();
            this.methods = methods.ToArray();
            this.rwFields = rwFields.ToArray();

            fieldsCache = new NameCache<XFieldInfo>();
            propertiesCache = new NameCache<XPropertyInfo>();
            indexersCache = new HashCache<RuntimeParamsSign, XIndexerInfo>();
            methodsCache = new HashCache<RuntimeMethodSign, XMethodInfo>();
            rwFieldsCache = new RWNameCache<IXFieldRW>(flags);

            foreach (var item in fields)
            {
                fieldsCache[item.name] = item;
            }
            foreach (var item in properties)
            {
                propertiesCache[item.name] = item;
            }
            foreach (var item in indexers)
            {
                indexersCache[ParametersToTypes(item.PropertyInfo.GetIndexParameters())] = item;
            }
            foreach (var item in methods)
            {
                methodsCache[(item.MethodInfo.Name, ParametersToTypes(item.MethodInfo.GetParameters()))] = item;
            }
            foreach (var item in rwFields)
            {
                rwFieldsCache[item.Name] = item;
            }

            GC.Collect();
        }

        /// <summary>
        /// 获取表示当前 XTypeInfo 的类型。
        /// </summary>
        public Type Type => type;

        /// <summary>
        /// 获取创建 XTypeInfo 的绑定标识。
        /// </summary>
        public XBindingFlags Flags => flags;

        /// <summary>
        /// 获取字段集合。
        /// </summary>
        public XMemberCollection<XFieldInfo> Fields => fields;

        /// <summary>
        /// 获取属性集合。
        /// </summary>
        public XMemberCollection<XPropertyInfo> Properties => properties;

        /// <summary>
        /// 获取索引器集合。
        /// </summary>
        public XMemberCollection<XIndexerInfo> Indexers => indexers;

        /// <summary>
        /// 获取方法集合。
        /// </summary>
        public XMemberCollection<XMethodInfo> Methods => methods;

        /// <summary>
        /// 获取指定名称的字段。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回字段信息或 Null</returns>
        public XFieldInfo GetField(string name) => fieldsCache.GetValue(name);

        /// <summary>
        /// 获取指定名称的属性。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回属性信息或 Null</returns>
        public XPropertyInfo GetProperty(string name) => propertiesCache.GetValue(name);

        /// <summary>
        /// 获取指定参数类型的索引器。
        /// </summary>
        /// <param name="types">指定参数类型</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo GetIndexer(Type[] types) => indexersCache.GetValue(types);

        /// <summary>
        /// 获取指定参数的索引器。
        /// </summary>
        /// <param name="parameters">指定参数</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo GetIndexer(object[] parameters)=> indexersCache.GetValue(parameters);

        /// <summary>
        /// 获取指定名称和参数类型的方法信息。
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <param name="types">方法参数类型</param>
        /// <returns>返回方法信息或 Null</returns>
        public XMethodInfo GetMethod(string name, Type[] types) => methodsCache.GetValue((name, types));

        /// <summary>
        /// 获取指定名称和参数的方法信息。
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <param name="parameters">方法参数</param>
        /// <returns>返回方法信息或 Null</returns>
        public XMethodInfo GetMethod(string name, object[] parameters) => methodsCache.GetValue((name, parameters));
    }
}
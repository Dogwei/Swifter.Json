using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// XTypeInfo 类型信息。
    /// 此类型信息主要提供该类型的成员的缓存。
    /// </summary>
    public sealed class XTypeInfo
    {
        internal const XBindingFlags Flags_RW = (XBindingFlags)0x10000000;

        private static BindingFlags AsBindingFlags(XBindingFlags flags)
        {
            var result = BindingFlags.DeclaredOnly;

            if (flags.On(XBindingFlags.Static))
            {
                result |= BindingFlags.Static;
            }

            if (flags.On(XBindingFlags.Instance))
            {
                result |= BindingFlags.Instance;
            }

            if (flags.On(XBindingFlags.Public))
            {
                result |= BindingFlags.Public;
            }

            if (flags.On(XBindingFlags.NonPublic))
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
        public static XTypeInfo Create(Type type, XBindingFlags flags = XBindingFlags.Default) => XTypeInfoCollection.GetInstance(type).GetOrCreateXTypeInfo(flags);

        /// <summary>
        /// 创建 XTypeInfo 类型信息。
        /// </summary>
        /// <typeparam name="T">需要创建 XTypeInfo 类型信息的类型</typeparam>
        /// <param name="flags">绑定参数</param>
        /// <returns>返回一个 XTypeInfo 类型信息</returns>
        public static XTypeInfo Create<T>(XBindingFlags flags = XBindingFlags.Default) => XTypeInfoCollection.GetInstance<T>().GetOrCreateXTypeInfo(flags);


        private static void GetItems(
            Type type,
            XBindingFlags flags,
            List<XFieldInfo> fields,
            List<XPropertyInfo> properties,
            List<XEventInfo> events,
            List<XIndexerInfo> indexers,
            List<XMethodInfo> methods)
        {
            if (flags.On(XBindingFlags.Field))
            {
                foreach (var item in type.GetFields(AsBindingFlags(flags)))
                {
                    fields.Add(XFieldInfo.Create(item, flags));
                }
            }

            if (flags.On(XBindingFlags.Property | XBindingFlags.Indexer))
            {
                foreach (var item in type.GetProperties(AsBindingFlags(flags)))
                {
                    var parameters = item.GetIndexParameters();

                    if (parameters != null && parameters.Length != 0)
                    {
                        if (flags.On(XBindingFlags.Indexer))
                        {
                            indexers.Add(XIndexerInfo.Create(item, flags));
                        }
                    }
                    else
                    {
                        if (flags.On(XBindingFlags.Property))
                        {
                            properties.Add(XPropertyInfo.Create(item, flags));
                        }
                    }
                }
            }

            if (flags.On(XBindingFlags.Event))
            {
                foreach (var item in type.GetEvents(AsBindingFlags(flags)))
                {
                    events.Add(XEventInfo.Create(item, flags));
                }
            }

            if (flags.On(XBindingFlags.Method))
            {
                foreach (var item in type.GetMethods(AsBindingFlags(flags)))
                {
                    methods.Add(XMethodInfo.Create(item, flags));
                }
            }

            if (type.BaseType != null && flags.On(XBindingFlags.InheritedMembers))
            {
                GetItems(type.BaseType, flags, fields, properties, events, indexers, methods);
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

        internal readonly OpenDictionary<string, XFieldInfo> fields;
        internal readonly OpenDictionary<string, XPropertyInfo> properties;
        internal readonly OpenDictionary<string, XEventInfo> events;
        internal readonly List<XIndexerInfo> indexers;
        internal readonly OpenDictionary<string, XMethodInfo> methods;
        internal readonly List<XConstructorInfo> constructors;
        internal readonly OpenDictionary<string, IXFieldRW> rwFields;
#if IMMUTABLE_COLLECTIONS
        internal readonly ImmutableArray<string> rwKeys;
#else
        internal readonly ReadOnlyCollection<string> rwKeys;
#endif

        internal readonly Type type;
        internal readonly XBindingFlags flags;

        internal XTypeInfo(Type type, XBindingFlags flags)
        {
            Console.WriteLine(type);
            Console.WriteLine(type.GetFields().Length);
            Console.WriteLine(type.GetProperties().Length);

            RWObjectAttribute[]? rwAttributes = null;

            if (flags.On(Flags_RW))
            {
                if ((flags & ~Flags_RW) == XBindingFlags.UseDefault)
                {
                    flags = XObjectRW.GetDefaultBindingFlags(type) | Flags_RW;
                }

                rwAttributes = Unsafe.As<RWObjectAttribute[]>(type.GetCustomAttributes(typeof(RWObjectAttribute), true)); // TODO: 是否满足顺序需求

                if (rwAttributes.Length != 0)
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
            }

            this.type = type;
            this.flags = flags;

            var fields = new List<XFieldInfo>();
            var properties = new List<XPropertyInfo>();
            var events = new List<XEventInfo>();
            var indexers = new List<XIndexerInfo>();
            var methods = new List<XMethodInfo>();
            var constructors = new List<XConstructorInfo>();
            var rwFields = new List<IXFieldRW>();
            var rwStringComparer = flags.On(XBindingFlags.RWIgnoreCase) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

            GetItems(type, flags, fields, properties, events, indexers, methods);

            if (flags.On(XBindingFlags.Constructor))
            {
                foreach (var item in type.GetConstructors(AsBindingFlags(flags)))
                {
                    constructors.Add(XConstructorInfo.Create(item, flags));
                }
            }

            foreach (var item in fields)
            {
                if (item is IXFieldRW rwField && rwField.FieldType.CanBeGenericParameter())
                {
                    var attributes = new List<RWFieldAttribute>(
                        Unsafe.As<RWFieldAttribute[]>(
                            item.FieldInfo.GetCustomAttributes(typeof(RWFieldAttribute), true)
                            )
                        );

                    if (rwAttributes != null && rwAttributes.Length != 0)
                    {
                        foreach (var objectAttribute in rwAttributes)
                        {
                            objectAttribute.OnLoadMember(type, item.FieldInfo, attributes);
                        }
                    }

                    if (attributes != null && attributes.Count != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedFieldRW = item.WithAttribute(attribute);

                            if (attribute.Access is not RWFieldAccess.Ignore)
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
                if (item is IXFieldRW rwField && rwField.FieldType.CanBeGenericParameter())
                {
                    var attributes = new List<RWFieldAttribute>(
                        Unsafe.As<RWFieldAttribute[]>(
                            item.PropertyInfo.GetCustomAttributes(typeof(RWFieldAttribute), true)
                            )
                        );

                    if (rwAttributes != null && rwAttributes.Length != 0)
                    {
                        foreach (var objectAttribute in rwAttributes)
                        {
                            objectAttribute.OnLoadMember(type, item.PropertyInfo, attributes);
                        }
                    }

                    if (attributes != null && attributes.Count != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedFieldRW = item.WithAttribute(attribute);

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

            methods.Sort((x, y) => x.MethodInfo.Name.CompareTo(y.MethodInfo.Name));
            rwFields.Sort((x, y) => x.Order.CompareTo(y.Order));

            if (rwAttributes != null && rwAttributes.Length != 0)
            {
                var tempRWFields = new List<IObjectField>(rwFields.Count);

                foreach (var item in rwFields)
                {
                    tempRWFields.Add(item);
                }

                foreach (var item in rwAttributes)
                {
                    item.OnCreate(type, tempRWFields);
                }

                rwFields.Clear();

                foreach (var item in tempRWFields)
                {
                    rwFields.Add((IXFieldRW)item);
                }
            }

            this.fields = new();
            this.properties = new();
            this.events = new();
            this.indexers = new();
            this.methods = new();
            this.constructors = new();
            this.rwFields = new(rwStringComparer);

            // TODO: Capacity

            foreach (var item in fields)
            {
                this.fields.Add(item.Name, item);
            }
            foreach (var item in properties)
            {
                this.properties.Add(item.Name, item);
            }
            foreach (var item in events)
            {
                this.events.Add(item.Name, item);
            }
            foreach (var item in indexers)
            {
                this.indexers.Add(item);
            }
            foreach (var item in methods)
            {
                this.methods.Add(item.Name, item);
            }
            foreach (var item in constructors)
            {
                this.constructors.Add(item);
            }
            foreach (var item in rwFields)
            {
                if (this.rwFields.FindIndex(item.Name) >= 0)
                {
                    continue;
                }

                this.rwFields.Add(item.Name, item);
            }

            var rwKeys = new string[this.rwFields.Count];

            for (int i = 0; i < rwKeys.Length; i++)
            {
                rwKeys[i] = this.rwFields[i].Key;
            }

#if IMMUTABLE_COLLECTIONS
            this.rwKeys = ImmutableArray.CreateRange(rwKeys);
#else
            this.rwKeys = new ReadOnlyCollection<string>(rwKeys);
#endif
        }

        /// <summary>
        /// 获取表示当前 XTypeInfo 的类型。
        /// </summary>
        public Type Type => type;

        /// <summary>
        /// 获取创建 XTypeInfo 的绑定标识。
        /// </summary>
        public XBindingFlags Flags => flags & ~(Flags_RW);

        /// <summary>
        /// 获取字段数量。
        /// </summary>
        public int FieldsCount => fields.Count;

        /// <summary>
        /// 获取属性数量。
        /// </summary>
        public int PropertiesCount => properties.Count;

        /// <summary>
        /// 获取事件数量。
        /// </summary>
        public int EventsCount => events.Count;

        /// <summary>
        /// 获取索引器数量。
        /// </summary>
        public int IndexersCount => indexers.Count;

        /// <summary>
        /// 获取方法数量。
        /// </summary>
        public int MethodsCount => methods.Count;

        /// <summary>
        /// 获取构造函数的数量。
        /// </summary>
        public int ConstructorsCount => constructors.Count;

        /// <summary>
        /// 获取指定名称的字段信息。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回字段信息或 Null</returns>
        public XFieldInfo? GetField(string name) => fields.FindIndex(name) is int index && index >= 0 ? fields[index].Value : null;

        /// <summary>
        /// 获取指定索引处的字段信息。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回字段信息</returns>
        public XFieldInfo GetField(int index) => fields[index].Value;

        /// <summary>
        /// 获取指定名称的属性信息。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回属性信息或 Null</returns>
        public XPropertyInfo? GetProperty(string name) => properties.FindIndex(name) is int index && index >= 0 ? properties[index].Value : null;

        /// <summary>
        /// 获取指定索引处的属性信息。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回属性信息</returns>
        public XPropertyInfo GetProperty(int index) => properties[index].Value;

        /// <summary>
        /// 获取指定名称的事件信息。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回事件信息或 Null</returns>
        public XEventInfo? GetEvent(string name) => events.FindIndex(name) is int index && index >= 0 ? events[index].Value : null;

        /// <summary>
        /// 获取指定索引处的事件信息。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回事件信息</returns>
        public XEventInfo GetEvent(int index) => events[index].Value;

        /// <summary>
        /// 获取指定参数类型的索引器。
        /// </summary>
        /// <param name="types">指定参数类型</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo? GetIndexer(Type[] types)
        {
            foreach (var indexer in indexers)
            {
                if (indexer.Parameters.Equals(types))
                {
                    return indexer;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取指定索引处的索引器信息。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回索引器信息</returns>
        public XIndexerInfo GetIndexer(int index) => indexers[index];

        /// <summary>
        /// 获取指定名称和参数类型的方法信息。
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <param name="types">方法参数类型</param>
        /// <returns>返回方法信息或 Null</returns>
        public XMethodInfo? GetMethod(string name, Type[] types)
        {
            var index = methods.FindIndex(name);

            while (index >= 0)
            {
                if (methods[index].Value.Parameters.Equals(types))
                {
                    return methods[index].Value;
                }

                index = methods.NextIndex(index);
            }

            return null;
        }

        /// <summary>
        /// 获取指定索引处的方法信息。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回方法信息</returns>
        public XMethodInfo GetMethod(int index) => methods[index].Value;

        /// <summary>
        /// 获取指定名称的方法集合。
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>返回方法集合</returns>
        public XMethodInfo[]? GetMethods(string name)
        {
            var index = methods.FindIndex(name);
            var count = 0;

            if (index >= 0)
            {
                Loop:

                ++count;

                var nextIndex = methods.NextIndex(index);

                if (nextIndex is not -1)
                {
                    VersionDifferences.Assert(index == nextIndex + 1);

                    index = nextIndex;

                    goto Loop;
                }
            }

            if (count is 0)
            {
                return null;
            }

            var result = new XMethodInfo[count];

            for (int i = result.Length - 1; i >= 0; i--)
            {
                result[i] = methods[index].Value;

                ++index;
            }

            return result;
        }

        /// <summary>
        /// 获取指定索引处的构造函数信息。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回构造函数信息</returns>
        public XConstructorInfo GetConstructor(int index) => constructors[index];

        /// <summary>
        /// 获取指定参数类型的构造函数。
        /// </summary>
        /// <param name="types">指定参数类型</param>
        /// <returns>返回构造函数信息或 Null</returns>
        public XConstructorInfo? GetConstructor(Type[] types)
        {
            foreach (var constructor in constructors)
            {
                if (constructor.Parameters.Equals(types))
                {
                    return constructor;
                }
            }

            return null;
        }
    }
}
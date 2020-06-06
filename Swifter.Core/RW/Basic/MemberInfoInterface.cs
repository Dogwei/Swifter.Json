using Swifter.Tools;
using System;
using System.Linq;
using System.Reflection;

namespace Swifter.RW
{
    internal sealed class MemberInfoInterface<T> : IValueInterface<T> where T : MemberInfo
    {
        public const string IndexerName = ".indexor";
        public const string ConstructorName = ".ctor";

        public const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static readonly bool IsAssignableToMethodInfo = typeof(MethodInfo).IsAssignableFrom(typeof(T));
        public static readonly bool IsAssignableToFieldInfo = typeof(FieldInfo).IsAssignableFrom(typeof(T));
        public static readonly bool IsAssignableToPropertyInfo = typeof(PropertyInfo).IsAssignableFrom(typeof(T));
        public static readonly bool IsAssignableToConstructorInfo = typeof(ConstructorInfo).IsAssignableFrom(typeof(T));
        public static readonly bool IsAssignableToEventInfo = typeof(EventInfo).IsAssignableFrom(typeof(T));

        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader) return tReader.ReadValue();

            if (IsAssignableToMethodInfo && valueReader is IValueReader<MethodInfo> methodReader) return (T)(object)methodReader.ReadValue();
            if (IsAssignableToFieldInfo && valueReader is IValueReader<MethodInfo> fieldReader) return (T)(object)fieldReader.ReadValue();
            if (IsAssignableToPropertyInfo && valueReader is IValueReader<PropertyInfo> propertyReader) return (T)(object)propertyReader.ReadValue();
            if (IsAssignableToConstructorInfo && valueReader is IValueReader<ConstructorInfo> constructorReader) return (T)(object)constructorReader.ReadValue();
            if (IsAssignableToEventInfo && valueReader is IValueReader<EventInfo> eventReader) return (T)(object)eventReader.ReadValue();

            var value = valueReader.DirectRead();

            if (value is null)
            {
                return null;
            }

            if (RWHelper.CreateReader(value, false) is IDataReader<string> infoReader)
            {
                var declaringType = ValueInterface<Type>.ReadValue(infoReader["DeclaringType"]);
                var name = ValueInterface<string>.ReadValue(infoReader["Name"]);

                if (name == ConstructorName || IsAssignableToConstructorInfo)
                {
                    var parameterTypes = ValueInterface<Type[]>.ReadValue(infoReader["ParameterTypes"]);

                    var constructor = declaringType.GetConstructor(parameterTypes);

                    if (constructor != null && constructor is T ret)
                    {
                        return ret;
                    }
                }
                else if (name == IndexerName)
                {
                    var parameterTypes = ValueInterface<Type[]>.ReadValue(infoReader["ParameterTypes"]);
                    var propertyType = ValueInterface<Type>.ReadValue(infoReader["PropertyType"]);

                    var indexer = declaringType.GetProperty(parameterTypes);

                    if (indexer != null && indexer.PropertyType == propertyType && indexer is T ret)
                    {
                        return ret;
                    }
                }
                else if (IsAssignableToFieldInfo)
                {
                    var fieldType = ValueInterface<Type>.ReadValue(infoReader["FieldType"]);

                    var field = declaringType.GetField(name);

                    if (field != null && field.FieldType == fieldType && field is T ret)
                    {
                        return ret;
                    }
                }
                else if (IsAssignableToPropertyInfo)
                {
                    var propertyType = ValueInterface<Type>.ReadValue(infoReader["PropertyType"]);

                    var property = declaringType.GetProperty(name);

                    if (property != null && property.PropertyType == propertyType && property is T ret)
                    {
                        return ret;
                    }
                }
                else if (IsAssignableToEventInfo)
                {
                    var eventHandlerType = ValueInterface<Type>.ReadValue(infoReader["EventHandlerType"]);

                    var @event = declaringType.GetEvent(name);

                    if (@event != null && @event.EventHandlerType == eventHandlerType && @event is T ret)
                    {
                        return ret;
                    }
                }
                else if (IsAssignableToMethodInfo)
                {
                    goto Method;
                }
                else
                {
                    var member = declaringType.GetMember(name, Flags);

                    if (member.Length > 0)
                    {
                        switch (member[0].MemberType)
                        {
                            case MemberTypes.Event:
                                {
                                    var eventHandlerType = ValueInterface<Type>.ReadValue(infoReader["EventHandlerType"]);

                                    if (member[0] is EventInfo @event && @event.EventHandlerType == eventHandlerType && @event is T ret)
                                    {
                                        return ret;
                                    }
                                }
                                break;
                            case MemberTypes.Field:
                                {
                                    var fieldType = ValueInterface<Type>.ReadValue(infoReader["FieldType"]);

                                    if (member[0] is FieldInfo field && field.FieldType == fieldType && field is T ret)
                                    {
                                        return ret;
                                    }
                                }
                                break;
                            case MemberTypes.Property:
                                {
                                    var propertyType = ValueInterface<Type>.ReadValue(infoReader["PropertyType"]);

                                    if (member[0] is PropertyInfo property && property.PropertyType == propertyType && property is T ret)
                                    {
                                        return ret;
                                    }
                                }
                                break;
                            case MemberTypes.Method:
                                goto Method;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }

                NoFound:

                throw new MissingMemberException(declaringType.FullName, name);

                Method:
                {

                    var parameterTypes = ValueInterface<Type[]>.ReadValue(infoReader["ParameterTypes"]);
                    var returnType = ValueInterface<Type>.ReadValue(infoReader["ReturnType"]);

                    var method = declaringType.GetMethod(name, Flags, Type.DefaultBinder, parameterTypes, null);

                    if (method != null && method.ReturnType == returnType && method is T ret)
                    {
                        return ret;
                    }

                    goto NoFound;
                }
            }

            if (value is string memberName)
            {
                try
                {
                    if (Type.GetType(memberName) is T ret)
                    {
                        return ret;
                    }
                }
                catch
                {
                }
            }

            return XConvert<T>.FromObject(value);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);
            }
            else if (value is MethodInfo method)
            {
                if (valueWriter is IValueWriter<MethodInfo> methodWriter)
                {
                    methodWriter.WriteValue(method);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, new
                    {
                        method.DeclaringType,
                        method.Name,
                        ParameterTypes = method.GetParameters().Select(item => item.ParameterType),
                        method.ReturnType
                    });
                }
            }
            else if (value is FieldInfo field)
            {
                if (valueWriter is IValueWriter<FieldInfo> fieldWriter)
                {
                    fieldWriter.WriteValue(field);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, new
                    {
                        field.DeclaringType,
                        field.Name,
                        field.FieldType
                    });
                }
            }
            else if (value is PropertyInfo property)
            {
                if (valueWriter is IValueWriter<PropertyInfo> propertyWriter)
                {
                    propertyWriter.WriteValue(property);
                }
                else if (property.GetIndexParameters() is var parameters && parameters.Length > 0)
                {
                    ValueInterface.WriteValue(valueWriter, new
                    {
                        property.DeclaringType,
                        Name = IndexerName,
                        ParameterTypes = parameters.Select(item => item.ParameterType),
                        property.PropertyType
                    });
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, new
                    {
                        property.DeclaringType,
                        property.Name,
                        property.PropertyType
                    });
                }
            }
            else if (value is ConstructorInfo constructor)
            {
                if (valueWriter is IValueWriter<ConstructorInfo> constructorWriter)
                {
                    constructorWriter.WriteValue(constructor);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, new
                    {
                        constructor.DeclaringType,
                        Name = ConstructorName,
                        ParameterTypes = constructor.GetParameters().Select(item => item.ParameterType)
                    });
                }
            }
            else if (value is EventInfo @event)
            {
                if (valueWriter is IValueWriter<EventInfo> eventWriter)
                {
                    eventWriter.WriteValue(@event);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, new
                    {
                        @event.DeclaringType,
                        @event.Name,
                        @event.EventHandlerType
                    });
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
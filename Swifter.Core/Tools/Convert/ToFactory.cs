using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    sealed class ToFactory : IInternalXConverterFactory
    {
        static readonly (string Prefix, string Suffix)[] MethodNames = { ("To", "") };

        static bool MatchName(Type type, string name)
        {
            return true;
        }

        static bool MethodPredicate(MethodInfo method)
        {
            return MethodNames.Any(name 
                => method.Name.StartsWith(name.Prefix) 
                && method.Name.EndsWith(name.Suffix) 
                && MatchName(method.ReturnType, method.Name.Substring(name.Prefix.Length, method.Name.Length - (name.Prefix.Length + name.Suffix.Length)))
                );
        }

        static IEnumerable<MethodInfo> GetMethods(Type sourceType, Type destinationType)
        {
            var methods = sourceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => !method.IsGenericMethodDefinition && method.ReturnType != typeof(void) && method.GetParameters().Length == 0)
                .Where(MethodPredicate);

            var sourceAssemblyExtensionMethods = sourceType.Assembly.GetExportedTypes()
                .Where(type => type.IsDefined(typeof(ExtensionAttribute), false))
                .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.ReturnType != typeof(void) && method.GetParameters().Length == 1)
                .Where(MethodPredicate)
                .Select(ExtensionMethodHandler)
                .Where(method => !method.IsGenericMethodDefinition)
                ;

            var destinationAssemblyExtensionMethods = destinationType.Assembly.GetExportedTypes()
                .Where(type => type.IsDefined(typeof(ExtensionAttribute), false))
                .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.ReturnType != typeof(void) && method.GetParameters().Length == 1)
                .Where(MethodPredicate)
                .Select(ExtensionMethodHandler)
                .Where(method => !method.IsGenericMethodDefinition)
                ;

            return methods.Concat(sourceAssemblyExtensionMethods).Concat(destinationAssemblyExtensionMethods);


            MethodInfo ExtensionMethodHandler(MethodInfo method)
            {
                if (method.IsGenericMethodDefinition)
                {
                    var genericParameters = method.GetGenericArguments();

                    var parType = method.GetParameters()[0].ParameterType;
                    var argType = sourceType;

                    if (TypeHelper.SpeculateGenericParameters(parType, argType, genericParameters))
                    {
                        return method.MakeGenericMethod(genericParameters);
                    }
                }

                return method;
            }
        }

        public XConvertMode Mode => XConvertMode.Extended;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            return GetMethods(typeof(TSource), typeof(TDestination))
                .Where(method => SystemConvertFactory.GetComparison(method, typeof(TSource), typeof(TDestination)) <= 2)
                .OrderBy(method => SystemConvertFactory.GetComparison(method, typeof(TSource), typeof(TDestination)))
                .FirstOrDefault();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    internal sealed class ToConvert : BaseDynamicConvert
    {
        public const BindingFlags ToFlags = BindingFlags.Public | BindingFlags.Instance;

        public static readonly NameInfo[] ToNames =
            typeof(ToConvert).GetMethods(ToFlags)
            .Where(item => item.ReturnType != typeof(void) && item.GetParameters().Length == 0 && CreateNameInfo(item) != null)
            .Select(item => CreateNameInfo(item))
            .Distinct()
            .ToArray();

        public static NameInfo CreateNameInfo(MethodInfo item)
        {
            var split = item.Name.Split(new string[] { item.ReturnType.Name }, StringSplitOptions.None);

            if (split.Length == 2)
            {
                return new NameInfo { Before = split[0], After = split[1] };
            }

            return null;
        }

        public static IEnumerable<string> GetDestinationName(Type tDestination)
        {
            if (tDestination.IsArray)
            {
                yield return nameof(Array);

                foreach (var item in GetDestinationName(tDestination.GetElementType()))
                {
                    yield return item + nameof(Array);
                    yield return item + "s";
                    yield return item + "es";

                    if (item.EndsWith("y")) yield return item.Substring(0, item.Length - 1) + "ies";
                    if (item.EndsWith("f")) yield return item.Substring(0, item.Length - 1) + "ves";
                    if (item.EndsWith("fe")) yield return item.Substring(0, item.Length - 2) + "ves";
                }
            }

            yield return tDestination.Name;
        }

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            foreach (var item in ToNames)
            {
                foreach (var dName in GetDestinationName(tDestination))
                {
                    method = tSource.GetMethod(
                        item.Before + dName + item.After,
                        ToFlags,
                        Type.DefaultBinder,
                        Type.EmptyTypes,
                        null);

                    if (method != null && method.ReturnType == tDestination)
                    {
                        return true;
                    }
                }
            }

            method = null;

            return false;
        }

        public int ToInt32() => default;

        public int Int32 => default;

        public long Int64() => default;

        public sealed class NameInfo : IEquatable<NameInfo>
        {
            public string Before;

            public string After;

            public bool Equals(NameInfo other) => other != null && Before == other.Before && After == other.After;

            public override bool Equals(object obj) => obj is NameInfo other && Equals(other);

            public override int GetHashCode() => Before.GetHashCode() ^ After.GetHashCode();
        }
    }
}
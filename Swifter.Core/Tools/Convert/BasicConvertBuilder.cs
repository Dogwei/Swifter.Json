using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Swifter.Tools
{
    internal class BasicConvertBuilder
    {
        public static string Build()
        {
            var interfaceTemp = @"INewConvert<{TParam}, {TReturn}>";
            var methodTemp = @"        {TReturn} INewConvert<{TParam}, {TReturn}>.Convert({TParam} value) => {TDeclaringType}.{Name}(value);";

            var interfaceSplit = ", ";
            var methodSplit = "\r\n";

            var interfaceGroupSplit = "\r\n        ";
            var methodGroupSplit = "\r\n";

            var interfaces = new StringBuilder();
            var methods = new StringBuilder();

            FormatParams lastParam = null;

            var list = new List<MethodInfo>();

            list.AddRange(typeof(ConvertAdd).GetMethods());
            list.AddRange(typeof(Convert).GetMethods());

            list = list.Distinct(new ConvertMethodComparer()).ToList();

            list.Sort((x, y) => x.ReturnType.Name.CompareTo(y.ReturnType.Name));

            foreach (var item in list)
            {
                if (item.IsStatic && item.Name.StartsWith("To") && item.ReturnType != typeof(void) && item.GetParameters().Length == 1)
                {

                    var param = new FormatParams
                    {
                        TParam = item.GetParameters()[0].ParameterType.Name,
                        TReturn = item.ReturnType.Name,
                        TDeclaringType = item.DeclaringType.FullName,
                        Name = item.Name
                    };

                    if (lastParam == null || param.TReturn != lastParam.TReturn)
                    {
                        interfaces.Append(interfaceGroupSplit);
                        methods.Append(methodGroupSplit);
                    }

                    interfaces.Append(StringHelper.Format(interfaceTemp, param));
                    methods.Append(StringHelper.Format(methodTemp, param));

                    interfaces.Append(interfaceSplit);
                    methods.Append(methodSplit);

                    lastParam = param;
                }
            }

            interfaces.Length -= interfaceSplit.Length;
            methods.Length -= methodSplit.Length;


            var codeTemp = @"using System;

namespace Swifter.Tools
\{
    internal sealed class BasicConvert : 
{Interfaces}
    \{

        public static readonly BasicConvert Instance = new BasicConvert();

        private BasicConvert() \{}

{Methods}

    }
}
";

            var code = StringHelper.Format(codeTemp, new
            {
                Interfaces = interfaces.ToString(),
                Methods = methods.ToString()
            });

            return code.ToString();
        }

        public class ConvertMethodComparer : IEqualityComparer<MethodInfo>
        {
            public bool Equals(MethodInfo x, MethodInfo y)
            {
                if (x.ReturnType != y.ReturnType)
                {
                    return false;
                }

                var xp = x.GetParameters();
                var yp = y.GetParameters();

                if (xp.Length != yp.Length)
                {
                    return false;
                }

                for (int i = 0; i < xp.Length; i++)
                {
                    if (xp[i].ParameterType != yp[i].ParameterType)
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(MethodInfo obj)
            {
                var hash = obj.ReturnType.GetHashCode();

                foreach (var item in obj.GetParameters())
                {
                    hash ^= item.ParameterType.GetHashCode();
                }

                return hash;
            }
        }

        public class FormatParams
        {
            public string TParam { get; set; }

            public string TReturn { get; set; }

            public string Name { get; set; }

            public string TDeclaringType { get; set; }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Swifter.Tools
{
    sealed class ConvertMethodComparer : IEqualityComparer<MethodInfo>
    {
        public static readonly ConvertMethodComparer Instance = new ConvertMethodComparer();

        private ConvertMethodComparer()
        {

        }

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
}

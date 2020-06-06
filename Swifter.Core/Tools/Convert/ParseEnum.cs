using System;

namespace Swifter.Tools
{
    internal sealed class ParseEnum<TDestination> : IXConverter<string, TDestination> where TDestination:struct
    {
        public TDestination Convert(string value)
        {
#if FullParse

            if (Enum.TryParse<TDestination>(value, out var result))
            {
                return result;
            }

#endif
            return (TDestination)Enum.Parse(typeof(TDestination), value);
        }
    }
}
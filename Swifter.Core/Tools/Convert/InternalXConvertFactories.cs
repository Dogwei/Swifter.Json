using System.Collections.Generic;
using System.Reflection;

namespace Swifter.Tools
{
    static class InternalXConvertFactories
    {
        static readonly List<IXConverterFactory> factories;

        static InternalXConvertFactories()
        {
            factories = new()
            {
                new InverterFactory(),
                new SystemConvertFactory(),
                new ConstructorFactory(),
                new ParseFactory(),
                new ToFactory(),
                new ExplicitFactory(),
                new BasicExplicitConvert(),
                new ImplicitFactory(),
                new CovariantFactory(),
                new BasicImplicitFactory(),
                new NonConvertibleClass()
            };
        }

        public static void Add(IXConverterFactory factory)
        {
            lock (factories)
            {
                factories.Add(factory);
            }
        }

        public static InternalXConverter GetConverter<TSource, TDestination>()
        {
            XConvertMode mode = XConvertMode.Custom;

            MethodBase? method = null;

            for (int i = factories.Count - 1; i >= 0; --i)
            {
                var factory = factories[i];

                if (method is null)
                {
                    if (factory.GetConverter<TSource, TDestination>() is MethodBase converter)
                    {
                        method = converter;

                        if (factory is IInternalXConverterFactory internalFactory)
                        {
                            mode = internalFactory.Mode;

                            break;
                        }
                    }
                }
                else if (factory is IInternalXConverterFactory internalFactory)
                {
                    if (internalFactory.GetConverter<TSource, TDestination>() is not null)
                    {
                        mode = internalFactory.Mode;

                        break;
                    }
                }
            }

            return new InternalXConverter(method, mode, Func);

            static object? Func(object value)
            {
                return InternalXConvert<TSource, TDestination>.Convert((TSource)value);
            }
        }
    }
}
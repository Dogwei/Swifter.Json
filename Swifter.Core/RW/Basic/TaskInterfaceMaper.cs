using System;

#if Async
using System.Threading.Tasks;
#endif

namespace Swifter.RW
{
    internal sealed class TaskInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {

#if Async
            if (typeof(Task).IsAssignableFrom(typeof(T)))
            {
                var type = typeof(T);

                while (type != typeof(Task))
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var resultType = type.GetGenericArguments()[0];

                        return (IValueInterface<T>)Activator.CreateInstance(typeof(TaskInterface<,>).MakeGenericType(resultType, typeof(T)));
                    }

                    type = type.BaseType;
                }

                return (IValueInterface<T>)Activator.CreateInstance(typeof(TaskInterface<>).MakeGenericType(typeof(T)));
            }

#if ValueTask

            if (typeof(ValueTask).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)(object)new ValueTaskInterface();
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var resultType = typeof(T).GetGenericArguments()[0];

                return (IValueInterface<T>)Activator.CreateInstance(typeof(ValueTaskInterface<>).MakeGenericType(resultType));
            }


#endif

#endif

            return null;
        }
    }
}
#if Async
#pragma warning disable

using Swifter.Tools;
using System.Threading.Tasks;

namespace Swifter.RW
{
    internal sealed class TaskInterface<TTask> : IValueInterface<TTask> where TTask : Task
    {
        public TTask ReadValue(IValueReader valueReader)
        {
            var task = Run();

            if (task is TTask ret)
            {
                return ret;
            }

            return XConvert<TTask>.FromObject(task);

            async Task Run()
            {
                valueReader.DirectRead();
            }
        }

        public void WriteValue(IValueWriter valueWriter, TTask value)
        {
            try
            {
                value.Wait();

                ValueInterface.WriteValue(valueWriter, new
                {
                    value.AsyncState,
                    value.Status
                });
            }
            catch
            {
                ValueInterface.WriteValue(valueWriter, new
                {
                    value.AsyncState,
                    value.Exception.InnerException,
                    value.Status
                });
            }
        }
    }

    internal sealed class TaskInterface<TResult, TTask> : IValueInterface<TTask> where TTask : Task<TResult>
    {
        public TTask ReadValue(IValueReader valueReader)
        {
            var task = Run();

            if (task is TTask ret)
            {
                return ret;
            }

            return XConvert<TTask>.FromObject(task);

            async Task<TResult> Run()
            {
                return ValueInterface<TResult>.ReadValue(valueReader);
            }
        }

        public void WriteValue(IValueWriter valueWriter, TTask value)
        {
            try
            {
                value.Wait();

                ValueInterface.WriteValue(valueWriter, new
                {
                    value.AsyncState,
                    value.Result,
                    value.Status
                });
            }
            catch
            {
                ValueInterface.WriteValue(valueWriter, new
                {
                    value.AsyncState,
                    value.Exception.InnerException,
                    value.Status
                });
            }
        }
    }


#if ValueTask

    internal sealed class ValueTaskInterface : IValueInterface<ValueTask>
    {

        public async ValueTask ReadValue(IValueReader valueReader)
        {
            valueReader.DirectRead();
        }

        public void WriteValue(IValueWriter valueWriter, ValueTask value)
        {
            var task = value.AsTask();

            try
            {
                task.Wait();

                ValueInterface.WriteValue(valueWriter, new
                {
                    task.AsyncState,
                    task.Status
                });
            }
            catch
            {
                ValueInterface.WriteValue(valueWriter, new
                {
                    task.AsyncState,
                    task.Exception.InnerException,
                    task.Status
                });
            }
        }
    }

    internal sealed class ValueTaskInterface<TResult> : IValueInterface<ValueTask<TResult>>
    {
        public async ValueTask<TResult> ReadValue(IValueReader valueReader)
        {
            return ValueInterface<TResult>.ReadValue(valueReader);
        }

        public void WriteValue(IValueWriter valueWriter, ValueTask<TResult> value)
        {
            var task = value.AsTask();

            try
            {
                task.Wait();

                ValueInterface.WriteValue(valueWriter, new
                {
                    task.AsyncState,
                    task.Result,
                    task.Status
                });
            }
            catch
            {
                ValueInterface.WriteValue(valueWriter, new
                {
                    task.AsyncState,
                    task.Exception.InnerException,
                    task.Status
                });
            }
        }
    }

#endif

}
#endif
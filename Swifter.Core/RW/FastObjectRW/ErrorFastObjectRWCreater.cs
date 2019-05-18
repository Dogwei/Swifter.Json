using Swifter.Tools;
using System;
using System.Reflection;

namespace Swifter.RW
{
    internal sealed class ErrorFastObjectRWCreater<T> : IFastObjectRWCreater<T>
    {
        public Exception InnerException { get; set; }

        public ErrorFastObjectRWCreater(Exception innerException)
        {
            InnerException = innerException;
        }

        public FastObjectRW<T> Create()
        {
            throw new TargetException($"Failed to create FastObjectRW of '{typeof(T).FullName}' type.", InnerException);
        }
    }
}
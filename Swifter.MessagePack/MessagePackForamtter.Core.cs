#if NETCOREAPP && !NETCOREAPP2_0

using Swifter.RW;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Swifter.MessagePack
{

    partial class MessagePackForamtter
    {

        public T Deserialize<T>(ReadOnlySpan<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public void DeserializeTo(ReadOnlySpan<byte> bytes, IDataWriter dataWriter)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(ReadOnlySpan<byte> bytes, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
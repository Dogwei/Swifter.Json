#if NET45 || NET451 || NET47 || NET471 || NETSTANDARD || NETCOREAPP
using System;
using System.IO;
using System.Threading.Tasks;

namespace Swifter.MessagePack
{

    partial class MessagePackForamtter
    {

        public Task<T> DeserializeAsync<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<object> DeserializeAsync(Stream stream, Type type)
        {
            throw new NotImplementedException();
        }

        public Task SerializeAsync<T>(T value, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
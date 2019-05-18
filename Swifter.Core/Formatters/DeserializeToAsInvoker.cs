using Swifter.Tools;
using Swifter.Writers;
using System.IO;

namespace Swifter.Formatters
{
    sealed class DeserializeToAsInvoker : IGenericInvoker
    {
        readonly ITextFormatter textFormatter;

        readonly string text;
        readonly TextReader textReader;

        readonly IDataWriter dataWriter;

        public DeserializeToAsInvoker(ITextFormatter textFormatter, string text, IDataWriter dataWriter)
        {
            this.textFormatter = textFormatter;
            this.text = text;
            this.dataWriter = dataWriter;
        }

        public DeserializeToAsInvoker(ITextFormatter textFormatter, TextReader textReader, IDataWriter dataWriter)
        {
            this.textFormatter = textFormatter;
            this.textReader = textReader;
            this.dataWriter = dataWriter;
        }

        public void Invoke<Key>()
        {
            if (text != null)
            {
                textFormatter.DeserializeToWriter(text, (IDataWriter<Key>)dataWriter);
            }
            else
            {
                textFormatter.DeserializeToWriter(textReader, (IDataWriter<Key>)dataWriter);
            }
        }
    }
}

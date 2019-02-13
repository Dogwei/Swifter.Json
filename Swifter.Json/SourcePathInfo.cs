using Swifter.Writers;

namespace Swifter.Json
{
    sealed class SourcePathInfo
    {
        public string name;
        public int index;

        public SourcePathInfo next;

        public IDataWriter writer;

        public SourcePathInfo(string name, SourcePathInfo next, IDataWriter writer)
        {
            this.name = name;
            this.next = next;
            this.writer = writer;
        }

        public SourcePathInfo(int index, SourcePathInfo next, IDataWriter baseWriter)
        {
            this.index = index;
            this.next = next;
            this.writer = baseWriter;
        }
    }
}

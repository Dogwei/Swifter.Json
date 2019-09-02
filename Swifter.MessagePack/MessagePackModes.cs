using Swifter.Tools;


namespace Swifter.MessagePack
{
    static class MessagePackModes
    {
        public struct Simple { }

        public struct Standard { }

        public struct Reference
        {
            public ReferenceCache<int> References;
        }
    }
}
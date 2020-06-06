using System.Collections.Generic;

namespace Swifter.MessagePack
{
    static class MessagePackDeserializeModes
    {
        public struct StandardMode { }

        public struct ReferenceMode
        {
            public Dictionary<int, object> References;
        }
    }
}
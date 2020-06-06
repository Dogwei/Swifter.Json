using Swifter.Tools;
using System.Collections.Generic;

namespace Swifter.MessagePack
{
    static class MessagePackSerializeModes
    {
        public struct StandardMode { }

        public struct ReferenceMode
        {
            public Dictionary<object, int> References;
        }
    }
}
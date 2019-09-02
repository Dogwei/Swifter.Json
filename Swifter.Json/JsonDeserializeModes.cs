
using Swifter.RW;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    static class JsonDeserializeModes
    {
        public struct Deflate
        {

        }

        public struct Standard
        {

        }

        public struct Verified
        {

        }

        public struct Reference
        {
            LinkedList<Item> items;
            IDataReader root;
            JsonReference jsonReference;

            public bool IsJsonReference => jsonReference != null || (items != null && items.Count != 0 && items.Last.Value.Writer is IDataReader reader && reader.ReferenceToken == null);

            public bool IsRoot => root == null;
            
            public void SetRoot(IDataWriter dataWriter)
            {
                if (dataWriter is IDataReader reader)
                {
                    root = reader;
                }
                else
                {
                    root = RWHelper.CreateReader(RWHelper.GetContent<object>(dataWriter));
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void SetItem(IDataWriter writer, RWPathInfo destination)
            {
                if (items == null)
                {
                    items = new LinkedList<Item>();
                }

                if (jsonReference != null)
                {
                    items.AddLast(new Item(jsonReference, writer, destination));

                    jsonReference = null;
                }
                else if (items != null && items.Count != 0 && items.Last.Value.Writer is IDataReader reader)
                {
                    var jsonReference = new JsonReference(reader, RWPathInfo.Root);

                    items.AddLast(new Item(jsonReference, writer, destination));
                }
            }
            
            public JsonReference CreateJsonReference(RWPathInfo reference)
            {
                if (root == null)
                {
                    throw new NotSupportedException("Json root is not initialized.");
                }

                var result = new JsonReference(root, reference);

                // if (!result.TryGetValue(out var value) || value is ValueType)
                {
                    jsonReference = result;
                }

                return result;
            }
            
            public void Process()
            {
                while (items != null && items.Count != 0)
                {
                    var finish = true;
                    var node = items.First;
                Loop:
                    if (node != null && node.Value.Source.TryGetValue(out var value))
                    {
                        node.Value.Destination.SetValue(node.Value.Writer, value);

                        var next = node.Next;

                        items.Remove(node);

                        node = next;

                        finish = false;

                        goto Loop;
                    }

                    if (finish)
                    {
                        break;
                    }
                }
            }

            sealed class Item
            {
                public readonly JsonReference Source;
                public readonly IDataWriter Writer;
                public readonly RWPathInfo Destination;

                public Item(JsonReference source, IDataWriter writer, RWPathInfo destination)
                {
                    Source = source;
                    Writer = writer;
                    Destination = destination;
                }
            }
        }
    }
}
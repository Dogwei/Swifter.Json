using Swifter.RW;

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
            LinkedItem first;
            LinkedItem last;

            LinkedObject root;
            LinkedObject curr;

            public void EnterObject(IDataWriter dataWriter)
            {
                var item = new LinkedObject(dataWriter);

                if (root is null)
                {
                    root = item;
                    curr = item;
                }
                else
                {
                    item.Prev = curr;
                    curr = item;
                }
            }

            public IDataWriter CurrentObject 
                => curr.DataWriter;

            public void LeavaObject()
            {
                if (curr == root)
                {
                    Process();

                    root = null;
                }
                else if(curr.Count > 0 && curr.DataWriter.ContentType?.IsValueType == true)
                {
                    AddItem(new LinkedItem(
                        curr.DataWriter,
                        RWPathInfo.Root, 
                        curr.Prev.DataWriter,
                        curr.Prev.CurrentKey.Clone()
                        ));
                }

                curr = curr.Prev;
            }

            private void AddItem(LinkedItem item)
            {
                ++curr.Count;

                if (first is null)
                {
                    first = item;
                    last = item;
                }
                else
                {
                    last.Next = item;
                    last = item;
                }
            }

            public void SetCurrentKey<TKey>(TKey key)
            {
                if (curr.CurrentKey is null)
                {
                    curr.CurrentKey = RWPathInfo.Create(key);
                }
                else
                {
                    RWPathInfo.SetPath(curr.CurrentKey, key);
                }
            }

            public object GetValue(RWPathInfo reference)
            {
                AddItem(new LinkedItem(
                    root.DataWriter, 
                    reference, 
                    curr.DataWriter,
                    curr.CurrentKey.Clone()
                    ));

                return null;
            }

            private void Process()
            {
                while (first != null)
                {
                    first.Process();

                    first = first.Next;
                }

                last = null;
            }

            sealed class LinkedItem
            {
                public readonly IDataWriter Source;
                public readonly RWPathInfo SourcePath;
                public readonly IDataWriter Destination;
                public readonly RWPathInfo DestinationPath;

                public LinkedItem Next;

                public LinkedItem(IDataWriter source, RWPathInfo sourcePath, IDataWriter destination, RWPathInfo destinationPath)
                {
                    Source = source;
                    SourcePath = sourcePath;
                    Destination = destination;
                    DestinationPath = destinationPath;
                }

                public void Process()
                {
                    if (SourcePath.IsRoot)
                    {
                        DestinationPath.GetValueWriter(Destination).DirectWrite(Source.Content);
                    }
                    else
                    {
                        var dataReader = Source as IDataReader ?? RWHelper.CreateReader(Source.Content, false);

                        if (dataReader != null)
                        {
                            DestinationPath.GetValueWriter(Destination).DirectWrite(SourcePath.GetValueReader(dataReader).DirectRead());
                        }
                    }
                }
            }

            sealed class LinkedObject
            {
                public readonly IDataWriter DataWriter;
                public RWPathInfo CurrentKey;

                public int Count;

                public LinkedObject Prev;

                public LinkedObject(IDataWriter dataWriter)
                {
                    DataWriter = dataWriter;
                }
            }
        }
    }
}
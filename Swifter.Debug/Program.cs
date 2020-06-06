using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using Swifter.Reflection;
using Swifter.Json;
using Swifter.RW;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Reflection.Emit;
using Swifter.Tools;
using System.Data;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using Swifter.MessagePack;
using System.Runtime.Serialization.Formatters;
using Swifter.Test.WPF;
using Swifter.Test.WPF.Tests;
using System.Collections;
using SpanJson;
using Swifter.Test.WPF.Models;
using Swifter.Data;
using MessagePack;
using System.Text.Unicode;
using Newtonsoft.Json;

namespace Swifter.Debug
{
	public sealed unsafe class Program
	{
		public static void Main()
        {
            EmitHelper.SwitchDoNotVerify = true;

            FastObjectRW.DefaultOptions &= ~FastObjectRWOptions.IgnoreCase;

            var catalog = new CatalogModel().GetObject();

            var json = JsonFormatter.SerializeObject(catalog, JsonFormatterOptions.IgnoreNull | JsonFormatterOptions.IgnoreZero | JsonFormatterOptions.IgnoreEmptyString);

            while (true)
            {
                fixed(char* chars = json)
                {
                    var stopwatch = Stopwatch.StartNew();

                    for (int i = 0; i < 100; i++)
                    {
                        ReadJson(JsonFormatter.CreateJsonReader(chars, json.Length));
                    }

                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                }

            }
        }

        public static void ReadJson(IJsonReader jsonReader)
        {
            switch (jsonReader.GetToken())
            {
                case Json.JsonToken.Object:

                    jsonReader.TryReadBeginObject();

                    while (!jsonReader.TryReadEndObject())
                    {
                        jsonReader.ReadPropertyName();

                        ReadJson(jsonReader);
                    }

                    break;
                case Json.JsonToken.Array:

                    jsonReader.TryReadBeginArray();

                    while (!jsonReader.TryReadEndArray())
                    {
                        ReadJson(jsonReader);
                    }

                    break;
                case Json.JsonToken.End:
                    return;
                default:
                    jsonReader.DirectRead();
                    break;
            }
        }
    }
}
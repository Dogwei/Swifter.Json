using Swifter.Json;
using Swifter.Reflection;
using Swifter.RW;
using Swifter.Test;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

public class Demo
{
    public string Name { get; set; }

    public static void Main()
    {
        //JsonFormatter.CharsPool.Ratio = 0;
        //FastObjectRW.DefaultOptions &= ~FastObjectRWOptions.IgnoreCase;

        //Application.Run(new MyForm());

        //ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);

        //var text = Encoding.UTF8.GetString(Resource.catalog);

        //var obj = JsonFormatter.DeserializeObject<Catalog>(text);

        //while (true)
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    for (int i = 0; i < 15; i++)
        //    {
        //        JsonFormatter.SerializeObject(obj);
        //    }

        //    Console.WriteLine(stopwatch.ElapsedMilliseconds);
        //}
    }
}

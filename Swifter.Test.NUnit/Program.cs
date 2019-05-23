using Swifter.Json;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Text;

public class Demo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public static void Main()
    {
        JsonFormatter.DeserializeObject<Demo>(json);
    }
}
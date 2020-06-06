using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Test.WPF.Models.Polymorphism
{
    [MessagePackObject(true)]
    public class Root
    {
        public int Count { get; set; }

        public int Id { get; set; }
    }

    [MessagePackObject(true)]
    public class Base : Root
    {
        public string Name { get; set; }
    }

    [MessagePackObject(true)]
    public class Polymorphism : Base
    {
        public new int Id { get; set; }
    }
}

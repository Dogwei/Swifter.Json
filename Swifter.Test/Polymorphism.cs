namespace Swifter.Test
{

    public class Root
    {
        public int Count { get; set; }

        public int Id { get; set; }
    }

    public class Base : Root
    {
        public string Name { get; set; }
    }

    public class Polymorphism : Base
    {
        public new int Id { get; set; }
    }
}

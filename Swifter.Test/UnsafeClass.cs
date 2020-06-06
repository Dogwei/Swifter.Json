namespace Swifter.Test
{
    unsafe class UnsafeClass
    {
        public string Name { get; set; }

        public int Count { get; set; }

        public void* Pointer { get; set; }

        public void* Address { get; set; }
    }
}
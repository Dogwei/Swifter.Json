namespace Swifter.Json
{
    internal enum JsonValueTypes : byte
    {
        String = 1,
        Number = 2,
        Object = 3,
        Array = 4,
        True = 5,
        False = 6,
        Null = 7,
        Undefined = 0
    }
}
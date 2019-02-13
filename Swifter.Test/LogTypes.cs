namespace Swifter.Test
{
    public enum LogTypes
    {
        OnlyFirst = 1,
        OnlyTheNext = 2,
        All = OnlyFirst | OnlyTheNext
    }
}
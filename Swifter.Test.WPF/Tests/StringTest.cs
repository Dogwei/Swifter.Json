namespace Swifter.Test.WPF.Tests
{
    public class StringTest : BaseTest<string>
    {
        public override string GetObject()
        {
            return new RandomValueReader(1218).ReadString();
        }
    }

}
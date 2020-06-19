using Swifter.RW;
using Swifter.Test.WPF.Models.Polymorphism;

namespace Swifter.Test.WPF.Tests
{
    public class PolymorphismTest : BaseTest<Polymorphism>
    {
        public override Polymorphism GetObject()
        {
            return new RandomValueReader(1218).FastReadObject<Polymorphism>();
        }
    }

}
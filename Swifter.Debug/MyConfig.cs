#if BenchmarkDotNet

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace Swifter.Debug
{
    public class MyConfig : ManualConfig
    {
        public MyConfig()
        {
            AddJob(Job.Default.WithUnrollFactor(2));

            AddDiagnoser(MemoryDiagnoser.Default);

            Orderer = new DefaultOrderer(SummaryOrderPolicy.Method);
        }
    }
}
#endif
using BenchmarkDotNet.Running;

namespace DotNet.Ildasm.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<AutoIndentationOverhead>();
        }
    }
}

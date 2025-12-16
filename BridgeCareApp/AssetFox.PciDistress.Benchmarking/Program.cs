using BenchmarkDotNet.Running;

namespace AssetFox.PciDistress.Benchmarks
{
    internal static class Program
    {
        private static void Main() => _ = BenchmarkRunner.Run<CharacterizationTestInputs>();
    }
}

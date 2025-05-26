using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using DotNet.Ildasm.Adapters;
using Mono.Cecil;
using static BenchmarkDotNet.Toolchains.DotNetCli.NetCoreAppSettings;

namespace DotNet.Ildasm.Benchmarks
{
    public class MultipleRuntimes : ManualConfig
    {
        public MultipleRuntimes()
        {
            AddJob(Job.Default.WithToolchain(CsProjCoreToolchain.From(NetCoreApp80)));
            AddJob(Job.Default.WithToolchain(CsProjCoreToolchain.From(NetCoreApp90)));
        }
    }

    [Config(typeof(MultipleRuntimes))]
    public class AutoIndentationOverhead
    {
        internal static readonly Lazy<AssemblyDefinition> SampleAssembly = new(static () =>
            AssemblyDefinition.ReadAssembly(
                typeof(Program).Assembly.GetManifestResourceStream(
                    "dotnet-ildasm.Benchmarks.Sample.dotnet-ildasm.Sample.exe")));

        private static readonly MethodDefinition MethodDefinition;

        static AutoIndentationOverhead()
        {
            var type = SampleAssembly.Value.MainModule.Types.FirstOrDefault(static x => x.Name == "PublicClass");
            MethodDefinition = type.Methods.FirstOrDefault(static x => x.Name == "UsingTryCatch");
        }

        [Benchmark]
        public void NoIndentation()
        {
            using var fileStreamOutputWriter = new FileStreamOutputWriter(Path.GetTempFileName());
            MethodDefinition.WriteILBody(fileStreamOutputWriter);
        }

        [Benchmark]
        public void AutoIndentation()
        {
            using var outputWriter = new FileStreamOutputWriter(Path.GetTempFileName());
            using var autoIndentOutputWriter = new AutoIndentOutputWriter(outputWriter);
            MethodDefinition.WriteILBody(autoIndentOutputWriter);
        }
    }
}

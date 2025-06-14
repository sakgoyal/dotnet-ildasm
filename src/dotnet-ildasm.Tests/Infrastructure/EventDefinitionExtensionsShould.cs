using System.Linq;
using DotNet.Ildasm.Tests.Internal;
using Mono.Cecil;
using NSubstitute;
using Xunit;

namespace DotNet.Ildasm.Tests.Infrastructure
{
    public class EventDefinitionExtensionsShould
    {
        private readonly OutputWriterDouble _outputWriter;
        private readonly AssemblyDefinition _assemblyDefinition;
        private readonly IOutputWriter _outputWriterMock;
        private static readonly string[] WCA = [
#if NETFRAMEWORK
                    ".custom instance void class dotnet_ildasm.Sample.Classes.SomeAttribute::.ctor() = ( 01 00 00 00 ) // ....",
#else
                    ".custom instance void class dotnet_ildasm.Sample.Classes.SomeAttribute::.ctor() = ( 01 00 00 00 ) // ....",
#endif
                ];
        private static readonly string[] WEM = [
#if NETFRAMEWORK
                    ".addon instance default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::add_SomeEventWithAttribute (class [mscorlib]System.EventHandler`1<System.Object> 'value')",
                    ".removeon instance default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::remove_SomeEventWithAttribute (class [mscorlib]System.EventHandler`1<System.Object> 'value')"
#else
                    ".addon instance default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::add_SomeEventWithAttribute (class [netstandard]System.EventHandler`1<System.Object> 'value')",
                    ".removeon instance default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::remove_SomeEventWithAttribute (class [netstandard]System.EventHandler`1<System.Object> 'value')"
#endif
                ];
        private static readonly string[] WESM = [
#if NETFRAMEWORK
                    ".addon default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::add_SomeStaticEventWithAttribute (class [mscorlib]System.EventHandler`1<System.String> 'value')",
                    ".removeon default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::remove_SomeStaticEventWithAttribute (class [mscorlib]System.EventHandler`1<System.String> 'value')"
#else
                    ".addon default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::add_SomeStaticEventWithAttribute (class [netstandard]System.EventHandler`1<System.String> 'value')",
                    ".removeon default void dotnet_ildasm.Sample.Classes.SomeClassWithAttribute::remove_SomeStaticEventWithAttribute (class [netstandard]System.EventHandler`1<System.String> 'value')"
#endif
                ];

        public EventDefinitionExtensionsShould()
        {
            _outputWriter = new OutputWriterDouble();
            _assemblyDefinition = DataHelper.SampleAssembly.Value;
            _outputWriterMock = Substitute.For<IOutputWriter>();
        }

        [Theory]
        [InlineData("SomeClassWithAttribute", "SomeEventWithAttribute", ".event class [netstandard]System.EventHandler`1<System.Object> SomeEventWithAttribute")]
        public void Write_Event_Signature(string className, string eventName, string expectedIL)
        {
            var type = _assemblyDefinition.MainModule.Types.FirstOrDefault(x => x.Name == className);
            var eventDefinition = type.Events.FirstOrDefault(x => x.Name == eventName);

            eventDefinition.WriteILSignature(_outputWriter);

            Assert.Equal(expectedIL, _outputWriter.ToString());
        }

        [Fact]
        public void Write_Custom_Attributes()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "SomeClassWithAttribute");
            var eventDefinition = type.Events.First(static x => x.Name == "SomeEventWithAttribute");

            eventDefinition.WriteILBody(_outputWriterMock);
            _outputWriterMock.Received(1).WriteLine(Arg.Is<string>(
                static x => WCA.Contains(x)
            ));
        }

        [Fact]
        public void Write_Event_Methods()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "SomeClassWithAttribute");
            var eventDefinition = type.Events.First(static x => x.Name == "SomeEventWithAttribute");

            eventDefinition.WriteILBody(_outputWriterMock);
            _outputWriterMock.Received(2).WriteLine(Arg.Is<string>(
                static x => WEM.Contains(x)
            ));
        }

        [Fact]
        public void Write_Event_Static_Methods()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "SomeClassWithAttribute");
            var eventDefinition = type.Events.First(static x => x.Name == "SomeStaticEventWithAttribute");

            eventDefinition.WriteILBody(_outputWriterMock);
            _outputWriterMock.Received(2).WriteLine(Arg.Is<string>(
                static x => WESM.Contains(x)
            ));
        }
    }
}

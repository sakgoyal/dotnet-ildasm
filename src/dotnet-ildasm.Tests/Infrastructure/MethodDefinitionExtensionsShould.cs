using System.Linq;
using DotNet.Ildasm.Tests.Internal;
using Mono.Cecil;
using NSubstitute;
using Xunit;

namespace DotNet.Ildasm.Tests.Infrastructure
{
    public class MethodDefinitionExtensionsShould
    {
        private readonly OutputWriterDouble _outputWriter;
        private readonly AssemblyDefinition _assemblyDefinition;
        private readonly IOutputWriter _outputWriterMock;
        private static readonly string[] SRVA = [
                    ".custom instance void class dotnet_ildasm.Sample.Classes.SomeAttribute::.ctor() = ( 01 00 00 00 ) // ....",
                    ".custom instance void class dotnet_ildasm.Sample.Classes.AnotherAttribute::.ctor() = ( 01 00 00 00 ) // ...."
                ];
        private static readonly string[] SPWA = [
                    ".param [1]",
                    ".custom instance void class dotnet_ildasm.Sample.Classes.SomeAttribute::.ctor() = ( 01 00 00 00 ) // ....",
                    ".param [2]",
                    ".custom instance void class dotnet_ildasm.Sample.Classes.AnotherAttribute::.ctor() = ( 01 00 00 00 ) // ...."
                ];
        private static readonly string[] BATSPK = [
#if NETFRAMEWORK
                    ".custom instance void class [System.Runtime]System.ParamArrayAttribute::.ctor() = ( 01 00 00 00 ) // ....",
#else
                    ".custom instance void class [netstandard]System.ParamArrayAttribute::.ctor() = ( 01 00 00 00 ) // ...."
#endif
                ];
        private static readonly string[] WCA = [
#if NETFRAMEWORK
                    ".custom instance void class [mscorlib]System.ObsoleteAttribute::.ctor(string) = ( 01 00 21 54 68 69 73 20 6D 65 74 68 6F 64 20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64 2E 2E 2E 00 00 00 ) // ...This.method.should.not.be.used......",
#else
                    ".custom instance void class [netstandard]System.ObsoleteAttribute::.ctor(string) = ( 01 00 21 54 68 69 73 20 6D 65 74 68 6F 64 20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64 2E 2E 2E 00 00 00 ) // ...This.method.should.not.be.used......"
#endif
                ];

        public MethodDefinitionExtensionsShould()
        {
            _outputWriter = new OutputWriterDouble();
            _assemblyDefinition = DataHelper.SampleAssembly.Value;
            _outputWriterMock = Substitute.For<IOutputWriter>();
        }

        [Theory]
        [InlineData("PublicClass", "PublicVoidMethod", ".method public hidebysig instance default void PublicVoidMethod() cil managed")]
        [InlineData("PublicClass", "PublicVoidMethodSingleParameter", ".method public hidebysig instance default void PublicVoidMethodSingleParameter(string parameter1) cil managed")]
        [InlineData("PublicClass", "PublicVoidMethodTwoParameters", ".method public hidebysig instance default void PublicVoidMethodTwoParameters(string parameter1, int32 parameter2) cil managed")]
        [InlineData("PublicClass", "PublicVoidMethodParams", ".method public hidebysig instance default void PublicVoidMethodParams(string[] parameters) cil managed")]
        [InlineData("PublicClass", "set_Property1", ".method public hidebysig specialname instance default void set_Property1(string 'value') cil managed")]
        [InlineData("PublicAbstractClass", "PublicAbstractMethod", ".method public hidebysig newslot abstract virtual instance default void PublicAbstractMethod() cil managed")]
        [InlineData("PublicAbstractClass", "PublicImplementedMethod", ".method public hidebysig instance default void PublicImplementedMethod() cil managed")]
        [InlineData("DerivedPublicClass", "PublicAbstractMethod", ".method public hidebysig virtual instance default void PublicAbstractMethod() cil managed")]
        [InlineData("DerivedPublicClass", "PublicAbstractSealedMethod", ".method public hidebysig virtual final instance default void PublicAbstractSealedMethod() cil managed")]
        [InlineData("DerivedPublicClass", "PublicImplementedMethod", ".method public hidebysig instance default void PublicImplementedMethod() cil managed")]
        [InlineData("StaticClass", "Method3", ".method public hidebysig static default native int Method3() cil managed")]
        public void Write_Method_Signature(string className, string methodName, string expectedIL)
        {
            var type = _assemblyDefinition.MainModule.Types.FirstOrDefault(x => x.Name == className);
            var method = type.Methods.FirstOrDefault(x => x.Name == methodName);

            method.WriteILSignature(_outputWriter);

            Assert.Equal(expectedIL, _outputWriter.ToString());
        }

        [Theory]
        [InlineData("SomeClassWithAttribute", "SomeDelegateWithAttribute", ".ctor", ".method public hidebysig specialname rtspecialname instance default void .ctor([netstandard]System.Object 'object', native int 'method') runtime managed")]
        public void Write_Method_Signature2(string className, string nestedClassName, string methodName, string expectedIL)
        {
            var type = _assemblyDefinition.MainModule.Types.FirstOrDefault(x => x.Name == className);
            var nestedType = type.NestedTypes.FirstOrDefault(x => x.Name == nestedClassName);
            var method = nestedType.Methods.FirstOrDefault(x => x.Name == methodName);

            method.WriteILSignature(_outputWriter);

            Assert.Equal(expectedIL, _outputWriter.ToString());
        }

        [Fact]
        public void Support_Return_Value_Attributes()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "SomeClassWithAttribute");
            var methodDefinition = type.Methods.First(static x => x.Name == "SomeMethodWithAttributeOnReturnValue");

            methodDefinition.WriteILBody(_outputWriterMock);

            _outputWriterMock.Received(1).WriteLine(".param [0]");
            _outputWriterMock.Received(2).WriteLine(Arg.Is<string>(
                static x => SRVA.Contains(x)
            ));
        }

        [Fact]
        public void Support_Parameters_With_Attributes()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "SomeClassWithAttribute");
            var methodDefinition = type.Methods.First(static x => x.Name == "SomeMethodWithAttributeOnParameter");

            methodDefinition.WriteILBody(_outputWriterMock);

            _outputWriterMock.Received(4).WriteLine(Arg.Is<string>(
                static x => SPWA.Contains(x)
            ));
        }

        [Fact]
        public void Be_Able_To_Support_Params_Keyword()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "PublicClass");
            var methodDefinition = type.Methods.First(static x => x.Name == "PublicVoidMethodParams");

            methodDefinition.WriteILBody(_outputWriterMock);

            _outputWriterMock.Received(1).WriteLine(".param [1]");
            _outputWriterMock.Received(1).WriteLine(Arg.Is<string>(
                static x => BATSPK.Contains(x)
            ));
        }

        [Fact]
        public void Write_Custom_Attributes()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "SomeClassWithAttribute");
            var methodDefinition = type.Methods.First(static x => x.Name == "SomeMethodWithAttribute");

            methodDefinition.WriteILBody(_outputWriterMock);
            _outputWriterMock.Received(1).WriteLine(Arg.Is<string>(
                static x => WCA.Contains(x)
            ));
        }

        [Fact]
        public void Be_Able_To_Initialise_Locals()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "PublicClass");
            var methodDefinition = type.Methods.First(static x => x.Name == "UsingIF");

            methodDefinition.WriteILBody(_outputWriterMock);

            _outputWriterMock.Received().WriteLine(Arg.Do(static (string IL_Code) =>
            {
                // For some reason, depending on platform/compilation the same code may generate two different ILs
                // potentially due to compiler optmisations.
                Assert.True(IL_Code == ".locals init(int32 V_0, boolean V_1)" ||
                            IL_Code == ".locals init(int32 V_0)");
            }));
        }

        [Fact]
        public void Be_Able_To_Support_Try_Catch()
        {
            var type = DataHelper.SampleAssembly.Value.Modules.First().Types.First(static x => x.Name == "PublicClass");
            var methodDefinition = type.Methods.First(static x => x.Name == "UsingTryCatch");

            methodDefinition.WriteILBody(_outputWriterMock);

            _outputWriterMock.Received().WriteLine(Arg.Do(static (string IL_Code) =>
            {
                // For some reason, depending on platform/compilation the same code may generate two different ILs
                // potentially due to compiler optmisations.
                Assert.True(IL_Code == ".locals init(int32 V_0, boolean V_1)" ||
                            IL_Code == ".locals init(int32 V_0)");
            }));
        }
    }
}

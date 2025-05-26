using NSubstitute;
using Xunit;

namespace DotNet.Ildasm.Tests
{
    public class DisassemblerFactoryShould
    {
        private readonly IAssemblyDefinitionResolver _assemblyDefinitionResolver;
        private readonly IAssemblyDecompiler _assemblyDecompiler;
        private readonly DisassemblerFactory _disassemblerFactory;

        public DisassemblerFactoryShould()
        {
            _assemblyDefinitionResolver = Substitute.For<IAssemblyDefinitionResolver>();
            _assemblyDecompiler = Substitute.For<IAssemblyDecompiler>();
            _disassemblerFactory = new DisassemblerFactory(_assemblyDefinitionResolver, _assemblyDecompiler);
        }

        [Fact]
        public void Create_ConsoleOutputDisassembler_When_Output_Has_Not_Been_Set()
        {
            var options = new CommandArgument
            {
                Assembly = "assembly.dll"
            };

            var disassembler = _disassemblerFactory.Create(options);

            Assert.IsType<ConsoleOutputDisassembler>(disassembler);
        }

        [Fact]
        public void Create_FileOutputDisassembler_When_Output_Has_Been_Set()
        {
            var options = new CommandArgument
            {
                Assembly = "assembly.dll",
                OutputFile = "output.il",
                ForceOverwrite = true
            };

            var disassembler = _disassemblerFactory.Create(options);

            Assert.IsType<FileOutputDisassembler>(disassembler);
        }
    }
}

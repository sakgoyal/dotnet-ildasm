using DotNet.Ildasm.Configuration;

namespace DotNet.Ildasm
{
    public sealed class ConsoleOutputDisassembler(IAssemblyDecompiler assemblyDataProcessor, IAssemblyDefinitionResolver assemblyResolver) : Disassembler(assemblyDataProcessor, assemblyResolver)
    {
        public override ExecutionResult Execute(CommandArgument argument, ItemFilter itemFilter)
        {
            var result = base.Execute(argument, itemFilter);
            return result;
        }
    }
}

using DotNet.Ildasm.Configuration;

namespace DotNet.Ildasm
{
    public sealed class FileOutputDisassembler(IAssemblyDecompiler assemblyDataProcessor, IAssemblyDefinitionResolver assemblyResolver) : Disassembler(assemblyDataProcessor, assemblyResolver)
    {
        public override ExecutionResult Execute(CommandArgument argument, ItemFilter itemFilter)
        {
            var result = base.Execute(argument, itemFilter);
            if (result.Succeeded)
                return new ExecutionResult(true, $"Assembly IL exported to {argument.OutputFile}");

            return result;
        }
    }
}

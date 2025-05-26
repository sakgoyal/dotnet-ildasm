using System;
using DotNet.Ildasm.Configuration;
using DotNet.Ildasm.Infrastructure;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DotNet.Ildasm
{
    public class AssemblyDecompiler(string assemblyPath, IOutputWriter outputWriter) : IAssemblyDecompiler
    {
        private readonly IOutputWriter _outputWriter = outputWriter;
        private readonly ModuleDirectivesProcessor _moduleDirectivesProcessor = new(assemblyPath, outputWriter);

        public void WriteAssemblyExternalReferences(AssemblyDefinition assembly)
        {
            foreach (var reference in assembly.MainModule.AssemblyReferences)
            {
                reference.WriteIL(_outputWriter);
            }
        }

        public void WriteAssemblySection(AssemblyDefinition assembly)
        {
            assembly.WriteIL(_outputWriter);
        }

        public void WriteModuleSection(ModuleDefinition module)
        {
            _moduleDirectivesProcessor.WriteModuleDirective();
            _moduleDirectivesProcessor.WriteModuleVersionId(module.Mvid);
            _moduleDirectivesProcessor.WriteImageBaseDirective();
            _moduleDirectivesProcessor.WriteFileAlignmentDirective();
            _moduleDirectivesProcessor.WriteStackReserveDirective();
            _moduleDirectivesProcessor.WriteSubsystemDirective();
            _moduleDirectivesProcessor.WriteCornFlagsDirective();
        }

        public void WriteModuleTypes(Collection<TypeDefinition> types, ItemFilter itemFilter)
        {
            if (types?.Count > 0)
            {
                var typesProcessor = new TypesProcessor(_outputWriter, itemFilter);
                typesProcessor.Write(types);
            }
        }

        private void ReleaseUnmanagedResources()
        {
            _outputWriter?.Dispose();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _outputWriter?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AssemblyDecompiler()
        {
            Dispose(false);
        }
    }
}

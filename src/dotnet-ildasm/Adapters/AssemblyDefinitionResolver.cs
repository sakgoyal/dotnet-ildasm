using Mono.Cecil;

namespace DotNet.Ildasm
{
    public class AssemblyDefinitionResolver : IAssemblyDefinitionResolver
    {
        public AssemblyDefinition Resolve(string assemblyPath)
        {
            try
            {
                return AssemblyDefinition.ReadAssembly(assemblyPath);
            }
            catch
            {
                return null;
            }
        }
    }
}

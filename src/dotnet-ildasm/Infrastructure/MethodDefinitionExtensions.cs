using System.Linq;
using DotNet.Ildasm.Infrastructure;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DotNet.Ildasm
{
    public static class MethodDefinitionExtensions
    {
        public static void WriteILBody(this MethodDefinition method, IOutputWriter outputWriter)
        {
            outputWriter.WriteLine(string.Empty);
            outputWriter.WriteLine("{");

            method.WriteCustomAttributes(outputWriter);

            if (method.HasBody)
            {
                if (method.MethodReturnType.HasCustomAttributes)
                {
                    outputWriter.WriteLine(".param [0]");
                    foreach (var attr in method.MethodReturnType.CustomAttributes)
                        attr.WriteIL(outputWriter);
                }

                var @params = method.Parameters.Where(static x => x.HasCustomAttributes).ToArray();
                for (int i = 0; i < @params.Length; i++)
                {
                    outputWriter.WriteLine($".param [{i + 1}]");
                    @params[i].CustomAttributes.First().WriteIL(outputWriter);
                }

                outputWriter.WriteLine($"// Method begins at Relative Virtual Address (RVA) 0x{method.RVA:X}");

                if (method.DeclaringType.Module.EntryPoint == method)
                    outputWriter.WriteLine(".entrypoint");

                outputWriter.WriteLine($"// Code size {method.Body.CodeSize} (0x{method.Body.CodeSize:X})");
                outputWriter.WriteLine($".maxstack {method.Body.MaxStackSize}");

                WriteLocalVariablesIfNeeded(method, outputWriter);

                var ilProcessor = method.Body.GetILProcessor();
                foreach (var instruction in ilProcessor.Body.Instructions)
                {
                    WriteExceptionHandler(method, instruction, outputWriter);
                    instruction.WriteIL(outputWriter);
                }
            }

            outputWriter.WriteLine($"}} // End of method {method.FullName}");
        }

        private static void WriteCustomAttributes(this MethodDefinition methodDefinition, IOutputWriter outputWriter)
        {
            foreach (var customAttribute in methodDefinition.CustomAttributes)
            {
                customAttribute.WriteIL(outputWriter);
            }
        }

        private static void WriteExceptionHandler(MethodDefinition method, Instruction instruction, IOutputWriter outputWriter)
        {
            if (method.Body.HasExceptionHandlers)
            {
                foreach (var bodyExceptionHandler in method.Body.ExceptionHandlers)
                {
                    //TODO: Support other handler types. #28
                    if (bodyExceptionHandler.HandlerType != ExceptionHandlerType.Catch)
                        return;

                    if (instruction.Offset == bodyExceptionHandler.TryStart.Offset ||
                        instruction.Offset == bodyExceptionHandler.HandlerStart.Offset)
                    {
                        if (instruction.Offset == bodyExceptionHandler.TryStart.Offset)
                            outputWriter.WriteLine(".try");

                        if (instruction.Offset == bodyExceptionHandler.HandlerStart.Offset)
                        {
                            outputWriter.WriteLine("}");
                            outputWriter.WriteLine($"catch {bodyExceptionHandler.CatchType.ToIL()}");
                        }

                        outputWriter.WriteLine("{");
                    }

                    if (instruction.Offset == bodyExceptionHandler.HandlerEnd.Offset)
                    {
                        outputWriter.WriteLine("}");
                    }
                }
            }
        }

        private static void WriteLocalVariablesIfNeeded(MethodDefinition method, IOutputWriter outputWriter)
        {
            if (method.Body.InitLocals)
            {
                string variables = string.Empty;
                int i = 0;

                foreach (var variable in method.Body.Variables)
                {
                    if (i > 0)
                        variables += ", ";

                    if (variable.VariableType.MetadataType == MetadataType.Class || variable.VariableType.IsGenericInstance)
                        variables += "class ";

                    variables += $"{variable.VariableType.ToIL()} V_{i++}";
                }

                outputWriter.WriteLine($".locals init({variables})");
            }
        }

        public static void WriteILSignature(this MethodDefinition method, IOutputWriter writer)
        {
            writer.Write(".method");

            if (method.IsPublic)
                writer.Write(" public");
            else if (method.IsPrivate)
                writer.Write(" private");

            if (method.IsHideBySig)
                writer.Write(" hidebysig");

            if (method.IsNewSlot)
                writer.Write(" newslot");

            if (method.IsAbstract)
                writer.Write(" abstract");

            if (method.IsVirtual)
                writer.Write(" virtual");

            if (method.IsSpecialName)
                writer.Write(" specialname");

            if (method.IsRuntimeSpecialName)
                writer.Write(" rtspecialname");

            if (method.IsFinal)
                writer.Write(" final");

            if (!method.IsStatic)
                writer.Write(" instance");
            else
                writer.Write(" static");

            if (!method.HasBody || method.IsDefinition)
                writer.Write(" default");

            writer.Write($" {method.ReturnType.ToNativeTypeIL()}");
            writer.Write($" {method.Name}");

            WriteMethodSignatureParameters(method, writer);

            if (method.IsRuntime)
                writer.Write(" runtime managed");
            else if (method.IsManaged)
                writer.Write(" cil managed");
        }

        private static void WriteMethodSignatureParameters(MethodDefinition method, IOutputWriter writer)
        {
            writer.Write("(");

            if (method.HasParameters)
            {
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");

                    var parameterDefinition = method.Parameters[i];
                    if (parameterDefinition.ParameterType.IsGenericInstance ||
                        parameterDefinition.ParameterType.MetadataType == MetadataType.Class)
                        writer.Write("class ");

                    writer.Write($"{parameterDefinition.ParameterType.ToNativeTypeIL()} ");

                    if (parameterDefinition.Name == "value" || parameterDefinition.Name == "object" || parameterDefinition.Name == "method")
                        writer.Write($"'{parameterDefinition.Name}'");
                    else
                        writer.Write(parameterDefinition.Name);
                }
            }

            writer.Write(")");
        }
    }
}

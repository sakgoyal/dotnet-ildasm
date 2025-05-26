using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.Ildasm.Configuration;
using DotNet.Ildasm.Infrastructure;
using Mono.Cecil;

namespace DotNet.Ildasm
{
    internal sealed class TypesProcessor(IOutputWriter outputWriter, ItemFilter itemFilter)
    {
        private readonly IOutputWriter _outputWriter = outputWriter;
        private readonly ItemFilter _itemFilter = itemFilter;

        public void Write(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                if (string.Equals(type.Name, "<Module>", StringComparison.Ordinal))
                    continue;

                if (!IsFilterSet() ||
                    DoesTypeMatchFilter(type) ||
                    DoesTypeContainMethodMatchingFilter(type))
                    HandleType(type);
            }
        }

        private bool DoesTypeMatchFilter(TypeDefinition type)
        {
            return string.Equals(type.Name, _itemFilter.Class, StringComparison.Ordinal);
        }

        private bool DoesTypeContainMethodMatchingFilter(TypeDefinition type)
        {
            return !string.IsNullOrEmpty(_itemFilter.Method) && type.Methods.Any(x => string.Equals(x.Name, _itemFilter.Method, StringComparison.Ordinal));
        }

        private bool IsFilterSet()
        {
            return _itemFilter.HasFilter;
        }

        private void HandleType(TypeDefinition type)
        {
            type.WriteILSignature(_outputWriter);
            _outputWriter.WriteLine("{");

            WriteCustomAttributes(type);
            WriteFields(type);
            WriteMethods(type);
            WriteProperties(type);
            WriteEvents(type);

            foreach (var nestedType in type.NestedTypes)
                HandleType(nestedType);

            _outputWriter.WriteLine($"}} // End of class {type.FullName}");
        }

        private void WriteCustomAttributes(TypeDefinition type)
        {
            if (type.HasCustomAttributes)
            {
                foreach (var customAttribute in type.CustomAttributes)
                    customAttribute.WriteIL(_outputWriter);
            }
        }

        private void WriteMethods(TypeDefinition type)
        {
            foreach (var method in type.Methods)
            {
                if (string.IsNullOrEmpty(_itemFilter.Method) ||
                    string.Equals(method.Name, _itemFilter.Method, StringComparison.Ordinal))
                    HandleMethod(method);
            }
        }

        private void WriteProperties(TypeDefinition type)
        {
            foreach (var property in type.Properties)
            {
                if (string.IsNullOrEmpty(_itemFilter.Method) ||
                    string.Equals(property.Name, _itemFilter.Method, StringComparison.Ordinal))
                    HandleProperty(property);
            }
        }

        private void WriteEvents(TypeDefinition type)
        {
            foreach (var @event in type.Events)
            {
                if (string.IsNullOrEmpty(_itemFilter.Method) ||
                    string.Equals(@event.Name, _itemFilter.Method, StringComparison.Ordinal))
                    HandleEvent(@event);
            }
        }

        private void WriteFields(TypeDefinition type)
        {
            if (type.HasFields)
            {
                foreach (var field in type.Fields)
                    field.WriteIL(_outputWriter);
            }
        }

        private void HandleMethod(MethodDefinition method)
        {
            method.WriteILSignature(_outputWriter);
            method.WriteILBody(_outputWriter);
        }

        private void HandleProperty(PropertyDefinition property)
        {
            property.WriteILSignature(_outputWriter);
            property.WriteILBody(_outputWriter);
        }

        private void HandleEvent(EventDefinition @event)
        {
            @event.WriteILSignature(_outputWriter);
            @event.WriteILBody(_outputWriter);
        }
    }
}

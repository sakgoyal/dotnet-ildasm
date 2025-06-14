﻿using System;
using Mono.Cecil;

namespace DotNet.Ildasm.Infrastructure
{
    public static class TypeReferenceExtensions
    {
        private static string ToPrefixedTypeName(TypeReference typeReference)
        {
            if (!typeReference.IsGenericInstance && string.Equals(typeReference.Scope.Name, typeReference.Module.Name,
                    StringComparison.OrdinalIgnoreCase))
                return $"{typeReference.FullName}";


            if (typeReference.MetadataType == MetadataType.ValueType)
                return $"valuetype [{typeReference.Scope.Name}]{typeReference.FullName}";

            return $"[{typeReference.Scope.Name}]{typeReference.FullName}";
        }

        public static string ToNativeTypeIL(this TypeReference typeReference)
        {
            if (typeReference.MetadataType == MetadataType.IntPtr)
                return "native int";

            return typeReference.ToIL();
        }

        public static string ToIL(this TypeReference typeReference)
        {
            if (typeReference.MetadataType == MetadataType.Boolean)
                return "bool";

            if (typeReference.IsGenericInstance ||
                typeReference.MetadataType == MetadataType.Class ||
                typeReference.MetadataType == MetadataType.Object ||
                typeReference.MetadataType == MetadataType.ValueType ||
                typeReference.MetadataType == MetadataType.IntPtr)
            {
                return ToPrefixedTypeName(typeReference);
            }

            if (typeReference.MetadataType != MetadataType.Array)
                return typeReference.MetadataType.ToString().ToLowerInvariant();

            if (typeReference.MetadataType == MetadataType.Array)
                return $"{typeReference.GetElementType().ToIL()}[]";

            throw new NotSupportedException();
        }
    }
}

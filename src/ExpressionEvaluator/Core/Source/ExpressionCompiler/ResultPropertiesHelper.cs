// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Forked by sharpdbg: extracted from DkmUtilities.cs (which is otherwise excluded from this fork because it
// references Microsoft.VisualStudio.Debugger.Engine classes). Only the ResultProperties computation is needed
// by the C# ExpressionEvaluator, and it only uses the Dkm enum types provided by DkmContractShim.cs.

using Microsoft.CodeAnalysis.Symbols;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Evaluation.ClrCompilation;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.ExpressionEvaluator
{
    internal static class ResultPropertiesHelper
    {
        internal static ResultProperties GetResultProperties<TSymbol>(this TSymbol? symbol, DkmClrCompilationResultFlags flags, bool isConstant)
            where TSymbol : class, ISymbolInternal
        {
            var category = (symbol != null) ? GetResultCategory(symbol.Kind)
                : DkmEvaluationResultCategory.Data;

            var accessType = (symbol != null) ? GetResultAccessType(symbol.DeclaredAccessibility)
                : DkmEvaluationResultAccessType.None;

            var storageType = (symbol != null) && symbol.IsStatic
                ? DkmEvaluationResultStorageType.Static
                : DkmEvaluationResultStorageType.None;

            var modifierFlags = DkmEvaluationResultTypeModifierFlags.None;
            if (isConstant)
            {
                modifierFlags = DkmEvaluationResultTypeModifierFlags.Constant;
            }
            else if (symbol is null)
            {
                // No change.
            }
            else if (symbol.IsVirtual || symbol.IsAbstract || symbol.IsOverride)
            {
                modifierFlags = DkmEvaluationResultTypeModifierFlags.Virtual;
            }
            else if (symbol.Kind == SymbolKind.Field && ((IFieldSymbolInternal)symbol).IsVolatile)
            {
                modifierFlags = DkmEvaluationResultTypeModifierFlags.Volatile;
            }

            // CONSIDER: for completeness, we could check for [MethodImpl(MethodImplOptions.Synchronized)]
            // and set DkmEvaluationResultTypeModifierFlags.Synchronized, but it doesn't seem to have any
            // impact on the UI.  It is exposed through the DTE, but cscompee didn't set the flag either.

            return new ResultProperties(flags, category, accessType, storageType, modifierFlags);
        }

        private static DkmEvaluationResultCategory GetResultCategory(SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Method:
                    return DkmEvaluationResultCategory.Method;
                case SymbolKind.Property:
                    return DkmEvaluationResultCategory.Property;
                default:
                    return DkmEvaluationResultCategory.Data;
            }
        }

        private static DkmEvaluationResultAccessType GetResultAccessType(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Public:
                    return DkmEvaluationResultAccessType.Public;
                case Accessibility.Protected:
                    return DkmEvaluationResultAccessType.Protected;
                case Accessibility.Private:
                    return DkmEvaluationResultAccessType.Private;
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal: // Dev12 treats this as "internal"
                case Accessibility.ProtectedAndInternal: // Dev12 treats this as "internal"
                    return DkmEvaluationResultAccessType.Internal;
                case Accessibility.NotApplicable:
                    return DkmEvaluationResultAccessType.None;
                default:
                    throw ExceptionUtilities.UnexpectedValue(accessibility);
            }
        }
    }
}

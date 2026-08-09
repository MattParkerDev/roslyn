// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Forked by sharpdbg: minimal stand-ins for the Microsoft.VisualStudio.Debugger.Engine (Dkm) contract enum types
// that the ExpressionEvaluator source references. Only the enum types actually used by the EE source are defined
// so the assembly has no dependency on the Microsoft.VisualStudio.Debugger.Engine assembly. Values match the
// published Dkm contract so the EE behaves identically.

using System;

namespace Microsoft.VisualStudio.Debugger.Clr
{
    /// <summary>Alias kinds for pseudo-variables such as $exception.</summary>
    public enum DkmClrAliasKind
    {
        Exception = 0,
        StowedException = 1,
        ReturnValue = 2,
        Variable = 3,
        ObjectId = 4,
    }
}

namespace Microsoft.VisualStudio.Debugger.Evaluation
{
    [Flags]
    public enum DkmEvaluationFlags
    {
        None = 0x0,
        TreatAsExpression = 0x1,
        TreatFunctionAsAddress = 0x2,
        NoSideEffects = 0x4,
        NoFuncEval = 0x8,
        DesignTime = 0x10,
        AllowImplicitVariables = 0x20,
        ForceEvaluationNow = 0x40,
        ShowValueRaw = 0x80,
        ForceRealFuncEval = 0x100,
        HideNonPublicMembers = 0x200,
        NoToString = 0x400,
        NoFormatting = 0x800,
        NoRawView = 0x1000,
        NoQuotes = 0x2000,
        DynamicView = 0x4000,
        ResultsOnly = 0x8000,
        NoExpansion = 0x10000,
        EnableExtendedSideEffects = 0x20000,
        FilterToFavorites = 0x40000,
        UseSimpleDisplayString = 0x80000,
        IncreaseMaxStringSize = 0x100000,
        CompactName = 0x200000,
    }

    public enum DkmEvaluationResultCategory
    {
        Other = 0,
        Data = 1,
        Method = 2,
        Event = 3,
        Property = 4,
        Class = 5,
        Interface = 6,
        BaseClass = 7,
        InnerClass = 8,
        MostDerivedClass = 9,
    }

    public enum DkmEvaluationResultAccessType
    {
        None = 0,
        Public = 1,
        Private = 2,
        Protected = 3,
        Final = 4,
        Internal = 5,
    }

    public enum DkmEvaluationResultStorageType
    {
        None = 0,
        Global = 1,
        Static = 2,
        Register = 3,
    }

    [Flags]
    public enum DkmEvaluationResultTypeModifierFlags
    {
        None = 0,
        Virtual = 1,
        Constant = 2,
        Synchronized = 4,
        Volatile = 8,
    }
}

namespace Microsoft.VisualStudio.Debugger.Evaluation.ClrCompilation
{
    [Flags]
    public enum DkmClrCompilationResultFlags
    {
        None = 0,
        PotentialSideEffect = 1,
        ReadOnlyResult = 2,
        BoolResult = 4,
    }
}

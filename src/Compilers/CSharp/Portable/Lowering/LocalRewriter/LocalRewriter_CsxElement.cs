// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed partial class LocalRewriter
    {
        // -----------------------------------------------------------------------
        // CSX (JSX-like component syntax) lowering — classic runtime
        //
        // <Button Color="red">
        //     <Icon Name="star" />
        //     Some text
        // </Button>
        //
        // Lowers to:
        //
        // H.CreateElement(
        //     Button.Render,                           // Func<ButtonProps, H.CSX.Element>
        //     new ButtonProps(Color: "red"),           // TProps
        //     [H.CreateElement(Icon.Render, new IconProps(Name: "star")),
        //      H.CSX.CreateTextNode("Some text")])    // params CSX.Element[] (collection expr)
        //
        // The BoundCsxElement already contains:
        //   FactoryMethod     — the unbound generic CreateElement method symbol
        //   ComponentMethod   — the concrete component method (e.g. Button.Render)
        //   ComponentArgument — BoundTypeExpression for the component type (unused here)
        //   PropsArgument     — BoundObjectCreationExpression for the props record
        //   Children          — already-bound child expressions
        // -----------------------------------------------------------------------

        public override BoundNode VisitCsxElement(BoundCsxElement node)
        {
            // ---- 1. Resolve the concrete component method ----
            var componentMethod = node.ComponentMethod;

            // ---- 2. Determine props type (first parameter of the component method) ----
            var propsType = componentMethod.Parameters.Length > 0
                ? componentMethod.Parameters[0].Type
                : null;

            // ---- 3. Construct CreateElement<TProps> ----
            // The factory method may be generic (CreateElement<TProps>). Construct it.
            var factoryMethod = node.FactoryMethod;
            if (factoryMethod.IsGenericMethod && propsType is not null)
            {
                factoryMethod = factoryMethod.Construct(ImmutableArray.Create(propsType));
            }

            // ---- 4. Build the component delegate arg ----
            // The first parameter of CreateElement<TProps> is Func<TProps, Element>.
            // We build: new Func<TProps, Element>(Button.Render)
            //
            // For a static method delegate, the canonical bound representation uses a
            // BoundTypeExpression (the containing type) as the argument, and sets methodOpt
            // to the resolved method. This is what the binder produces for `new Func<T,R>(Cls.M)`
            // and avoids triggering the ExtensionMethodReferenceRewriter assertion that fires
            // when argument is a BoundMethodGroup for a static method.
            BoundExpression componentArg;
            if (factoryMethod.Parameters.Length > 0)
            {
                var delegateType = factoryMethod.Parameters[0].Type;
                var containingType = componentMethod.ContainingType;
                var typeExpr = new BoundTypeExpression(
                    syntax: node.Syntax,
                    aliasOpt: null,
                    type: containingType)
                { WasCompilerGenerated = true };
                componentArg = new BoundDelegateCreationExpression(
                    syntax: node.Syntax,
                    argument: typeExpr,
                    methodOpt: componentMethod,
                    isExtensionMethod: false,
                    wasTargetTyped: false,
                    type: delegateType)
                { WasCompilerGenerated = true };
            }
            else
            {
                // Shouldn't normally happen; fall back to the bound expression.
                componentArg = VisitExpression(node.ComponentArgument);
            }

            // ---- 5. Lower the props argument ----
            BoundExpression? propsArg = node.PropsArgument is not null
                ? VisitExpression(node.PropsArgument)
                : null;

            // ---- 6. Lower children and wrap in a collection expression ----
            // Use _factory.ArrayOrEmpty so:
            //   • 0 children → Array.Empty<Element>()   (no allocation)
            //   • N children → new Element[] { c0, c1, … }
            // At the CLR level Element[] and Element?[] are the same type; nullability
            // is a compile-time annotation only.
            var loweredChildren = VisitList(node.Children);
            var elementType = node.Type; // CSX.Element
            var childrenArg = _factory.ArrayOrEmpty(elementType, loweredChildren);

            // ---- 7. Assemble args matching CreateElement(component, props, children[]) ----
            ImmutableArray<BoundExpression> args;
            if (propsArg is not null)
            {
                args = ImmutableArray.Create(componentArg, propsArg, childrenArg);
            }
            else
            {
                // No props (zero-parameter component); omit props arg.
                args = ImmutableArray.Create(componentArg, childrenArg);
            }

            return _factory.StaticCall(factoryMethod, args);
        }
    }
}

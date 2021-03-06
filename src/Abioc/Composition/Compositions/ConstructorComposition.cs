﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Generation;

    /// <summary>
    /// A composition to produce code to create a class via a constructor.
    /// </summary>
    internal class ConstructorComposition : CompositionBase
    {
        private readonly List<IParameterExpression> _parameterExpressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorComposition"/> class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> created by the constructor.</param>
        /// <param name="parameters">The <see cref="Parameters"/> of the constructor.</param>
        /// <param name="isDefault">
        /// A value indicating whether this is the default composition, and therefore can be superseded by
        /// another composition.
        /// </param>
        public ConstructorComposition(
            Type type,
            IReadOnlyList<ParameterInfo> parameters,
            bool isDefault = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            Type = type;
            Parameters = parameters;
            IsDefault = isDefault;

            _parameterExpressions = new List<IParameterExpression>(parameters.Count);
        }

        /// <summary>
        /// Gets the type created by the constructor.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Gets the parameters of the constructor.
        /// </summary>
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <summary>
        /// Gets a value indicating whether this is the default composition, and therefore can be superseded by
        /// another composition.
        /// </summary>
        public bool IsDefault { get; }

        /// <inheritdoc/>
        public override string GetInstanceExpression(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Get the expressions for the all the constructor parameters.
            IEnumerable<IParameterExpression> expressions = GetParameterExpressions(context);
            IEnumerable<string> parameterExpressions =
                from e in expressions
                let ctx = context.Customize(recipientType: Type, serviceType: e.Type)
                select e.GetInstanceExpression(ctx);

            // Join the parameters expressions.
            string parameters =
                string.Join(
                    "," + Environment.NewLine + "    ",
                    parameterExpressions.Select(p => CodeGen.Indent(p)));

            // Create the new Expression.
            string expression = string.IsNullOrEmpty(parameters)
                ? $"new {Type.ToCompileName()}()"
                : $"new {Type.ToCompileName()}({Environment.NewLine}    {parameters})";
            return expression;
        }

        /// <inheritdoc/>
        public override string GetComposeMethodName(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = "Create" + Type.ToCompileMethodName(context.UsingSimpleNames);
            return methodName;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetMethods(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string parameter = RequiresConstructionContext(context)
                ? $"{Environment.NewLine}    {context.ConstructionContext} context"
                : string.Empty;

            string methodName = GetComposeMethodName(context);
            string signature = $"private {Type.ToCompileName()} {methodName}({parameter})";

            string instanceExpression = GetInstanceExpression(context);
            instanceExpression = CodeGen.Indent(instanceExpression);

            string method =
                string.Format("{0}{2}{{{2}    return {1};{2}}}", signature, instanceExpression, Environment.NewLine);
            return new[] { method };
        }

        /// <inheritdoc/>
        public override bool RequiresConstructionContext(IGenerationContext context)
        {
            return GetParameterExpressions(context).Any(c => c.RequiresConstructionContext(context));
        }

        private IEnumerable<IParameterExpression> GetParameterExpressions(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (_parameterExpressions.Count == Parameters.Count)
                return _parameterExpressions;

            foreach (ParameterInfo parameter in Parameters)
            {
                if (context.Compositions.TryGetValue(parameter.ParameterType, out IComposition composition))
                {
                    IParameterExpression expression = new SimpleParameterExpression(composition);
                    _parameterExpressions.Add(expression);
                    continue;
                }

                TypeInfo parameterTypeInfo = parameter.ParameterType.GetTypeInfo();
                if (parameterTypeInfo.IsGenericType)
                {
                    Type genericTypeDefinition = parameterTypeInfo.GetGenericTypeDefinition();
                    if (typeof(IEnumerable<>) == genericTypeDefinition)
                    {
                        Type enumerableType = parameterTypeInfo.GenericTypeArguments.Single();
                        IParameterExpression expression =
                            new EnumerableParameterExpression(enumerableType, context.HasConstructionContext);
                        _parameterExpressions.Add(expression);
                        continue;
                    }
                }

                string message =
                    $"Failed to get the compositions for the parameter '{parameter}' to the constructor of " +
                    $"'{Type}'. Is there a missing registration mapping?";
                throw new CompositionException(message);
            }

            return _parameterExpressions;
        }
    }
}

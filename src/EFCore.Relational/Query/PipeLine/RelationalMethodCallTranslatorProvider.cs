// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalMethodCallTranslatorProvider : IMethodCallTranslatorProvider
    {
        private readonly List<IMethodCallTranslator> _methodCallTranslators = new List<IMethodCallTranslator>();

        public RelationalMethodCallTranslatorProvider(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _methodCallTranslators.AddRange(
                new[] {
                    new EqualsTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor)
                });
        }

        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            return _methodCallTranslators.Select(t => t.Translate(methodCallExpression)).FirstOrDefault(t => t != null);
        }

        protected virtual void AddTranslators(IEnumerable<IMethodCallTranslator> translators)
            => _methodCallTranslators.InsertRange(0, translators);
    }
}

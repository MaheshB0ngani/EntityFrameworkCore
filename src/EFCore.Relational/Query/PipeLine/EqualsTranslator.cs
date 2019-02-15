// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public EqualsTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            Expression left = null;
            Expression right = null;
            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Object != null)
            {
                left = methodCallExpression.Object;
                right = UnwrapObjectConvert(methodCallExpression.Arguments[0]);
            }
            else if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 2
                && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
            {
                left = UnwrapObjectConvert(methodCallExpression.Arguments[0]);
                right = UnwrapObjectConvert(methodCallExpression.Arguments[1]);
            }

            if (left != null && right != null)
            {
                if (left.Type.UnwrapNullableType() == right.Type.UnwrapNullableType())
                {
                    var typeMapping = ExpressionExtensions.InferTypeMapping(left, right);

                    if (typeMapping == null)
                    {
                        throw new InvalidOperationException("Equals should have at least one argument with TypeMapping.");
                    }

                    var leftSql = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(left, typeMapping, false);
                    var rightSql = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(right, typeMapping, false);

                    return new SqlBinaryExpression(ExpressionType.Equal, leftSql, rightSql,
                        typeof(bool), _typeMappingSource.FindMapping(typeof(bool)), true);
                }
                else
                {
                    return Expression.Constant(false);
                }
            }

            return null;
        }

        private static Expression UnwrapObjectConvert(Expression expression)
        {
            return expression is UnaryExpression unary
                && expression.Type == typeof(object)
                && expression.NodeType == ExpressionType.Convert
                ? unary.Operand
                : expression;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public interface ITypeMappingApplyingExpressionVisitor
    {
        SqlExpression ApplyTypeMapping(Expression expression, RelationalTypeMapping typeMapping, bool condition = false);
    }

    public class TypeMappingApplyingExpressionVisitor : ITypeMappingApplyingExpressionVisitor
    {
        private readonly RelationalTypeMapping _boolTypeMapping;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public TypeMappingApplyingExpressionVisitor(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        public virtual SqlExpression ApplyTypeMapping(
            Expression expression, RelationalTypeMapping typeMapping, bool condition = false)
        {
            if (expression is SqlExpression sqlExpression)
            {
                if (sqlExpression.IsCondition == condition)
                {
                    return sqlExpression;
                }

                return sqlExpression.InvertCondition();
            }

            if (expression.NodeType == ExpressionType.Extension)
            {
                return ApplyTypeMappingOnExtension(expression, typeMapping, condition);
            }

            switch (expression)
            {
                case BinaryExpression binaryExpression:
                    return ApplyTypeMappingOnBinary(binaryExpression, typeMapping, condition);

                case UnaryExpression unaryExpression:
                    return ApplyTypeMappingOnUnary(unaryExpression, typeMapping, condition);

                case ConstantExpression constantExpression:
                    return ApplyTypeMappingOnConstant(constantExpression, typeMapping, condition);

                case ParameterExpression parameterExpression:
                    return ApplyTypeMappingOnParameter(parameterExpression, typeMapping, condition);
            }

            return null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnBinary(
            BinaryExpression binaryExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            var left = binaryExpression.Left;
            var right = binaryExpression.Right;
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);

            if (inferredTypeMapping == null)
            {
                return null;
            }

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    {
                        if (binaryExpression.Type != typeof(bool))
                        {
                            throw new InvalidCastException("Comparison operation should be of type bool.");
                        }

                        var leftSql = ApplyTypeMapping(left, inferredTypeMapping, false);
                        var rightSql = ApplyTypeMapping(right, inferredTypeMapping, false);

                        return new SqlBinaryExpression(
                            binaryExpression.NodeType,
                            leftSql,
                            rightSql,
                            typeof(bool),
                            _boolTypeMapping,
                            true);
                    }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        var leftSql = ApplyTypeMapping(left, inferredTypeMapping, true);
                        var rightSql = ApplyTypeMapping(right, inferredTypeMapping, true);

                        return new SqlBinaryExpression(
                            binaryExpression.NodeType,
                            leftSql,
                            rightSql,
                            typeof(bool),
                            _boolTypeMapping,
                            true);
                    }
            }

            return null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnUnary(
            UnaryExpression unaryExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            if (unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Type == typeof(object)
                    && unaryExpression.Operand.Type != typeMapping.ClrType)
                {
                    var operand = ApplyTypeMapping(
                        unaryExpression.Operand,
                        _typeMappingSource.FindMapping(unaryExpression.Operand.Type),
                        false);

                    return new SqlUnaryExpression(
                        ExpressionType.Convert,
                        operand,
                        typeMapping.ClrType,
                        typeMapping,
                        false);
                }

                if (//unaryExpression.Type == typeof(object)
                    /*||*/ unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type)
                {
                    return ApplyTypeMapping(unaryExpression.Operand, typeMapping, condition);
                }
            }

            return null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnConstant(
            ConstantExpression constantExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            return typeMapping != null ? new SqlConstantExpression(constantExpression, typeMapping, condition) : null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnParameter(
            ParameterExpression parameterExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            return typeMapping != null ? new SqlParameterExpression(parameterExpression, typeMapping, condition) : null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnExtension(
            Expression expression, RelationalTypeMapping typeMapping, bool condition)
        {
            return null;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;
        private readonly TypeMappingInferringExpressionVisitor _typeInference;

        private SelectExpression _selectExpression;

        public RelationalSqlTranslatingExpressionVisitor(
            IRelationalTypeMappingSource typeMappingSource,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _typeInference = new TypeMappingInferringExpressionVisitor();
            _typeMappingSource = typeMappingSource;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public SqlExpression Translate(SelectExpression selectExpression, Expression expression, bool condition)
        {
            _selectExpression = selectExpression;

            var translation = Visit(expression);

            if (!(translation is SqlExpression sqlExpression))
            {
                sqlExpression = translation.ApplyDefaultTypeMapping(_typeMappingSource);
            }

            if (condition
                && !sqlExpression.IsCondition
                && sqlExpression.TypeMapping == _typeMappingSource.FindMapping(typeof(bool)))
            {
                sqlExpression = new SqlExpression(sqlExpression.Expression);
            }

            _selectExpression = null;

            return sqlExpression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);
            if (innerExpression is EntityShaperExpression entityShaper)
            {
                var entityType = entityShaper.EntityType;
                var property = entityType.FindProperty(memberExpression.Member.GetSimpleMemberName());

                return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
            }

            if (memberExpression.Member.Name == nameof(Nullable<int>.Value)
                && memberExpression.Member.DeclaringType.IsNullableType()
                && innerExpression is SqlExpression sqlExpression)
            {
                return sqlExpression.ChangeTypeNullablility(makeNullable: false);
            }

            return _memberTranslatorProvider.Translate(memberExpression.Update(innerExpression));
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var firstArgument = Visit(methodCallExpression.Arguments[0]);

                // In certain cases EF.Property would have convert node around the source.
                if (firstArgument.NodeType == ExpressionType.Convert
                    && firstArgument.Type == typeof(object))
                {
                    firstArgument = ((UnaryExpression)firstArgument).Operand;
                }

                if (firstArgument is EntityShaperExpression entityShaper)
                {
                    var entityType = entityShaper.EntityType;
                    var property = entityType.FindProperty((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value);

                    return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
                }
            }

            var @object = Visit(methodCallExpression.Object);
            var arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = Visit(methodCallExpression.Arguments[i]);
            }

            return _methodCallTranslatorProvider.Translate(methodCallExpression.Update(@object, arguments));
        }

        private static readonly MethodInfo _stringConcatObjectMethodInfo
            = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo _stringConcatStringMethodInfo
            = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        private SqlExpression ConvertToString(Expression expression, RelationalTypeMapping stringTypeMapping)
        {
            if (expression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Type == typeof(object))
            {
                expression = unaryExpression.Operand;
            }

            if (expression.Type != typeof(string))
            {
                expression = new SqlCastExpression(expression, typeof(string));
            }

            return expression is SqlExpression sql
                ? sql
                : new SqlExpression(
                    expression,
                    stringTypeMapping);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);

            if (binaryExpression.NodeType == ExpressionType.Add
                && (_stringConcatObjectMethodInfo.Equals(binaryExpression.Method)
                    || _stringConcatStringMethodInfo.Equals(binaryExpression.Method)))
            {
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);

                Debug.Assert(stringTypeMapping != null);

                left = ConvertToString(left, stringTypeMapping);
                right = ConvertToString(right, stringTypeMapping);

                return new SqlExpression(
                    binaryExpression.Update(left, VisitAndConvert(binaryExpression.Conversion, "VisitBinary"), right),
                    stringTypeMapping);
            }
            else if (binaryExpression.NodeType == ExpressionType.Equal
                    || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                // Convert null comparison
                var nullComparison = TransformNullComparison(left, right, binaryExpression.NodeType);

                if (nullComparison != null)
                {
                    return nullComparison;
                }
            }

            var newExpression = binaryExpression.Update(
                left, VisitAndConvert(binaryExpression.Conversion, "VisitBinary"), right);

            return _typeInference.Visit(newExpression);
        }

        private static Expression TransformNullComparison(Expression left, Expression right, ExpressionType expressionType)
        {
            var isLeftNullConstant = left is ConstantExpression leftConstant && leftConstant.Value == null;
            var isRightNullConstant = right is ConstantExpression rightConstant && rightConstant.Value == null;

            if ((isLeftNullConstant || isRightNullConstant)
                && ((isLeftNullConstant ? right : left) is SqlExpression sqlExpression))
            {
                return new SqlExpression(new IsNullExpression(sqlExpression, expressionType == ExpressionType.NotEqual));
            }

            return null;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression)
            {
                return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            if (newExpression.Members == null
                || newExpression.Arguments.Count == 0)
            {
                return null;
            }

            var bindings = new Expression[newExpression.Arguments.Count];

            for (var i = 0; i < bindings.Length; i++)
            {
                var translation = Visit(newExpression.Arguments[i]);

                if (translation == null)
                {
                    return null;
                }

                bindings[i] = translation;
            }

            return Expression.Constant(bindings);
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            if (operand is SqlExpression operandSql
                && unaryExpression.Type != typeof(object)
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                return new SqlCastExpression(
                    operandSql,
                    unaryExpression.Type);
            }

            if (unaryExpression.NodeType == ExpressionType.Not
                && operand is SqlExpression sql)
            {
                return new SqlExpression(Expression.Not(sql));
            }

            return unaryExpression.Update(operand);
        }
    }
}

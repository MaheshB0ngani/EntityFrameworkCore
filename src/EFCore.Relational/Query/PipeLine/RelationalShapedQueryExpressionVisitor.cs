﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;

        public RelationalShapedQueryCompilingExpressionVisitor(
            IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory2 querySqlGeneratorFactory)
            : base(entityMaterializerSource)
        {
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        private class SqlVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                VerifySql(binaryExpression.Left);
                VerifySql(binaryExpression.Right);

                return binaryExpression;
            }

            protected override Expression VisitBlock(BlockExpression node) => throw new InvalidOperationException();
            protected override CatchBlock VisitCatchBlock(CatchBlock node) => throw new InvalidOperationException();
            protected override Expression VisitConditional(ConditionalExpression node) => throw new InvalidOperationException();
            protected override Expression VisitConstant(ConstantExpression node) => node;
            protected override Expression VisitDebugInfo(DebugInfoExpression node) => throw new InvalidOperationException();
            protected override Expression VisitDefault(DefaultExpression node) => throw new InvalidOperationException();
            protected override Expression VisitDynamic(DynamicExpression node) => throw new InvalidOperationException();
            protected override ElementInit VisitElementInit(ElementInit node) => throw new InvalidOperationException();

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                switch (extensionExpression)
                {
                    case SelectExpression selectExpression:
                        {
                            foreach (var projection in selectExpression.Projection)
                            {
                                VerifySql(projection);
                            }

                            if (selectExpression.Predicate != null)
                            {
                                VerifySql(selectExpression.Predicate);
                            }

                            foreach (var ordering in selectExpression.Orderings)
                            {
                                VerifySql(ordering.Expression);
                            }

                            if (selectExpression.Offset != null)
                            {
                                VerifySql(selectExpression.Offset);
                            }

                            if (selectExpression.Limit != null)
                            {
                                VerifySql(selectExpression.Limit);
                            }

                            return selectExpression;
                        }

                    case ColumnExpression _:
                        return extensionExpression;

                    case SqlFunctionExpression sqlFunctionExpression:
                        {
                            if (sqlFunctionExpression.Instance != null)
                            {
                                VerifySql(sqlFunctionExpression.Instance);
                            }

                            foreach (var argument in sqlFunctionExpression.Arguments)
                            {
                                VerifySql(argument);
                            }

                            return sqlFunctionExpression;
                        }
                }

                throw new InvalidOperationException();
            }

            protected override Expression VisitGoto(GotoExpression node) => throw new InvalidOperationException();
            protected override Expression VisitIndex(IndexExpression node) => throw new InvalidOperationException();
            protected override Expression VisitInvocation(InvocationExpression node) => throw new InvalidOperationException();
            protected override Expression VisitLabel(LabelExpression node) => throw new InvalidOperationException();
            protected override LabelTarget VisitLabelTarget(LabelTarget node) => throw new InvalidOperationException();
            protected override Expression VisitLambda<T>(Expression<T> node) => throw new InvalidOperationException();
            protected override Expression VisitListInit(ListInitExpression node) => throw new InvalidOperationException();
            protected override Expression VisitLoop(LoopExpression node) => throw new InvalidOperationException();
            protected override Expression VisitMember(MemberExpression node) => throw new InvalidOperationException();
            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) => throw new InvalidOperationException();
            protected override MemberBinding VisitMemberBinding(MemberBinding node) => throw new InvalidOperationException();
            protected override Expression VisitMemberInit(MemberInitExpression node) => throw new InvalidOperationException();
            protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) => throw new InvalidOperationException();
            protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) => throw new InvalidOperationException();
            protected override Expression VisitMethodCall(MethodCallExpression node) => throw new InvalidOperationException();
            protected override Expression VisitNew(NewExpression node) => throw new InvalidOperationException();
            protected override Expression VisitNewArray(NewArrayExpression node) => throw new InvalidOperationException();
            protected override Expression VisitParameter(ParameterExpression node) => node;
            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) => throw new InvalidOperationException();
            protected override Expression VisitSwitch(SwitchExpression node) => throw new InvalidOperationException();
            protected override SwitchCase VisitSwitchCase(SwitchCase node) => throw new InvalidOperationException();
            protected override Expression VisitTry(TryExpression node) => throw new InvalidOperationException();
            protected override Expression VisitTypeBinary(TypeBinaryExpression node) => throw new InvalidOperationException();
            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Not)
                {
                    VerifySql(node.Operand);

                    return node;
                }

                throw new InvalidOperationException();
            }

            private void VerifySql(Expression expression)
            {
                if (expression is SqlExpression sql)
                {
                    Visit(sql.Expression);

                    return;
                }

                throw new InvalidOperationException();
            }
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperLambda = InjectEntityMaterializer(shapedQueryExpression.ShaperExpression);
            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

            selectExpression.ApplyProjection();

            new SqlVerifyingExpressionVisitor().Visit(selectExpression);

            var newBody = new RelationalProjectionBindingRemovingExpressionVisitor(selectExpression)
                .Visit(shaperLambda.Body);

            shaperLambda = Expression.Lambda(
                newBody,
                QueryCompilationContext2.QueryContextParameter,
                RelationalProjectionBindingRemovingExpressionVisitor.DataReaderParameter);

            return Expression.New(
                typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext2.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(_querySqlGeneratorFactory.Create()),
                Expression.Constant(selectExpression),
                Expression.Constant(shaperLambda.Compile()));
        }

        private class QueryingEnumerable<T> : IEnumerable<T>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, DbDataReader, T> _shaper;
            private readonly QuerySqlGenerator _querySqlGenerator;

            public QueryingEnumerable(RelationalQueryContext relationalQueryContext,
                QuerySqlGenerator querySqlGenerator,
                SelectExpression selectExpression,
                Func<QueryContext, DbDataReader, T> shaper)
            {
                _relationalQueryContext = relationalQueryContext;
                _querySqlGenerator = querySqlGenerator;
                _selectExpression = selectExpression;
                _shaper = shaper;
            }

            public IEnumerator<T> GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class Enumerator : IEnumerator<T>
            {
                private RelationalDataReader _dataReader;
                private readonly RelationalQueryContext _relationalQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, DbDataReader, T> _shaper;
                private readonly QuerySqlGenerator _querySqlGenerator;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable)
                {
                    _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _querySqlGenerator = queryingEnumerable._querySqlGenerator;
                }

                public T Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _dataReader.Dispose();
                    _dataReader = null;
                    _relationalQueryContext.Connection.Close();
                }

                public bool MoveNext()
                {
                    if (_dataReader == null)
                    {
                        _relationalQueryContext.Connection.Open();

                        try
                        {
                            var relationalCommand = _querySqlGenerator
                                .GetCommand(_selectExpression, _relationalQueryContext.ParameterValues);

                            _dataReader
                                = relationalCommand.ExecuteReader(
                                    _relationalQueryContext.Connection,
                                    _relationalQueryContext.ParameterValues);
                        }
                        catch
                        {
                            // If failure happens creating the data reader, then it won't be available to
                            // handle closing the connection, so do it explicitly here to preserve ref counting.
                            _relationalQueryContext.Connection.Close();

                            throw;
                        }
                    }

                    var hasNext = _dataReader.Read();

                    Current
                        = hasNext
                            ? _shaper(_relationalQueryContext, _dataReader.DbDataReader)
                            : default;

                    return hasNext;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class RelationalProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            public static readonly ParameterExpression DataReaderParameter
                = Expression.Parameter(typeof(DbDataReader), "dataReader");

            private readonly IDictionary<ParameterExpression, int> _materializationContextBindings
                = new Dictionary<ParameterExpression, int>();

            public RelationalProjectionBindingRemovingExpressionVisitor(SelectExpression selectExpression)
            {
                _selectExpression = selectExpression;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;

                    _materializationContextBindings[parameterExpression]
                        = (int)((ConstantExpression)_selectExpression.GetProjectionExpression(((ProjectionBindingExpression)newExpression.Arguments[0]).ProjectionMember)).Value;

                    var updatedExpression = Expression.New(newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    var originalIndex = (int)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    var materializationContext = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object;
                    var indexOffset = _materializationContextBindings[materializationContext];

                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;

                    return CreateGetValueExpression(
                        originalIndex + indexOffset,
                        property,
                        property.FindRelationalMapping(),
                        methodCallExpression.Type);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projectionIndex = (int)((ConstantExpression)_selectExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember)).Value;
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        projectionIndex,
                        null,
                        projection.TypeMapping,
                        projectionBindingExpression.Type);
                }

                return base.VisitExtension(extensionExpression);
            }

            private static Expression CreateGetValueExpression(
                int index,
                IProperty property,
                RelationalTypeMapping typeMapping,
                Type clrType)
            {
                var getMethod = typeMapping.GetDataReaderMethod();

                var indexExpression = Expression.Constant(index);

                Expression valueExpression
                    = Expression.Call(
                        DataReaderParameter,
                        //getMethod.DeclaringType != typeof(DbDataReader)
                        //    ? Expression.Convert(DataReaderParameter, getMethod.DeclaringType)
                        //    : DataReaderParameter,
                        getMethod,
                        indexExpression);

                //valueExpression = mapping.CustomizeDataReaderExpression(valueExpression);

                var converter = typeMapping.Converter;

                if (converter != null)
                {
                    if (valueExpression.Type != converter.ProviderClrType)
                    {
                        valueExpression = Expression.Convert(valueExpression, converter.ProviderClrType);
                    }

                    valueExpression = new ReplacingExpressionVisitor(
                        new Dictionary<Expression, Expression>
                            {
                                { converter.ConvertFromProviderExpression.Parameters.Single(), valueExpression }
                            }
                        ).Visit(converter.ConvertFromProviderExpression.Body);
                }

                if (valueExpression.Type != clrType)
                {
                    valueExpression = Expression.Convert(valueExpression, clrType);
                }

                //var exceptionParameter
                //    = Expression.Parameter(typeof(Exception), name: "e");

                //var property = materializationInfo.Property;

                //if (detailedErrorsEnabled)
                //{
                //    var catchBlock
                //        = Expression
                //            .Catch(
                //                exceptionParameter,
                //                Expression.Call(
                //                    _throwReadValueExceptionMethod
                //                        .MakeGenericMethod(valueExpression.Type),
                //                    exceptionParameter,
                //                    Expression.Call(
                //                        dataReaderExpression,
                //                        _getFieldValueMethod.MakeGenericMethod(typeof(object)),
                //                        indexExpression),
                //                    Expression.Constant(property, typeof(IPropertyBase))));

                //    valueExpression = Expression.TryCatch(valueExpression, catchBlock);
                //}

                //if (box && valueExpression.Type.GetTypeInfo().IsValueType)
                //{
                //    valueExpression = Expression.Convert(valueExpression, typeof(object));
                //}

                if (property?.IsNullable != false
                    || property.DeclaringEntityType.BaseType != null)
                {
                    valueExpression
                        = Expression.Condition(
                            Expression.Call(DataReaderParameter, _isDbNullMethod, indexExpression),
                            Expression.Default(valueExpression.Type),
                            valueExpression);
                }

                return valueExpression;
            }

            private static readonly MethodInfo _isDbNullMethod
                = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));
            private readonly SelectExpression _selectExpression;
        }
    }
}

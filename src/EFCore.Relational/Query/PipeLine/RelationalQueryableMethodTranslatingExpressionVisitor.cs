﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public RelationalQueryableMethodTranslatingExpressionVisitor(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _sqlTranslator = new RelationalSqlTranslatingExpressionVisitor(
                typeMappingSource,
                typeMappingApplyingExpressionVisitor,
                memberTranslatorProvider,
                methodCallTranslatorProvider);
            _typeMappingSource = typeMappingSource;
        }

        protected override ShapedQueryExpression TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        {
            //var translation = TranslateLambdaExpression(source, predicate, true);

            //if (translation?.IsCondition == true)
            //{
            //    ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

            //    return source;
            //}

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            var translation = new SqlFunctionExpression(
                    null,
                    "COUNT",
                    null,
                    new[]
                    {
                        new SqlFragmentExpression("*")
                    },
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int)),
                    false);

            var _projectionMapping = new Dictionary<ProjectionMember, Expression>
            {
                { new ProjectionMember(), translation }
            };

            selectExpression.ClearOrdering();
            selectExpression.ApplyProjection(_projectionMapping);

            source.ShaperExpression
                = Expression.Lambda(
                    new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(int)),
                    source.ShaperExpression.Parameters);

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            selectExpression.ApplyLimit(
                new SqlConstantExpression(
                    Expression.Constant(1),
                    _typeMappingSource.FindMapping(typeof(int)),
                    false));

            return source;
        }

        protected override ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression predicate, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            selectExpression.Reverse();

            selectExpression.ApplyLimit(
                new SqlConstantExpression(
                    Expression.Constant(1),
                    _typeMappingSource.FindMapping(typeof(int)),
                    false));

            return source;
        }

        protected override ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateOfType(ShapedQueryExpression source, Type resultType) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = TranslateLambdaExpression(source, keySelector, false);

            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).ApplyOrderBy(new OrderingExpression(translation, ascending));

                return source;
            }

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateReverse(ShapedQueryExpression source) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
        {
            if (selector.Body == selector.Parameters[0])
            {
                return source;
            }

            var parameterBindings = new Dictionary<Expression, Expression>
            {
                { selector.Parameters.Single(), source.ShaperExpression.Body }
            };

            var newSelectorBody = new ReplacingExpressionVisitor(parameterBindings).Visit(selector.Body);

            newSelectorBody = new RelationalProjectionBindingExpressionVisitor(_sqlTranslator)
                .Translate((SelectExpression)source.QueryExpression, newSelectorBody);

            source.ShaperExpression = Expression.Lambda(newSelectorBody, source.ShaperExpression.Parameters);

            return source;
        }

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression predicate, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            selectExpression.ApplyLimit(
                new SqlConstantExpression(
                    Expression.Constant(1),
                    _typeMappingSource.FindMapping(typeof(int)),
                    false));

            return source;
        }

        protected override ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            //var selectExpression = (SelectExpression)source.QueryExpression;
            //var translation = TranslateExpression(selectExpression, count, false);

            //if (translation != null)
            //{
            //    selectExpression.ApplyOffset(translation);

            //    return source;
            //}

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count)
        {
            //var selectExpression = (SelectExpression)source.QueryExpression;
            //var translation = TranslateExpression(selectExpression, count, false);

            //if (translation != null)
            //{
            //    selectExpression.ApplyLimit(translation);

            //    return source;
            //}

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            //var translation = TranslateLambdaExpression(source, keySelector, false);

            //if (translation != null)
            //{
            //    ((SelectExpression)source.QueryExpression).ApplyThenBy(new OrderingExpression(translation, ascending));

            //    return source;
            //}

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate, true);

            if (translation?.IsCondition == true)
            {
                ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

                return source;
            }

            throw new InvalidOperationException();
        }

        private SqlExpression TranslateExpression(SelectExpression selectExpression, Expression expression, bool condition)
        {
            return _sqlTranslator.Translate(selectExpression, expression, condition);
        }

        private SqlExpression TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression, bool condition)
        {
            var parameterBindings = new Dictionary<Expression, Expression>
            {
                { lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression.Body }
            };

            var lambdaBody = new ReplacingExpressionVisitor(parameterBindings).Visit(lambdaExpression.Body);

            return TranslateExpression((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody, condition);
        }
    }
}

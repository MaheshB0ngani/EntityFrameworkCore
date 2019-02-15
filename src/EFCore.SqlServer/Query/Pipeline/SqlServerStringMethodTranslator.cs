// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    //public class SqlServerStringMethodTranslator : IMethodCallTranslator
    //{
    //    private static readonly MethodInfo _indexOfMethodInfo
    //        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });

    //    private static readonly MethodInfo _replaceMethodInfo
    //        = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) });

    //    private static readonly MethodInfo _methodInfo
    //        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

    //    private static readonly MethodInfo _concat
    //        = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });
    //    private readonly IRelationalTypeMappingSource _typeMappingSource;

    //    public SqlServerStringMethodTranslator(IRelationalTypeMappingSource typeMappingSource)
    //    {
    //        _typeMappingSource = typeMappingSource;
    //    }

    //    public Expression Translate(MethodCallExpression methodCallExpression)
    //    {
    //        var method = methodCallExpression.Method;
    //        if (_indexOfMethodInfo.Equals(method))
    //        {
    //            var @object = methodCallExpression.Object;
    //            var argument = methodCallExpression.Arguments[0];

    //            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(@object, argument);
    //            var intTypeMapping = _typeMappingSource.FindMapping(typeof(int));

    //            Debug.Assert(stringTypeMapping != null, "At least one argument would be server correlated and should have typeMapping assigned");

    //            var charIndexExpression = Expression.Subtract(
    //                new SqlFunctionExpression(
    //                    null,
    //                    "CHARINDEX",
    //                    null,
    //                    new[]
    //                    {
    //                        argument.ApplyTypeMapping(stringTypeMapping),
    //                        @object.ApplyTypeMapping(stringTypeMapping)
    //                    },
    //                    methodCallExpression.Type).ApplyTypeMapping(stringTypeMapping),
    //                Expression.Constant(1).ApplyTypeMapping(intTypeMapping)).ApplyTypeMapping(intTypeMapping);

    //            return new CaseExpression(
    //                new[]
    //                {
    //                    new CaseWhenClause(
    //                        new sqlExpressionOld(
    //                            Expression.Equal(
    //                                argument.ApplyTypeMapping(stringTypeMapping),
    //                                Expression.Constant(string.Empty).ApplyTypeMapping(stringTypeMapping))),
    //                        Expression.Constant(0).ApplyTypeMapping(intTypeMapping))
    //                },
    //                charIndexExpression).ApplyTypeMapping(intTypeMapping);
    //        }

    //        if (_replaceMethodInfo.Equals(method))
    //        {
    //            var @object = methodCallExpression.Object.ApplyDefaultTypeMapping(_typeMappingSource);
    //            var argument = methodCallExpression.Arguments[0].ApplyDefaultTypeMapping(_typeMappingSource);

    //            return new SqlFunctionExpression(
    //                null,
    //                "REPLACE",
    //                null,
    //                new[]
    //                {
    //                    @object,
    //                    argument
    //                },
    //                methodCallExpression.Type)
    //                .ApplyDefaultTypeMapping(_typeMappingSource);
    //        }

    //        return null;
    //    }
    //}
}

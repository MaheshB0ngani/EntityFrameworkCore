// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.Linq.Expressions;
//using System.Reflection;
//using Microsoft.EntityFrameworkCore.Relational.Query.PipeLine;
//using Microsoft.EntityFrameworkCore.Storage;

//namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
//{
//    public class SqlServerNewGuidTranslator : IMethodCallTranslator
//    {
//        private readonly IRelationalTypeMappingSource _typeMappingSource;
//        private static MethodInfo _methodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Array.Empty<Type>());

//        public SqlServerNewGuidTranslator(IRelationalTypeMappingSource typeMappingSource)
//        {
//            _typeMappingSource = typeMappingSource;
//        }

//        public Expression Translate(MethodCallExpression methodCallExpression)
//        {
//            return _methodInfo.Equals(methodCallExpression.Method)
//                ? new SqlFunctionExpression(
//                    null,
//                    "NEWID",
//                    null,
//                    null,
//                    methodCallExpression.Type).ApplyDefaultTypeMapping(_typeMappingSource)
//                : null;
//        }
//    }
//}

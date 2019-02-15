// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System.Linq.Expressions;
//using Microsoft.EntityFrameworkCore.Relational.Query.PipeLine;
//using Microsoft.EntityFrameworkCore.Storage;

//namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
//{
//    public class SqlServerStringMemberTranslator : IMemberTranslator
//    {
//        private readonly IRelationalTypeMappingSource _typeMappingSource;

//        public SqlServerStringMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
//        {
//            _typeMappingSource = typeMappingSource;
//        }

//        public Expression Translate(MemberExpression memberExpression)
//        {
//            if (memberExpression.Expression is sqlExpressionOld sql
//                && sql.Type == typeof(string)
//                && memberExpression.Member.Name == nameof(string.Length))
//            {
//                return new SqlCastExpression(
//                    new SqlFunctionExpression(
//                        null,
//                        "LEN",
//                        null,
//                        new[] { sql },
//                        memberExpression.Type)
//                        .ApplyDefaultTypeMapping(_typeMappingSource),
//                    memberExpression.Type);
//            }

//            return null;
//        }
//    }
//}

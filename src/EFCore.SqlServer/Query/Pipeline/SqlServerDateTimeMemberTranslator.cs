// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq.Expressions;
//using Microsoft.EntityFrameworkCore.Relational.Query.PipeLine;
//using Microsoft.EntityFrameworkCore.Storage;

//namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
//{
//    public class SqlServerDateTimeMemberTranslator : IMemberTranslator
//    {
//        private readonly IRelationalTypeMappingSource _typeMappingSource;
//        private static readonly Dictionary<string, string> _datePartMapping
//            = new Dictionary<string, string>
//            {
//                { nameof(DateTime.Year), "year" },
//                { nameof(DateTime.Month), "month" },
//                { nameof(DateTime.DayOfYear), "dayofyear" },
//                { nameof(DateTime.Day), "day" },
//                { nameof(DateTime.Hour), "hour" },
//                { nameof(DateTime.Minute), "minute" },
//                { nameof(DateTime.Second), "second" },
//                { nameof(DateTime.Millisecond), "millisecond" }
//            };

//        public SqlServerDateTimeMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
//        {
//            _typeMappingSource = typeMappingSource;
//        }

//        public Expression Translate(MemberExpression memberExpression)
//        {
//            var declaringType = memberExpression.Member.DeclaringType;

//            if (declaringType == typeof(DateTime)
//                || declaringType == typeof(DateTimeOffset))
//            {
//                var memberName = memberExpression.Member.Name;

//                if (memberExpression.Expression is sqlExpressionOld sql)
//                {
//                    // Instance members
//                    var typeMapping = sql.TypeMapping;

//                    if (_datePartMapping.TryGetValue(memberName, out var datePart))
//                    {
//                        return new SqlFunctionExpression(
//                            null,
//                            "DATEPART",
//                            null,
//                            new[]{
//                                new SqlFragmentExpression(datePart),
//                                sql
//                            },
//                            memberExpression.Type);
//                    }

//                    if (string.Equals(memberName, nameof(DateTime.Date)))
//                    {
//                        return new SqlFunctionExpression(
//                            null,
//                            "CONVERT",
//                            null,
//                            new[]{
//                                new SqlFragmentExpression("date"),
//                                sql
//                            },
//                            memberExpression.Type);
//                    }

//                    if (string.Equals(memberName, nameof(DateTime.TimeOfDay)))
//                    {
//                        return new SqlCastExpression(sql, memberExpression.Type);
//                    }
//                }
//                else
//                {
//                    Debug.Assert(memberExpression.Expression == null, "Must be static member.");

//                    var typeMapping = _typeMappingSource.FindMapping(memberExpression.Type);

//                    switch (memberName)
//                    {
//                        case nameof(DateTime.Now):
//                            return new SqlFunctionExpression(
//                                    null,
//                                    declaringType == typeof(DateTime) ? "GETDATE" : "SYSDATETIMEOFFSET",
//                                    null,
//                                    null,
//                                    memberExpression.Type);

//                        case nameof(DateTime.UtcNow):
//                            var serverTranslation = new SqlFunctionExpression(
//                                    null,
//                                    declaringType == typeof(DateTime) ? "GETUTCDATE" : "SYSUTCDATETIME",
//                                    null,
//                                    null,
//                                    memberExpression.Type);

//                            return declaringType == typeof(DateTime)
//                                ? (Expression)serverTranslation
//                                : new SqlCastExpression(
//                                    serverTranslation,
//                                    serverTranslation.Type);

//                        case nameof(DateTime.Today):
//                            return new SqlFunctionExpression(
//                                null,
//                                "CONVERT",
//                                null,
//                                new[]{
//                                new SqlFragmentExpression("date"),
//                                new SqlFunctionExpression(
//                                    null,
//                                    "GETDATE",
//                                    null,
//                                    null,
//                                    memberExpression.Type)
//                                    .ApplyDefaultTypeMapping(_typeMappingSource)
//                                },
//                                memberExpression.Type)
//                                .ApplyDefaultTypeMapping(_typeMappingSource);

//                    }
//                }





//            }

//            return null;
//        }
//    }
//}

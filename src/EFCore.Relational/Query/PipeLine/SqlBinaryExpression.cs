// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlBinaryExpression : SqlExpression
    {
        public SqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            Type type,
            RelationalTypeMapping typeMapping,
            bool condition)
            : base(type, typeMapping, condition)
        {
            OperatorType = operatorType;
            Left = left;
            Right = right;
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Left { get; }
        public SqlExpression Right { get; }
    }
}

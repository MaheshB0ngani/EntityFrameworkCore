// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlUnaryExpression : SqlExpression
    {
        public SqlUnaryExpression(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping,
            bool condition)
            : base(type, typeMapping, condition)
        {
            OperatorType = operatorType;
            Operand = operand;
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Operand { get; }
    }
}

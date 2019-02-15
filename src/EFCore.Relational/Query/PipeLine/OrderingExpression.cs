// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class OrderingExpression : Expression
    {
        public OrderingExpression(SqlExpression expression, bool ascending)
        {
            Expression = expression;
            Ascending = ascending;
        }

        public SqlExpression Expression { get; }
        public bool Ascending { get; }
    }
}

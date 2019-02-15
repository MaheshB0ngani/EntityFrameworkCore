// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public abstract class SqlExpression : Expression
    {
        protected SqlExpression(Type type, RelationalTypeMapping typeMapping, bool condition)
        {
            Type = type;
            IsCondition = condition;
            TypeMapping = typeMapping;
        }

        public SqlExpression InvertCondition()
        {
            IsCondition = !IsCondition;

            return this;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type { get; }
        public bool IsCondition { get; private set; }
        public RelationalTypeMapping TypeMapping { get; }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public static class ExpressionExtensions
    {
        public static RelationalTypeMapping InferTypeMapping(params Expression[] expressions)
        {
            for (var i = 0; i < expressions.Length; i++)
            {
                if (expressions[i] is SqlExpression sql)
                {
                    return sql.TypeMapping;
                }
            }

            return null;
        }
    }
}

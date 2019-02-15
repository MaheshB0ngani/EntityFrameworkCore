// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlFunctionExpression : SqlExpression
    {
        public SqlFunctionExpression(
            Expression instance,
            string functionName,
            string schema,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping,
            bool condition)
            : base(type, typeMapping, condition)
        {
            Instance = instance;
            FunctionName = functionName;
            Schema = schema;
            Arguments = (arguments ?? Array.Empty<SqlExpression>()).ToList();
        }

        public string FunctionName { get; }
        public string Schema { get; }
        public IReadOnlyList<SqlExpression> Arguments { get; }
        public Expression Instance { get; }
    }
}

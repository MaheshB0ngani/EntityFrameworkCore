// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlFragmentExpression : SqlExpression
    {
        public SqlFragmentExpression(string sql)
            : base(typeof(string), null, false)
        {
            Sql = sql;
        }

        public string Sql { get; }
    }
}

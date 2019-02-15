// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Relational.Query.PipeLine;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqlServerMethodCallTranslatorProvider(IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
            : base(typeMappingSource, typeMappingApplyingExpressionVisitor)
        {
            //AddTranslators(new IMethodCallTranslator[]
            //{
            //    new SqlServerMathTranslator(typeMappingSource),
            //    new SqlServerNewGuidTranslator(typeMappingSource),
            //    new SqlServerStringMethodTranslator(typeMappingSource)
            //});
        }
    }
}

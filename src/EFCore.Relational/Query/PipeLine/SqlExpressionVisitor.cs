// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public abstract class SqlExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ColumnExpression columnExpression:
                    return VisitColumn(columnExpression);

                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);

                case TableExpression tableExpression:
                    return VisitTable(tableExpression);

                case SqlBinaryExpression sqlBinaryExpression:
                    return VisitSqlBinary(sqlBinaryExpression);

                case SqlConstantExpression sqlConstantExpression:
                    return VisitSqlConstant(sqlConstantExpression);

                case SqlParameterExpression sqlParameterExpression:
                    return VisitSqlParameter(sqlParameterExpression);

                case OrderingExpression orderingExpression:
                    return VisitOrdering(orderingExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return VisitSqlFunction(sqlFunctionExpression);

                case SqlFragmentExpression sqlFragmentExpression:
                    return VisitSqlFragment(sqlFragmentExpression);

                case SqlUnaryExpression sqlUnaryExpression:
                    return VisitSqlUnary(sqlUnaryExpression);
            }

            return base.VisitExtension(node);
        }

        protected abstract Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression);
        protected abstract Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression);
        protected abstract Expression VisitOrdering(OrderingExpression orderingExpression);
        protected abstract Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression);
        protected abstract Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression);
        protected abstract Expression VisitColumn(ColumnExpression columnExpression);
        protected abstract Expression VisitSelect(SelectExpression selectExpression);
        protected abstract Expression VisitTable(TableExpression tableExpression);
        protected abstract Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression);
        protected abstract Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression);
    }
}

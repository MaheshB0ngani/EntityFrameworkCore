﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class QuerySqlGenerator : ExpressionVisitor
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalCommandBuilder _relationalCommandBuilder;
        private IReadOnlyDictionary<string, object> _parametersValues;
        //private ParameterNameGenerator _parameterNameGenerator;

        private static readonly Dictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        public QuerySqlGenerator(IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper)
        {
            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public virtual IRelationalCommand GetCommand(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            //_parameterNameGenerator = Dependencies.ParameterNameGeneratorFactory.Create();

            _parametersValues = parameterValues;

            Visit(selectExpression);

            return _relationalCommandBuilder.Build();
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);

                case ColumnExpression columnExpression:
                    return VisitColumn(columnExpression);

                case TableExpression tableExpression:
                    return VisitTable(tableExpression);

                case SqlFragmentExpression sqlFragmentExpression:
                    _relationalCommandBuilder.Append(sqlFragmentExpression.Sql);

                    return sqlFragmentExpression;

                case SqlExpression sqlExpression:
                    var innerExpression = sqlExpression.Expression;
                    if (innerExpression is ConstantExpression constantExpression)
                    {
                        _relationalCommandBuilder
                            .Append(GenerateConstantLiteral(constantExpression.Value, sqlExpression.TypeMapping));
                    }
                    else if (innerExpression is ParameterExpression parameterExpression)
                    {
                        _relationalCommandBuilder
                            .Append(GenerateParameter(parameterExpression, sqlExpression.TypeMapping));
                    }
                    else if (innerExpression is SqlCastExpression sqlCastExpression)
                    {
                        _relationalCommandBuilder.Append("CAST(");
                        Visit(sqlCastExpression.Expression);
                        _relationalCommandBuilder.Append(" AS ");
                        _relationalCommandBuilder.Append(sqlExpression.TypeMapping.StoreType);
                        _relationalCommandBuilder.Append(")");

                        return sqlCastExpression;
                    }
                    else
                    {
                        Visit(innerExpression);
                    }

                    return sqlExpression;

                case OrderingExpression orderingExpression:
                    return VisitOrdering(orderingExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return VisitSqlFunction(sqlFunctionExpression);

                case IsNullExpression isNullExpression:
                    return VisitIsNullExpression(isNullExpression);

                case CaseExpression caseExpression:
                    return VisitCase(caseExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression VisitCase(CaseExpression caseExpression)
        {
            _relationalCommandBuilder.Append("CASE");

            using (_relationalCommandBuilder.Indent())
            {
                foreach (var whenClause in caseExpression.WhenClauses)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("WHEN ");
                    Visit(whenClause.Test);
                    _relationalCommandBuilder.Append(" THEN ");
                    Visit(whenClause.Result);
                }

                if (caseExpression.ElseResult != null)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("ELSE ");
                    Visit(caseExpression.ElseResult);
                }
            }

            _relationalCommandBuilder
                .AppendLine()
                .Append("END");

            return caseExpression;
        }

        private Expression VisitIsNullExpression(IsNullExpression isNullExpression)
        {
            Visit(isNullExpression.Expression);

            _relationalCommandBuilder.Append($" IS {(isNullExpression.Negated ? "NOT" : "")} NULL");

            return isNullExpression;
        }

        private Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.Schema != null)
            {
                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Schema))
                    .Append(".")
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.FunctionName));
            }
            else
            {
                _relationalCommandBuilder.Append(sqlFunctionExpression.FunctionName);
            }

            _relationalCommandBuilder.Append("(");

            GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));

            _relationalCommandBuilder.Append(")");

            return sqlFunctionExpression;
        }

        private Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            var innerExpression = orderingExpression.Expression.Expression;
            if (innerExpression is ConstantExpression constantExpression
                || innerExpression is ParameterExpression parameterExpression)
            {
                _relationalCommandBuilder
                    .Append("(SELECT 1)");
            }
            else
            {
                Visit(orderingExpression.Expression);

                if (!orderingExpression.Ascending)
                {
                    _relationalCommandBuilder.Append(" DESC");
                }
            }

            return orderingExpression;
        }

        private Expression VisitTable(TableExpression tableExpression)
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Table, tableExpression.Schema))
                .Append(" AS ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        protected Expression VisitColumn(ColumnExpression columnExpression)
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        protected Expression VisitSelect(SelectExpression selectExpression)
        {
            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                _relationalCommandBuilder.Append("TOP(");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(") ");
            }

            if (selectExpression.Projection.Any())
            {
                GenerateList(selectExpression.Projection, e => Visit(e));
            }
            else
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("FROM ");

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            if (selectExpression.Orderings.Any())
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("ORDER BY ");

                GenerateList(selectExpression.Orderings, e => Visit(e));
            }

            if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                _relationalCommandBuilder.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _relationalCommandBuilder.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    _relationalCommandBuilder.Append(" ROWS ONLY");
                }
            }

            return selectExpression;
        }

        protected string GenerateParameter(ParameterExpression parameterExpression, RelationalTypeMapping typeMapping)
        {
            var parameterNameInCommand = _sqlGenerationHelper.GenerateParameterName(parameterExpression.Name);

            if (_relationalCommandBuilder.ParameterBuilder.Parameters
                .All(p => p.InvariantName != parameterExpression.Name))
            {
                _relationalCommandBuilder.AddParameter(
                    parameterExpression.Name,
                    parameterNameInCommand,
                    typeMapping,
                    parameterExpression.Type.IsNullableType());
            }

            return _sqlGenerationHelper.GenerateParameterNamePlaceholder(parameterExpression.Name);
        }

        protected string GenerateConstantLiteral(object value, RelationalTypeMapping typeMapping)
        {
            //var mappingClrType = typeMapping.ClrType.UnwrapNullableType();

            //if (value == null
            //    || mappingClrType.IsInstanceOfType(value)
            //    || value.GetType().IsInteger()
            //    && (mappingClrType.IsInteger()
            //    || mappingClrType.IsEnum))
            //{
            //    if (value?.GetType().IsInteger() == true
            //        && mappingClrType.IsEnum)
            //    {
            //        value = Enum.ToObject(mappingClrType, value);
            //    }
            //}

            return typeMapping.GenerateSqlLiteral(value);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);

            _relationalCommandBuilder.Append(_operatorMap[binaryExpression.NodeType]);

            Visit(binaryExpression.Right);

            return binaryExpression;
        }

        private void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                generationAction(items[i]);
            }
        }
    }
}

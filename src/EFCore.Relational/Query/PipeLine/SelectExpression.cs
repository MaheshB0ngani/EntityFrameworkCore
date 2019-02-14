// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SelectExpression : TableExpressionBase
    {
        private IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();

        private List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<SqlExpression> _projection = new List<SqlExpression>();
        private SqlExpression _predicate;
        private SqlExpression _limit;
        private SqlExpression _offset;
        private List<OrderingExpression> _orderings = new List<OrderingExpression>();

        public IReadOnlyList<SqlExpression> Projection => _projection;
        public IReadOnlyList<TableExpressionBase> Tables => _tables;
        public IReadOnlyList<OrderingExpression> Orderings => _orderings;
        public SqlExpression Predicate => _predicate;
        public SqlExpression Limit => _limit;
        public SqlExpression Offset => _offset;

        public SelectExpression(IEntityType entityType)
            : base("")
        {
            var tableExpression = new TableExpression(
                entityType.Relational().TableName,
                entityType.Relational().Schema,
                entityType.Relational().TableName.ToLower().Substring(0,1));

            _tables.Add(tableExpression);

            _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, tableExpression);
        }

        public Expression BindProperty(Expression projectionExpression, IProperty property)
        {
            var member = (projectionExpression as ProjectionBindingExpression).ProjectionMember;

            return ((EntityProjectionExpression)_projectionMapping[member]).GetProperty(property);
        }

        public void ApplyProjection()
        {
            var index = 0;
            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                result[keyValuePair.Key] = Constant(index);
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    foreach (var property in entityProjection.EntityType.GetProperties())
                    {
                        _projection.Add(entityProjection.GetProperty(property));
                        index++;
                    }
                }
                else
                {
                    _projection.Add((SqlExpression)keyValuePair.Value);
                    index++;
                }
            }

            _projectionMapping = result;
        }

        public void ApplyPredicate(SqlExpression expression)
        {
            if (_predicate == null)
            {
                _predicate = expression;
            }
            else
            {
                _predicate = new SqlExpression(AndAlso(_predicate, expression));
            }
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public void ApplyProjection(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            _projectionMapping.Clear();

            foreach (var kvp in projectionMapping)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        public Expression GetProjectionExpression(ProjectionMember projectionMember)
        {
            return _projectionMapping[projectionMember];
        }

        public void ApplyOrderBy(OrderingExpression orderingExpression)
        {
            _orderings.Clear();
            _orderings.Add(orderingExpression);
        }

        public void ApplyThenBy(OrderingExpression orderingExpression)
        {
            _orderings.Add(orderingExpression);
        }

        public void ApplyLimit(SqlExpression sqlExpression)
        {
            _limit = sqlExpression;
        }

        public void ApplyOffset(SqlExpression sqlExpression)
        {
            _offset = sqlExpression;
        }

        public void Reverse()
        {
            var existingOrdering = _orderings.ToArray();

            _orderings.Clear();

            for (var i = 0; i < existingOrdering.Length; i++)
            {
                _orderings.Add(
                    new OrderingExpression(
                        existingOrdering[i].Expression,
                        !existingOrdering[i].Ascending));
            }
        }

        public void ClearOrdering()
        {
            _orderings.Clear();
        }
    }

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

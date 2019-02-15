﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    public class NavigationBindingExpression : Expression, IPrintable
    {
        public Expression Operand { get; }
        public ParameterExpression RootParameter { get; }
        public IEntityType EntityType { get; }
        public IReadOnlyList<INavigation> Navigations { get; }
        public SourceMapping SourceMapping { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => true;
        public override Type Type => Operand.Type;

        public override Expression Reduce()
            => Operand;

        public NavigationBindingExpression(
            Expression operand,
            ParameterExpression rootParameter,
            List<INavigation> navigations,
            IEntityType entityType,
            SourceMapping sourceMapping)
        {
            Operand = operand;
            RootParameter = rootParameter;
            Navigations = navigations.AsReadOnly();
            EntityType = entityType;
            SourceMapping = sourceMapping;
        }

        public void Print([NotNull] ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("BINDING(");
            expressionPrinter.Visit(RootParameter);
            expressionPrinter.StringBuilder.Append(" | ");
            expressionPrinter.StringBuilder.Append(string.Join(", ", Navigations.Select(n => n.Name)) + ")");
        }
    }

    public class NavigationBindingExpression2 : Expression, IPrintable
    {
        public Expression Operand { get; }
        public ParameterExpression RootParameter { get; }
        public IEntityType EntityType { get; }
        public NavigationTreeNode2 NavigationTreeNode { get; }
        public SourceMapping2 SourceMapping { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => true;
        public override Type Type => Operand.Type;

        public override Expression Reduce()
            => Operand;

        public NavigationBindingExpression2(
            Expression operand,
            ParameterExpression rootParameter,
            NavigationTreeNode2 navigationTreeNode,
            IEntityType entityType,
            SourceMapping2 sourceMapping)
        {
            Operand = operand;
            RootParameter = rootParameter;
            NavigationTreeNode = navigationTreeNode;
            EntityType = entityType;
            SourceMapping = sourceMapping;
        }

        public void Print([NotNull] ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("BINDING(");
            expressionPrinter.Visit(RootParameter);
            expressionPrinter.StringBuilder.Append(" | ");

            // TODO: fix this
            expressionPrinter.StringBuilder.Append(string.Join(", ", NavigationTreeNode.FromMappings.First()) + ")");
        }
    }
}
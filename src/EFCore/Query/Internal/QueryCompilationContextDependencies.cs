// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public sealed class QueryCompilationContextDependencies
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] ICurrentDbContext currentContext)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(commandLogger, nameof(commandLogger));
            Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory));
            Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory));
            Check.NotNull(currentContext, nameof(currentContext));

            Model = model;
            Logger = logger;
            CommandLogger = commandLogger;
            EntityQueryModelVisitorFactory = entityQueryModelVisitorFactory;
            RequiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;
            CurrentContext = currentContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IModel Model { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IRequiresMaterializationExpressionVisitorFactory RequiresMaterializationExpressionVisitorFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ICurrentDbContext CurrentContext { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies With([NotNull] IModel model)
            => new QueryCompilationContextDependencies(
                model,
                Logger,
                CommandLogger,
                EntityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                CurrentContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => new QueryCompilationContextDependencies(
                Model,
                logger,
                CommandLogger,
                EntityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                CurrentContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
            => new QueryCompilationContextDependencies(
                Model,
                Logger,
                commandLogger,
                EntityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                CurrentContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies With([NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory)
            => new QueryCompilationContextDependencies(
                Model,
                Logger,
                CommandLogger,
                entityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                CurrentContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies With(
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory)
            => new QueryCompilationContextDependencies(
                Model,
                Logger,
                CommandLogger,
                EntityQueryModelVisitorFactory,
                requiresMaterializationExpressionVisitorFactory,
                CurrentContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContextDependencies With([NotNull] ICurrentDbContext currentContext)
            => new QueryCompilationContextDependencies(
                Model,
                Logger,
                CommandLogger,
                EntityQueryModelVisitorFactory,
                RequiresMaterializationExpressionVisitorFactory,
                currentContext);
    }
}

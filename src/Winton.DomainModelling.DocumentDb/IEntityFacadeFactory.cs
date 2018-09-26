// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     A factory interface to create an <see cref="IEntityFacade{TEntity,TEntityId}" /> for an
    ///     <see cref="Entity{TEntityId}" /> of a specified type.
    /// </summary>
    public interface IEntityFacadeFactory
    {
        /// <summary>
        ///     Create an <see cref="IEntityFacade{TEntity,TEntityId}" /> for an <see cref="Entity{TEntityId}" /> of a specified
        ///     type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection.</param>
        /// <returns>The created <see cref="IEntityFacade{TEntity,TEntityId}" />.</returns>
        IEntityFacade<TEntity, TEntityId> Create<TEntity, TEntityId>(
            Database database,
            DocumentCollection documentCollection)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;
    }
}
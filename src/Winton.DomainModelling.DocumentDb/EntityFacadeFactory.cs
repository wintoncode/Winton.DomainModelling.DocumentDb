// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     The default factory to create an <see cref="IEntityFacade{TEntity,TEntityId}" /> for an
    ///     <see cref="Entity{TEntityId}" /> of a specified type.
    /// </summary>
    public sealed class EntityFacadeFactory : IEntityFacadeFactory
    {
        private readonly IDocumentClient _documentClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityFacadeFactory" /> class.
        /// </summary>
        /// <param name="documentClient">A document client implementation.</param>
        public EntityFacadeFactory(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create an <see cref="IEntityFacade{TEntity,TEntityId}" /> for an <see cref="Entity{TEntityId}" /> of a specified
        ///     type. The default implementation allows multiple types to be transparently stored in one collection using a
        ///     'wrapper' document type with a type discriminator and namespaced ID.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection. Partitioned collections are not supported.</param>
        /// <returns>The created <see cref="IEntityFacade{TEntity,TEntityId}" />.</returns>
        public IEntityFacade<TEntity, TEntityId> Create<TEntity, TEntityId>(
            Database database,
            DocumentCollection documentCollection)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            return new EntityFacade<TEntity, TEntityId>(database, documentCollection, _documentClient);
        }
    }
}
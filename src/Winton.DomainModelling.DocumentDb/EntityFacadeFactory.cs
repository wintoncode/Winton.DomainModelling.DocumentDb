// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     The default factory to create an <see cref="IEntityFacade{TEntity,TEntityId}" /> or an
    ///     <see cref="IEntityFacade{TEntity,TEntityId,TDto}" /> for an <see cref="Entity{TEntityId}" /> of a specified type.
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
        /// <typeparam name="TEntity">The type of the Entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the Entity.</typeparam>
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

        /// <inheritdoc />
        /// <summary>
        ///     Create an <see cref="IEntityFacade{TEntity,TEntityId,TDto}" /> for an <see cref="Entity{TEntityId}" /> of a
        ///     specified type. The default implementation allows multiple types to be transparently stored in one collection using
        ///     a 'wrapper' document type with a type discriminator and namespaced ID.
        /// </summary>
        /// <typeparam name="TEntity">The type of the Entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the Entity.</typeparam>
        /// <typeparam name="TDto">The DTO type for the Entity.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection. Partitioned collections are not supported.</param>
        /// <param name="dtoMapping">A mapping function from the <see cref="Entity{TEntityId}" /> type to the DTO type.</param>
        /// <param name="entityMapping">A mapping function from the DTO type to the <see cref="Entity{TEntityId}" /> type.</param>
        /// <returns>The created <see cref="IEntityFacade{TEntity,TEntityId,TDto}" />.</returns>
        public IEntityFacade<TEntity, TEntityId, TDto> Create<TEntity, TEntityId, TDto>(
            Database database,
            DocumentCollection documentCollection,
            Func<TEntity, TDto> dtoMapping,
            Func<TDto, TEntity> entityMapping)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            return new EntityFacade<TEntity, TEntityId, TDto>(
                database,
                documentCollection,
                _documentClient,
                dtoMapping,
                entityMapping);
        }

        private sealed class EntityFacade<TEntity, TEntityId> :
            EntityFacade<TEntity, TEntityId, TEntity>,
            IEntityFacade<TEntity, TEntityId>
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            public EntityFacade(
                Database database,
                DocumentCollection documentCollection,
                IDocumentClient documentClient)
                : base(database, documentCollection, documentClient, x => x, x => x)
            {
            }
        }
    }
}
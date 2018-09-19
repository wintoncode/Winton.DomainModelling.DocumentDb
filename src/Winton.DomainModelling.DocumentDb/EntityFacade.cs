// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Winton.DomainModelling.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     An abstraction layer over <see cref="Entity{TEntityId}" /> CRUD operations in DocumentDb. Allows multiple types to
    ///     be transparently stored in one collection using a 'wrapper' document type with a type discriminator and namespaced
    ///     ID.
    /// </summary>
    public sealed class EntityFacade : IEntityFacade
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityFacade" /> class.
        /// </summary>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection. Partitioned collections are not supported.</param>
        /// <param name="documentClient">A document client implementation.</param>
        public EntityFacade(Database database, DocumentCollection documentCollection, IDocumentClient documentClient)
        {
            if (documentCollection.PartitionKey.Paths.Any())
            {
                throw new NotSupportedException("Partitioned collections not supported.");
            }

            _database = database;
            _documentCollection = documentCollection;
            _documentClient = documentClient;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create an <see cref="T:Winton.DomainModelling.Entity`1" /> of a specified type. Supports automatic ID generation
        ///     for <see cref="T:System.String" />-serializable ID types, otherwise IDs must be set before creating.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="entity">The <see cref="T:Winton.DomainModelling.Entity`1" /> to persist.</param>
        /// <returns>The created <see cref="T:Winton.DomainModelling.Entity`1" />.</returns>
        public async Task<TEntity> Create<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            var document = new EntityDocument<TEntity, TEntityId>(entity.WithId<TEntity, TEntityId>());

            ResourceResponse<Document> response = await _documentClient.CreateDocumentAsync(GetUri(), document);

            EntityDocument<TEntity, TEntityId> responseDocument = (dynamic)response.Resource;

            return responseDocument.Entity;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete an <see cref="T:Winton.DomainModelling.Entity`1" /> of a specified type by ID.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="id">The ID of the <see cref="T:Winton.DomainModelling.Entity`1" /> to delete.</param>
        /// <returns>A Task.</returns>
        public async Task Delete<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            await _documentClient.DeleteDocumentAsync(GetUri<TEntity, TEntityId>(id));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Query <see cref="T:Winton.DomainModelling.Entity`1" /> instances of a specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" />.</returns>
        public IQueryable<TEntity> Query<TEntity, TEntityId>()
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            string entityType = EntityDocument<TEntity, TEntityId>.GetDocumentType();

            return _documentClient.CreateDocumentQuery<EntityDocument<TEntity, TEntityId>>(GetUri())
                                  .Where(x => x.Type == entityType)
                                  .Select(x => x.Entity);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Read an <see cref="T:Winton.DomainModelling.Entity`1" /> of a specified type by ID.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="id">The ID of the <see cref="T:Winton.DomainModelling.Entity`1" /> to read.</param>
        /// <returns>The <see cref="T:Winton.DomainModelling.Entity`1" /> with the given ID, if it exists, otherwise null.</returns>
        public async Task<TEntity> Read<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            try
            {
                ResourceResponse<Document> response =
                    await _documentClient.ReadDocumentAsync(GetUri<TEntity, TEntityId>(id));

                EntityDocument<TEntity, TEntityId> responseDocument = (dynamic)response.Resource;

                return responseDocument.Entity;
            }
            catch (DocumentClientException dce) when (dce.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Upsert an <see cref="T:Winton.DomainModelling.Entity`1" /> of a specified type. The ID must be set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="entity">The <see cref="T:Winton.DomainModelling.Entity`1" /> to upsert.</param>
        /// <returns>The upserted <see cref="T:Winton.DomainModelling.Entity`1" />.</returns>
        public async Task<TEntity> Upsert<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            if (Equals(entity.Id, default(TEntityId)))
            {
                throw new NotSupportedException("Upserting with default ID not supported.");
            }

            var document = new EntityDocument<TEntity, TEntityId>(entity);

            ResourceResponse<Document> response = await _documentClient.UpsertDocumentAsync(GetUri(), document);

            EntityDocument<TEntity, TEntityId> responseDocument = (dynamic)response.Resource;

            return responseDocument.Entity;
        }

        private Uri GetUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_database.Id, _documentCollection.Id);
        }

        private Uri GetUri<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            return UriFactory.CreateDocumentUri(
                _database.Id,
                _documentCollection.Id,
                EntityDocument<TEntity, TEntityId>.GetDocumentId(id));
        }
    }
}
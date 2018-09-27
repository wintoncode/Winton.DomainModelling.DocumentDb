// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Winton.DomainModelling.DocumentDb
{
    internal class EntityFacade<TEntity, TEntityId, TDto> : IEntityFacade<TEntity, TEntityId, TDto>
        where TEntity : Entity<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly Func<TEntity, TDto> _dtoMapping;
        private readonly Func<TDto, TEntity> _entityMapping;

        public EntityFacade(
            Database database,
            DocumentCollection documentCollection,
            IDocumentClient documentClient,
            Func<TEntity, TDto> dtoMapping,
            Func<TDto, TEntity> entityMapping)
        {
            if (documentCollection.PartitionKey.Paths.Any())
            {
                throw new NotSupportedException("Partitioned collections are not supported.");
            }

            _database = database;
            _documentCollection = documentCollection;
            _documentClient = documentClient;
            _dtoMapping = dtoMapping;
            _entityMapping = entityMapping;
        }

        public async Task<TEntity> Create(TEntity entity)
        {
            TEntity entityWithId = entity.WithId<TEntity, TEntityId>();

            var document = new EntityDocument<TEntity, TEntityId, TDto>(entityWithId, _dtoMapping(entityWithId));

            ResourceResponse<Document> response = await _documentClient.CreateDocumentAsync(GetUri(), document);
            EntityDocument<TEntity, TEntityId, TDto> responseDocument = (dynamic)response.Resource;

            return _entityMapping(responseDocument.Dto);
        }

        public async Task Delete(TEntityId id)
        {
            await _documentClient.DeleteDocumentAsync(GetUri(id));
        }

        public IEnumerable<TEntity> Query(Expression<Func<TDto, bool>> predicate = null)
        {
            string entityType = EntityDocument<TEntity, TEntityId, TDto>.GetDocumentType();

            return _documentClient.CreateDocumentQuery<EntityDocument<TEntity, TEntityId, TDto>>(GetUri())
                                  .Where(x => x.Type == entityType)
                                  .Select(x => x.Dto)
                                  .Where(predicate ?? (x => true))
                                  .AsEnumerable()
                                  .Select(x => _entityMapping(x));
        }

        public async Task<TEntity> Read(TEntityId id)
        {
            try
            {
                ResourceResponse<Document> response = await _documentClient.ReadDocumentAsync(GetUri(id));
                EntityDocument<TEntity, TEntityId, TDto> responseDocument = (dynamic)response.Resource;

                return _entityMapping(responseDocument.Dto);
            }
            catch (DocumentClientException dce) when (dce.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<TEntity> Upsert(TEntity entity)
        {
            if (Equals(entity.Id, default(TEntityId)))
            {
                throw new NotSupportedException("Upserting with default ID is not supported.");
            }

            var document = new EntityDocument<TEntity, TEntityId, TDto>(entity, _dtoMapping(entity));

            ResourceResponse<Document> response = await _documentClient.UpsertDocumentAsync(GetUri(), document);
            EntityDocument<TEntity, TEntityId, TDto> responseDocument = (dynamic)response.Resource;

            return _entityMapping(responseDocument.Dto);
        }

        private Uri GetUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_database.Id, _documentCollection.Id);
        }

        private Uri GetUri(TEntityId id)
        {
            return UriFactory.CreateDocumentUri(
                _database.Id,
                _documentCollection.Id,
                EntityDocument<TEntity, TEntityId, TDto>.GetDocumentId(id));
        }
    }
}
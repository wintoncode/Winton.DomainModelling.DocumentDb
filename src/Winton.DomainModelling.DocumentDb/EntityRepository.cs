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
    internal class EntityRepository<T> : IEntityRepository<T>
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly string _entityType;
        private readonly Func<T, string> _idSelector;

        public EntityRepository(
            IDocumentClient documentClient,
            Database database,
            DocumentCollection documentCollection,
            string entityType,
            Func<T, string> idSelector)
        {
            if (documentCollection.PartitionKey.Paths.Any())
            {
                throw new NotSupportedException("Partitioned collections are not supported.");
            }

            _documentClient = documentClient;
            _database = database;
            _documentCollection = documentCollection;
            _idSelector = idSelector;
            _entityType = entityType;
        }

        public async Task Delete(string id)
        {
            await _documentClient.DeleteDocumentAsync(GetUri(id));
        }

        public async Task Put(T entity)
        {
            var id = _idSelector(entity);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new NotSupportedException("An id must be specified to put.");
            }

            await _documentClient.UpsertDocumentAsync(GetUri(), EntityDocument<T>.Create(id, _entityType, entity));
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>>? predicate = null)
            => _documentClient
                .CreateDocumentQuery<EntityDocument<T>>(GetUri())
                .Where(x => x.Type == _entityType)
                .Select(x => x.Entity)
                .Where(predicate ?? (x => true));

        public async Task<T> Read(string id)
        {
            try
            {
                var response = await _documentClient.ReadDocumentAsync(GetUri(id));
                EntityDocument<T> responseDocument = (dynamic)response.Resource;
                return responseDocument.Entity;
            }
            catch (DocumentClientException dce) when (dce.StatusCode == HttpStatusCode.NotFound)
            {
                return default!;
            }
        }

        private Uri GetUri() => UriFactory.CreateDocumentCollectionUri(_database.Id, _documentCollection.Id);

        private Uri GetUri(string id) => UriFactory.CreateDocumentUri(
            _database.Id,
            _documentCollection.Id,
            EntityDocument<T>.CreateId(id, _entityType));
    }
}

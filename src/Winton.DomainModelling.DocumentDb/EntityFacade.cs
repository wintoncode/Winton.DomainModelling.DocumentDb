using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Winton.DomainModelling.DocumentDb
{
    public sealed class EntityFacade : IEntityFacade
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;

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

        public async Task<TEntity> Create<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            var document = new EntityDocument<TEntity, TEntityId>(entity.WithId<TEntity, TEntityId>());

            ResourceResponse<Document> response = await _documentClient.CreateDocumentAsync(GetUri(), document);

            EntityDocument<TEntity, TEntityId> responseDocument = (dynamic)response.Resource;

            return responseDocument.Entity;
        }

        public async Task Delete<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            await _documentClient.DeleteDocumentAsync(GetUri<TEntity, TEntityId>(id));
        }

        public IQueryable<TEntity> Query<TEntity, TEntityId>()
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            string entityType = EntityDocument<TEntity, TEntityId>.GetDocumentType();

            return _documentClient.CreateDocumentQuery<EntityDocument<TEntity, TEntityId>>(GetUri())
                                  .Where(x => x.Type == entityType)
                                  .Select(x => x.Entity);
        }

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
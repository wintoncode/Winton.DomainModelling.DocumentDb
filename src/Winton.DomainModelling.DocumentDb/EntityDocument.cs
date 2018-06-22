using System;
using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class EntityDocument<TEntity, TEntityId>
        where TEntity : Entity<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        public EntityDocument(TEntity entity)
        {
            Entity = entity;
        }

        public TEntity Entity { get; }

        [JsonProperty(PropertyName = "id")]
        public string Id => GetDocumentId(Entity.Id);

        public string Type => GetDocumentType();

        public static string GetDocumentId(TEntityId id)
        {
            return $"{GetDocumentType()}_{JsonConvert.SerializeObject(id)}";
        }

        public static string GetDocumentType()
        {
            return typeof(TEntity).Name;
        }
    }
}
// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class EntityDocument<TEntity, TEntityId, TDto>
        where TEntity : Entity<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        [JsonConstructor]
        private EntityDocument(TDto entity, string id, string type)
        {
            Dto = entity;
            Id = id;
            Type = type;
        }

        [JsonProperty(PropertyName = "Entity")]
        public TDto Dto { get; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; }

        public string Type { get; }

        public static EntityDocument<TEntity, TEntityId, TDto> Create(TEntity entity, TDto dto)
        {
            return new EntityDocument<TEntity, TEntityId, TDto>(dto, GetDocumentId(entity.Id), GetDocumentType());
        }

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
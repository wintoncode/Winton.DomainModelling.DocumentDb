// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class EntityDocument<T>
    {
        [JsonConstructor]
        private EntityDocument(string id, string type, T entity)
        {
            Entity = entity;
            Id = id;
            Type = type;
        }

        public T Entity { get; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; }

        public string Type { get; }

        internal static EntityDocument<T> Create(string id, string type, T entity)
        {
            return new EntityDocument<T>(CreateId(id, type), type, entity);
        }

        internal static string CreateId(string id, string type)
        {
            return $"{type}_{id}";
        }
    }
}
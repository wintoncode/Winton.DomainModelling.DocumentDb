// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Winton.DomainModelling.DocumentDb
{
    internal static class EntityExtensions
    {
        public static TEntity WithId<TEntity, TEntityId>(this TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            if (!Equals(entity.Id, default(TEntityId)))
            {
                return entity;
            }

            JObject jObject = JObject.FromObject(entity);
            jObject[nameof(Entity<TEntityId>.Id)] = Guid.NewGuid();

            try
            {
                return jObject.ToObject<TEntity>();
            }
            catch (Exception)
            {
                throw new NotSupportedException($"Automatic ID generation for {typeof(TEntityId).Name} not supported.");
            }
        }
    }
}
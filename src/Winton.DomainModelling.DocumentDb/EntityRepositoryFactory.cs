// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class EntityRepositoryFactory : IEntityRepositoryFactory
    {
        private readonly Func<Task<IDocumentClient>> _documentClientFactory;

        public EntityRepositoryFactory(Func<Task<IDocumentClient>> documentClientFactory)
        {
            _documentClientFactory = documentClientFactory;
        }

        public async Task<IEntityRepository<T>> Create<T>(
            Database database,
            DocumentCollection documentCollection,
            string entityType,
            Func<T, string> idSelector) => new EntityRepository<T>(
                await _documentClientFactory(),
                database,
                documentCollection,
                entityType,
                idSelector);
    }
}

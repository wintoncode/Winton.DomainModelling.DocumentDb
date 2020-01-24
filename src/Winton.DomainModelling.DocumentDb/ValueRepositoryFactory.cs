// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class ValueRepositoryFactory : IValueRepositoryFactory
    {
        private readonly Func<Task<IDocumentClient>> _documentClientFactory;

        public ValueRepositoryFactory(Func<Task<IDocumentClient>> documentClientFactory)
        {
            _documentClientFactory = documentClientFactory;
        }

        public async Task<IValueRepository<T>> Create<T>(
            Database database,
            DocumentCollection documentCollection,
            string valueType)
            where T : IEquatable<T>
        {
            return new ValueRepository<T>(await _documentClientFactory(), database, documentCollection, valueType);
        }
    }
}
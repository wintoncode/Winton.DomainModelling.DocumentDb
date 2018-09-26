// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class ValueObjectFacade<TValueObject> : IValueObjectFacade<TValueObject>
        where TValueObject : IEquatable<TValueObject>
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;

        public ValueObjectFacade(
            Database database,
            DocumentCollection documentCollection,
            IDocumentClient documentClient)
        {
            if (documentCollection.PartitionKey.Paths.Any())
            {
                throw new NotSupportedException("Partitioned collections are not supported.");
            }

            _database = database;
            _documentCollection = documentCollection;
            _documentClient = documentClient;
        }

        public async Task Create(TValueObject valueObject)
        {
            ValueObjectDocument<TValueObject> document = Get(valueObject);

            if (document == null)
            {
                document = ValueObjectDocument<TValueObject>.Create(valueObject);

                await _documentClient.CreateDocumentAsync(GetUri(), document);
            }
        }

        public async Task Delete(TValueObject valueObject)
        {
            ValueObjectDocument<TValueObject> document = Get(valueObject);

            if (document != null)
            {
                await _documentClient.DeleteDocumentAsync(GetUri(document.Id));
            }
        }

        public IEnumerable<TValueObject> Query(Expression<Func<TValueObject, bool>> predicate = null)
        {
            return CreateValueObjectDocumentQuery()
                .Select(x => x.ValueObject)
                .Where(predicate ?? (x => true))
                .AsEnumerable();
        }

        private IQueryable<ValueObjectDocument<TValueObject>> CreateValueObjectDocumentQuery()
        {
            string valueObjectType = ValueObjectDocument<TValueObject>.GetDocumentType();

            return _documentClient.CreateDocumentQuery<ValueObjectDocument<TValueObject>>(GetUri())
                                  .Where(x => x.Type == valueObjectType);
        }

        private ValueObjectDocument<TValueObject> Get(TValueObject valueObject)
        {
            return CreateValueObjectDocumentQuery()
                .AsEnumerable()
                .SingleOrDefault(x => x.ValueObject.Equals(valueObject));
        }

        private Uri GetUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_database.Id, _documentCollection.Id);
        }

        private Uri GetUri(string id)
        {
            return UriFactory.CreateDocumentUri(_database.Id, _documentCollection.Id, id);
        }
    }
}
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
    internal class ValueObjectFacade<TValueObject, TDto> : IValueObjectFacade<TValueObject, TDto>
        where TValueObject : IEquatable<TValueObject>
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly Func<TValueObject, TDto> _dtoMapping;
        private readonly Func<TDto, TValueObject> _valueObjectMapping;

        public ValueObjectFacade(
            Database database,
            DocumentCollection documentCollection,
            IDocumentClient documentClient,
            Func<TValueObject, TDto> dtoMapping,
            Func<TDto, TValueObject> valueObjectMapping)
        {
            if (documentCollection.PartitionKey.Paths.Any())
            {
                throw new NotSupportedException("Partitioned collections are not supported.");
            }

            _database = database;
            _documentCollection = documentCollection;
            _documentClient = documentClient;
            _dtoMapping = dtoMapping;
            _valueObjectMapping = valueObjectMapping;
        }

        public async Task Create(TValueObject valueObject)
        {
            ValueObjectDocument<TValueObject, TDto> document = Get(valueObject);

            if (document == null)
            {
                document = new ValueObjectDocument<TValueObject, TDto>(valueObject, _dtoMapping(valueObject));

                await _documentClient.CreateDocumentAsync(GetUri(), document);
            }
        }

        public async Task Delete(TValueObject valueObject)
        {
            ValueObjectDocument<TValueObject, TDto> document = Get(valueObject);

            if (document != null)
            {
                await _documentClient.DeleteDocumentAsync(GetUri(document.Id));
            }
        }

        public IEnumerable<TValueObject> Query(Expression<Func<TDto, bool>> predicate = null)
        {
            return CreateValueObjectDocumentQuery()
                .Select(x => x.Dto)
                .Where(predicate ?? (x => true))
                .AsEnumerable()
                .Select(x => _valueObjectMapping(x));
        }

        private IQueryable<ValueObjectDocument<TValueObject, TDto>> CreateValueObjectDocumentQuery()
        {
            string valueObjectType = ValueObjectDocument<TValueObject, TDto>.GetDocumentType();

            return _documentClient.CreateDocumentQuery<ValueObjectDocument<TValueObject, TDto>>(GetUri())
                                  .Where(x => x.Type == valueObjectType);
        }

        private ValueObjectDocument<TValueObject, TDto> Get(TValueObject valueObject)
        {
            return CreateValueObjectDocumentQuery()
                .AsEnumerable()
                .SingleOrDefault(x => _valueObjectMapping(x.Dto).Equals(valueObject));
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
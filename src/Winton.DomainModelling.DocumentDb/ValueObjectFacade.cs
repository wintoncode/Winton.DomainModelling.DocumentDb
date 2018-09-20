// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Winton.DomainModelling.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     An abstraction layer over Value Object operations in DocumentDb. Allows multiple types to be transparently stored
    ///     in one collection using a 'wrapper' document type with a type discriminator.
    /// </summary>
    public sealed class ValueObjectFacade : IValueObjectFacade
    {
        private readonly Database _database;
        private readonly IDocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueObjectFacade" /> class.
        /// </summary>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection. Partitioned collections are not supported.</param>
        /// <param name="documentClient">A document client implementation.</param>
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

        /// <inheritdoc />
        /// <summary>
        ///     Create a Value Object of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <param name="valueObject">The Value Object to persist.</param>
        /// <returns>A Task.</returns>
        public async Task Create<TValueObject>(TValueObject valueObject)
            where TValueObject : IEquatable<TValueObject>
        {
            ValueObjectDocument<TValueObject> document = Get(valueObject);

            if (document == null)
            {
                document = ValueObjectDocument<TValueObject>.Create(valueObject);

                await _documentClient.CreateDocumentAsync(GetUri(), document);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete a Value Object of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <param name="valueObject">The Value Object to delete.</param>
        /// <returns>A Task.</returns>
        public async Task Delete<TValueObject>(TValueObject valueObject)
            where TValueObject : IEquatable<TValueObject>
        {
            ValueObjectDocument<TValueObject> document = Get(valueObject);

            if (document != null)
            {
                await _documentClient.DeleteDocumentAsync(GetUri(document.Id));
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Query Value Objects of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Objects.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" />.</returns>
        public IQueryable<TValueObject> Query<TValueObject>()
            where TValueObject : IEquatable<TValueObject>
        {
            return CreateValueObjectDocumentQuery<TValueObject>().Select(x => x.ValueObject);
        }

        private IQueryable<ValueObjectDocument<TValueObject>> CreateValueObjectDocumentQuery<TValueObject>()
            where TValueObject : IEquatable<TValueObject>
        {
            string valueObjectType = ValueObjectDocument<TValueObject>.GetDocumentType();

            return _documentClient.CreateDocumentQuery<ValueObjectDocument<TValueObject>>(GetUri())
                                  .Where(x => x.Type == valueObjectType);
        }

        private ValueObjectDocument<TValueObject> Get<TValueObject>(TValueObject valueObject)
            where TValueObject : IEquatable<TValueObject>
        {
            return CreateValueObjectDocumentQuery<TValueObject>()
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
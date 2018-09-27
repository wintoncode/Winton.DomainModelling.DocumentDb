// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     The default factory to create an <see cref="IValueObjectFacade{TValueObject}" /> for a Value Object of a specified
    ///     type.
    /// </summary>
    public sealed class ValueObjectFacadeFactory : IValueObjectFacadeFactory
    {
        private readonly IDocumentClient _documentClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueObjectFacadeFactory" /> class.
        /// </summary>
        /// <param name="documentClient">A document client implementation.</param>
        public ValueObjectFacadeFactory(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create an <see cref="IValueObjectFacade{TValueObject}" /> for a Value Object of a specified type. The default
        ///     implementation allows multiple types to be transparently stored in one collection using a 'wrapper' document type
        ///     with a type discriminator.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection. Partitioned collections are not supported.</param>
        /// <returns>The created <see cref="IValueObjectFacade{TValueObject}" />.</returns>
        public IValueObjectFacade<TValueObject> Create<TValueObject>(
            Database database,
            DocumentCollection documentCollection)
            where TValueObject : IEquatable<TValueObject>
        {
            return new ValueObjectFacade<TValueObject>(database, documentCollection, _documentClient);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create an <see cref="IValueObjectFacade{TValueObject,TDto}" /> for a Value Object of a specified type. The default
        ///     implementation allows multiple types to be transparently stored in one collection using a 'wrapper' document type
        ///     with a type discriminator.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <typeparam name="TDto">The DTO type for the Value Object.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection. Partitioned collections are not supported.</param>
        /// <param name="dtoMapping">A mapping function from the Value Object type to the DTO type.</param>
        /// <param name="valueObjectMapping">A mapping function from the DTO type to the Value Object type.</param>
        /// <returns>The created <see cref="IValueObjectFacade{TValueObject,TDto}" />.</returns>
        public IValueObjectFacade<TValueObject, TDto> Create<TValueObject, TDto>(
            Database database,
            DocumentCollection documentCollection,
            Func<TValueObject, TDto> dtoMapping,
            Func<TDto, TValueObject> valueObjectMapping)
            where TValueObject : IEquatable<TValueObject>
        {
            return new ValueObjectFacade<TValueObject, TDto>(
                database,
                documentCollection,
                _documentClient,
                dtoMapping,
                valueObjectMapping);
        }

        private sealed class ValueObjectFacade<TValueObject> :
            ValueObjectFacade<TValueObject, TValueObject>,
            IValueObjectFacade<TValueObject>
            where TValueObject : IEquatable<TValueObject>
        {
            public ValueObjectFacade(
                Database database,
                DocumentCollection documentCollection,
                IDocumentClient documentClient)
                : base(database, documentCollection, documentClient, x => x, x => x)
            {
            }
        }
    }
}
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
    }
}
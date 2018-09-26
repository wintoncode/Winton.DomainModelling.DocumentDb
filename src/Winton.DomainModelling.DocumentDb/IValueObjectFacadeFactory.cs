// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     A factory interface to create an <see cref="IValueObjectFacade{TValueObject}" /> for a Value Object of a specified
    ///     type.
    /// </summary>
    public interface IValueObjectFacadeFactory
    {
        /// <summary>
        ///     Create an <see cref="IValueObjectFacade{TValueObject}" /> for a Value Object of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection.</param>
        /// <returns>The created <see cref="IValueObjectFacade{TValueObject}" />.</returns>
        IValueObjectFacade<TValueObject> Create<TValueObject>(
            Database database,
            DocumentCollection documentCollection)
            where TValueObject : IEquatable<TValueObject>;
    }
}
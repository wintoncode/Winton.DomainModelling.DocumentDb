// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     A factory interface to create an <see cref="IValueObjectFacade{TValueObject}" /> or an
    ///     <see cref="IValueObjectFacade{TValueObject,TDto}" /> for a Value Object of a specified type.
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

        /// <summary>
        ///     Create an <see cref="IValueObjectFacade{TValueObject,TDto}" /> for a Value Object of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <typeparam name="TDto">The DTO type for the Value Object.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection.</param>
        /// <param name="dtoMapping">A mapping function from the Value Object type to the DTO type.</param>
        /// <param name="valueObjectMapping">A mapping function from the DTO type to the Value Object type.</param>
        /// <returns>The created <see cref="IValueObjectFacade{TValueObject,TDto}" />.</returns>
        IValueObjectFacade<TValueObject, TDto> Create<TValueObject, TDto>(
            Database database,
            DocumentCollection documentCollection,
            Func<TValueObject, TDto> dtoMapping,
            Func<TDto, TValueObject> valueObjectMapping)
            where TValueObject : IEquatable<TValueObject>;
    }
}
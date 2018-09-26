// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     An abstraction layer over Value Object operations in DocumentDb.
    /// </summary>
    /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
    public interface IValueObjectFacade<TValueObject>
        where TValueObject : IEquatable<TValueObject>
    {
        /// <summary>
        ///     Create a Value Object of a specified type.
        /// </summary>
        /// <param name="valueObject">The Value Object to persist.</param>
        /// <returns>A Task.</returns>
        Task Create(TValueObject valueObject);

        /// <summary>
        ///     Delete a Value Object of a specified type.
        /// </summary>
        /// <param name="valueObject">The Value Object to delete.</param>
        /// <returns>A Task.</returns>
        Task Delete(TValueObject valueObject);

        /// <summary>
        ///     Query Value Objects of a specified type. If a predicate expression is supplied, it will be evaluated directly by
        ///     the DocumentDb query provider (database-side), so must be supported by the LINQ to SQL API.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the results.</param>
        /// <returns>An <see cref="IEnumerable{TValueObject}" />.</returns>
        IEnumerable<TValueObject> Query(Expression<Func<TValueObject, bool>> predicate = null);
    }
}
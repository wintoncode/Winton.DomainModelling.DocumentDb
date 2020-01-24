// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     An abstraction layer over value object CRUD operations in DocumentDb.
    ///     Allows multiple values object types to be stored in the same collection.
    /// </summary>
    /// <typeparam name="T">The type to be persisted.</typeparam>
    public interface IValueRepository<T>
    {
        /// <summary>
        ///     Delete a value object of a specified type.
        /// </summary>
        /// <param name="value">The value object to delete.</param>
        /// <returns>A <see cref="Task" />.</returns>
        Task Delete(T value);

        /// <summary>
        ///     Put a value object of a specified type.
        /// </summary>
        /// <param name="value">The value object to persist.</param>
        /// <returns>A <see cref="Task" />.</returns>
        Task Put(T value);

        /// <summary>
        ///     Query value objects of a specified type.
        /// </summary>
        /// <remarks>
        ///     If a predicate expression is supplied, it will be evaluated directly by the DocumentDb query provider
        ///     (database-side), so must be supported by the LINQ to SQL API.
        /// </remarks>
        /// <param name="predicate">An optional predicate to filter the results by.</param>
        /// <returns>An <see cref="IEnumerable{T}" /> of the value objects that match the predicate.</returns>
        IEnumerable<T> Query(Expression<Func<T, bool>> predicate = null);
    }
}
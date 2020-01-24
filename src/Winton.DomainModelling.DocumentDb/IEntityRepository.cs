// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     An abstraction layer over entity CRUD operations in DocumentDb.
    ///     Allows multiple entity types to be stored in the same collection.
    /// </summary>
    /// <typeparam name="T">The type to be persisted.</typeparam>
    public interface IEntityRepository<T>
    {
        /// <summary>
        ///     Delete an entity of a specified type by id.
        /// </summary>
        /// <param name="id">The id of the entity to delete.</param>
        /// <returns>A <see cref="Task" />.</returns>
        Task Delete(string id);

        /// <summary>
        ///     Put an entity of a specified type.
        /// </summary>
        /// <remarks>
        ///     The id must be set.
        /// </remarks>
        /// <param name="entity">The entity to put.</param>
        /// <returns>A <see cref="Task" />.</returns>
        Task Put(T entity);

        /// <summary>
        ///     Query entity instances of a specified type.
        /// </summary>
        /// <remarks>
        ///     If a predicate expression is supplied, it will be evaluated directly by the DocumentDb query provider
        ///     (database-side), so must be supported by the LINQ to SQL API.
        /// </remarks>
        /// <param name="predicate">An optional predicate to filter the results by.</param>
        /// <returns>An <see cref="IEnumerable{T}" /> of the entities that match the predicate.</returns>
        IEnumerable<T> Query(Expression<Func<T, bool>> predicate = null);

        /// <summary>
        ///     Read an entity of a specified type by id.
        /// </summary>
        /// <remarks>
        ///     Returns <c>default(T)</c> if the entity is not found.
        /// </remarks>
        /// <param name="id">The id of the entity to read.</param>
        /// <returns>The entity for the given id, if it exists, otherwise <c>default(T)</c>.</returns>
        Task<T> Read(string id);
    }
}

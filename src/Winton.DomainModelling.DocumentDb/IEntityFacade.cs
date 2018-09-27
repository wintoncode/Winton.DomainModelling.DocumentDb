// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Winton.DomainModelling.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     An abstraction layer over <see cref="Entity{TEntityId}" /> CRUD operations in DocumentDb.
    /// </summary>
    /// <typeparam name="TEntity">The type of the Entity.</typeparam>
    /// <typeparam name="TEntityId">The ID type of the Entity.</typeparam>
    public interface IEntityFacade<TEntity, in TEntityId> : IEntityFacade<TEntity, TEntityId, TEntity>
        where TEntity : Entity<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
    }

    /// <summary>
    ///     An abstraction layer over <see cref="Entity{TEntityId}" /> CRUD operations in DocumentDb. Allows
    ///     <see cref="Entity{TEntityId}" /> types to be mapped to DTO types for persistence.
    /// </summary>
    /// <typeparam name="TEntity">The type of the Entity.</typeparam>
    /// <typeparam name="TEntityId">The ID type of the Entity.</typeparam>
    /// <typeparam name="TDto">The DTO type for the Entity.</typeparam>
    public interface IEntityFacade<TEntity, in TEntityId, TDto>
        where TEntity : Entity<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        /// <summary>
        ///     Create an <see cref="Entity{TEntityId}" /> of a specified type. Supports automatic ID generation
        ///     for <see cref="string" />-serializable ID types, otherwise IDs must be set before creating.
        /// </summary>
        /// <param name="entity">The <see cref="Entity{TEntityId}" /> to persist.</param>
        /// <returns>The created <see cref="Entity{TEntityId}" />.</returns>
        Task<TEntity> Create(TEntity entity);

        /// <summary>
        ///     Delete an <see cref="Entity{TEntityId}" /> of a specified type by ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Entity{TEntityId}" /> to delete.</param>
        /// <returns>A Task.</returns>
        Task Delete(TEntityId id);

        /// <summary>
        ///     Query <see cref="Entity{TEntityId}" /> instances of a specified type. If a predicate expression is supplied, it
        ///     will be evaluated directly by the DocumentDb query provider (database-side), so must be supported by the LINQ to
        ///     SQL API.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the results.</param>
        /// <returns>An <see cref="IEnumerable{TEntity}" />.</returns>
        IEnumerable<TEntity> Query(Expression<Func<TDto, bool>> predicate = null);

        /// <summary>
        ///     Read an <see cref="Entity{TEntityId}" /> of a specified type by ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Entity{TEntityId}" /> to read.</param>
        /// <returns>The <see cref="Entity{TEntityId}" /> with the given ID, if it exists, otherwise null.</returns>
        Task<TEntity> Read(TEntityId id);

        /// <summary>
        ///     Upsert an <see cref="Entity{TEntityId}" /> of a specified type. The ID must be set.
        /// </summary>
        /// <param name="entity">The <see cref="Entity{TEntityId}" /> to upsert.</param>
        /// <returns>The upserted <see cref="Entity{TEntityId}" />.</returns>
        Task<TEntity> Upsert(TEntity entity);
    }
}
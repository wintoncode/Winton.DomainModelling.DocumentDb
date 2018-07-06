// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     An abstraction layer over <see cref="Entity{TEntityId}" /> CRUD operations in DocumentDb.
    /// </summary>
    public interface IEntityFacade
    {
        /// <summary>
        ///     Create an <see cref="Entity{TEntityId}" /> of a specified type. Supports automatic ID generation for
        ///     <see cref="string" />-serializable ID types, otherwise IDs must be set before creating.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="entity">The <see cref="Entity{TEntityId}" /> to persist.</param>
        /// <returns>The created <see cref="Entity{TEntityId}" />.</returns>
        Task<TEntity> Create<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        /// <summary>
        ///     Delete an <see cref="Entity{TEntityId}" /> of a specified type by ID.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="id">The ID of the <see cref="Entity{TEntityId}" /> to delete.</param>
        /// <returns>A Task.</returns>
        Task Delete<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        /// <summary>
        ///     Query <see cref="Entity{TEntityId}" /> instances of a specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <returns>An <see cref="IQueryable{TEntity}" />.</returns>
        IQueryable<TEntity> Query<TEntity, TEntityId>()
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        /// <summary>
        ///     Read an <see cref="Entity{TEntityId}" /> of a specified type by ID.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="id">The ID of the <see cref="Entity{TEntityId}" /> to read.</param>
        /// <returns>The <see cref="Entity{TEntityId}" /> with the given ID, if it exists, otherwise null.</returns>
        Task<TEntity> Read<TEntity, TEntityId>(TEntityId id)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        /// <summary>
        ///     Upsert an <see cref="Entity{TEntityId}" /> of a specified type. The ID must be set.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the entity.</typeparam>
        /// <param name="entity">The <see cref="Entity{TEntityId}" /> to upsert.</param>
        /// <returns>The upserted <see cref="Entity{TEntityId}" />.</returns>
        Task<TEntity> Upsert<TEntity, TEntityId>(TEntity entity)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;
    }
}
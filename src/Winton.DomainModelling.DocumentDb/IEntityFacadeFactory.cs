// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     A factory interface to create an <see cref="IEntityFacade{TEntity,TEntityId}" /> or an
    ///     <see cref="IEntityFacade{TEntity,TEntityId,TDto}" /> for an <see cref="Entity{TEntityId}" /> of a specified type.
    /// </summary>
    public interface IEntityFacadeFactory
    {
        /// <summary>
        ///     Create an <see cref="IEntityFacade{TEntity,TEntityId}" /> for an <see cref="Entity{TEntityId}" /> of a specified
        ///     type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the Entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the Entity.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection.</param>
        /// <returns>The created <see cref="IEntityFacade{TEntity,TEntityId}" />.</returns>
        IEntityFacade<TEntity, TEntityId> Create<TEntity, TEntityId>(
            Database database,
            DocumentCollection documentCollection)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;

        /// <summary>
        ///     Create an <see cref="IEntityFacade{TEntity,TEntityId,TDto}" /> for an <see cref="Entity{TEntityId}" /> of a
        ///     specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the Entity.</typeparam>
        /// <typeparam name="TEntityId">The ID type of the Entity.</typeparam>
        /// <typeparam name="TDto">The DTO type for the Entity.</typeparam>
        /// <param name="database">The DocumentDb database.</param>
        /// <param name="documentCollection">The DocumentDb collection.</param>
        /// <param name="dtoMapping">A mapping function from the <see cref="Entity{TEntityId}" /> type to the DTO type.</param>
        /// <param name="entityMapping">A mapping function from the DTO type to the <see cref="Entity{TEntityId}" /> type.</param>
        /// <returns>The created <see cref="IEntityFacade{TEntity,TEntityId,TDto}" />.</returns>
        IEntityFacade<TEntity, TEntityId, TDto> Create<TEntity, TEntityId, TDto>(
            Database database,
            DocumentCollection documentCollection,
            Func<TEntity, TDto> dtoMapping,
            Func<TDto, TEntity> entityMapping)
            where TEntity : Entity<TEntityId>
            where TEntityId : IEquatable<TEntityId>;
    }
}
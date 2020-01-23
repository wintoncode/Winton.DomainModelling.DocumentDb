// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     A factory interface to create an <see cref="IEntityRepository{T}" /> for an object of a specified type.
    /// </summary>
    public interface IEntityRepositoryFactory
    {
        /// <summary>
        ///     Create an <see cref="IEntityRepository{T}" /> for an object of a specified type.
        /// </summary>
        /// <remarks>
        ///     The created <see cref="IEntityRepository{T}" /> can be used to persist any type of object providing
        ///     it can be serialised as JSON using Newtonsoft.Json.
        ///     The <paramref name="entityType" /> acts as a namespace to differentiate between objects in the
        ///     collection.
        ///     It is used to lookup the objects of type <typeparamref name="T" /> in the collection
        ///     and so would require a data migration if it was ever modified.
        ///     For that reason it is recommended to use a string literal rather than reflecting on
        ///     <typeparamref name="T" /> as the latter is not safe against refactoring.
        /// </remarks>
        /// <typeparam name="T">The type of object to be stored in the collection.</typeparam>
        /// <param name="database">The <see cref="Database" />.</param>
        /// <param name="documentCollection">The <see cref="DocumentCollection" />.</param>
        /// <param name="entityType">The type name of the entity that is being persisted.</param>
        /// <param name="idSelector">A function that selects the id from the object.</param>
        /// <returns>The created <see cref="IEntityRepository{T}" />.</returns>
        Task<IEntityRepository<T>> Create<T>(
            Database database,
            DocumentCollection documentCollection,
            string entityType,
            Func<T, string> idSelector);
    }
}

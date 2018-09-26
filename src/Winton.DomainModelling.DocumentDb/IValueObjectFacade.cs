// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
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
        ///     Query Value Objects of a specified type.
        /// </summary>
        /// <returns>An <see cref="IQueryable{TValueObject}" />.</returns>
        IQueryable<TValueObject> Query();
    }
}
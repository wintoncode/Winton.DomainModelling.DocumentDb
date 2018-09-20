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
    public interface IValueObjectFacade
    {
        /// <summary>
        ///     Create a Value Object of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <param name="valueObject">The Value Object to persist.</param>
        /// <returns>A Task.</returns>
        Task Create<TValueObject>(TValueObject valueObject)
            where TValueObject : IEquatable<TValueObject>;

        /// <summary>
        ///     Delete a Value Object of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Object.</typeparam>
        /// <param name="valueObject">The Value Object to delete.</param>
        /// <returns>A Task.</returns>
        Task Delete<TValueObject>(TValueObject valueObject)
            where TValueObject : IEquatable<TValueObject>;

        /// <summary>
        ///     Query Value Objects of a specified type.
        /// </summary>
        /// <typeparam name="TValueObject">The type of the Value Objects.</typeparam>
        /// <returns>An <see cref="IQueryable{TValueObject}" />.</returns>
        IQueryable<TValueObject> Query<TValueObject>()
            where TValueObject : IEquatable<TValueObject>;
    }
}
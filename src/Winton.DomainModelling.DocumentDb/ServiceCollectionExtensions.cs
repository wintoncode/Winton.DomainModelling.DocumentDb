// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Winton.DomainModelling.DocumentDb
{
    /// <summary>
    ///     Extension methods for adding the required services to an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers the services required and provided by this library with the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        ///     Microsoft recommend making <see cref="IDocumentClient"/> a singleton.
        ///     This library calls the <paramref name="documentClientFactory"/> to get an <see cref="IDocumentClient"/>.
        ///     It makes no attempt to ensure this is a singleton, it is the responsibility of the consuming application
        ///     to ensure this condition is met, for example by returning a <see cref="Lazy{T}"/> value.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection" /> to register the services with.</param>
        /// <param name="documentClientFactory">A function that produces an <see cref="IDocumentClient"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDomainModellingDocumentDb(
            this IServiceCollection services,
            Func<IServiceProvider, Task<IDocumentClient>> documentClientFactory)
        {
            return services
                .AddTransient<IEntityRepositoryFactory>(provider =>
                    new EntityRepositoryFactory(() => documentClientFactory(provider)))
                .AddTransient<IValueRepositoryFactory>(provider =>
                    new ValueRepositoryFactory(() => documentClientFactory(provider)));
        }
    }
}
// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityRepositoryTests
    {
        public sealed class Constructor : EntityRepositoryTests
        {
            private static Action Constructing(Func<EntityRepository<TestEntity>> ctor)
            {
                return () => ctor();
            }

            [Fact]
            private void ShouldNotThrowIfPartitionKeyIsNotSpecified()
            {
                var documentCollection = new DocumentCollection();

                Action constructing =
                    Constructing(
                        () =>
                            new EntityRepository<TestEntity>(
                                null,
                                null,
                                documentCollection,
                                "TestEntity",
                                entity => entity.Id));

                constructing.Should().NotThrow();
            }

            [Fact]
            private void ShouldThrowIfPartitionKeyIsSpecified()
            {
                var documentCollection = new DocumentCollection
                {
                    PartitionKey = new PartitionKeyDefinition { Paths = { "/id" } }
                };

                Action constructing =
                    Constructing(
                        () =>
                            new EntityRepository<TestEntity>(
                                null,
                                null,
                                documentCollection,
                                "TestEntity",
                                entity => entity.Id));

                constructing
                    .Should()
                    .Throw<NotSupportedException>()
                    .WithMessage("Partitioned collections are not supported.");
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class TestEntity
        {
            public TestEntity(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }
    }
}
// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityFacadeTests
    {
        public sealed class Constructor : EntityFacadeTests
        {
            private static Action Constructing(Func<EntityFacade<TestEntity, string>> ctor)
            {
                return () => ctor();
            }

            [Fact]
            private void ShouldNotThrowIfPartitionKeyIsNotSpecified()
            {
                var documentCollection = new DocumentCollection();

                Action constructing =
                    Constructing(() => new EntityFacade<TestEntity, string>(null, documentCollection, null));

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
                    Constructing(() => new EntityFacade<TestEntity, string>(null, documentCollection, null));

                constructing.Should().Throw<NotSupportedException>()
                            .WithMessage("Partitioned collections are not supported.");
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class TestEntity : Entity<string>
        {
            public TestEntity(string id)
                : base(id)
            {
            }
        }
    }
}
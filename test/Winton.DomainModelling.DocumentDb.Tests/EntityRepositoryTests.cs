using System;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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

                var constructing =
                    Constructing(
                        () =>
                            new EntityRepository<TestEntity>(
                                new DocumentClient(new Uri("https://example.com"), string.Empty),
                                new Database(),
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

                var constructing =
                    Constructing(
                        () =>
                            new EntityRepository<TestEntity>(
                                new DocumentClient(new Uri("https://example.com"), string.Empty),
                                new Database(),
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

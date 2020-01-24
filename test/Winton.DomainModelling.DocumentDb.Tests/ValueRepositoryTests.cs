using System;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class ValueRepositoryTests
    {
        public sealed class Constructor : ValueRepositoryTests
        {
            private static Action Constructing(Func<ValueRepository<string>> ctor)
            {
                return () => ctor();
            }

            [Fact]
            private void ShouldNotThrowIfPartitionKeyIsNotSpecified()
            {
                var documentCollection = new DocumentCollection();

                var constructing =
                    Constructing(
                        () => new ValueRepository<string>(
                            new DocumentClient(new Uri("https://example.com"), string.Empty),
                            new Database(),
                            documentCollection,
                            "ValueType"));

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
                            new ValueRepository<string>(
                                new DocumentClient(new Uri("https://example.com"), string.Empty),
                                new Database(),
                                documentCollection,
                                "ValueType"));

                constructing
                    .Should()
                    .Throw<NotSupportedException>()
                    .WithMessage("Partitioned collections are not supported.");
            }
        }
    }
}

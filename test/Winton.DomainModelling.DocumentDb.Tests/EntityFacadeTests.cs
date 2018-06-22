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
            private static Action Constructing(Func<EntityFacade> ctor)
            {
                return () => ctor();
            }

            [Fact]
            private void ShouldNotThrowIfPartitionKeyIsNotSpecified()
            {
                var documentCollection = new DocumentCollection();

                Action constructing = Constructing(() => new EntityFacade(null, documentCollection, null));

                constructing.Should().NotThrow();
            }

            [Fact]
            private void ShouldThrowIfPartitionKeyIsSpecified()
            {
                var documentCollection = new DocumentCollection
                {
                    PartitionKey = new PartitionKeyDefinition { Paths = { "/id" } }
                };

                Action constructing = Constructing(() => new EntityFacade(null, documentCollection, null));

                constructing.Should().Throw<NotSupportedException>()
                            .WithMessage("Partitioned collections not supported.");
            }
        }
    }
}
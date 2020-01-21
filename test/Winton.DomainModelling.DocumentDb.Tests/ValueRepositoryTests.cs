// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using FluentAssertions;
using Microsoft.Azure.Documents;
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

                Action constructing =
                    Constructing(
                        () => new ValueRepository<string>(null, null, documentCollection, "ValueType"));

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
                        () => new ValueRepository<string>(null, null, documentCollection, "ValueType"));

                constructing
                    .Should()
                    .Throw<NotSupportedException>()
                    .WithMessage("Partitioned collections are not supported.");
            }
        }
    }
}
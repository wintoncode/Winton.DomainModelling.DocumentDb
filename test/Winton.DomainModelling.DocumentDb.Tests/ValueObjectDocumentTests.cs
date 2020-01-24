// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class ValueObjectDocumentTests
    {
        public sealed class Id : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldDefaultToNull()
            {
                var document = ValueObjectDocument<TestValueObject>.Create("TestValueObject", new TestValueObject("A"));

                var id = document.Id;

                id.Should().BeNull();
            }

            [Fact]
            private void ShouldSerializePropertyNameAsLowercase()
            {
                typeof(ValueObjectDocument<TestValueObject>)
                    .GetProperty(nameof(ValueObjectDocument<TestValueObject>.Id))
                    .Should()
                    .BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "id");
            }
        }

        public sealed class Serialisation : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldDeserialiseFromJson()
            {
                const string json = @"{""Type"":""TestValueObject"",""ValueObject"":{""Name"":""A""}}";

                var document = JsonConvert.DeserializeObject<ValueObjectDocument<TestValueObject>>(json);

                document
                    .Should()
                    .BeEquivalentTo(
                        ValueObjectDocument<TestValueObject>.Create("TestValueObject", new TestValueObject("A")));
            }

            [Fact]
            private void ShouldSerialiseAsJson()
            {
                var document = ValueObjectDocument<TestValueObject>.Create("TestValueObject", new TestValueObject("A"));

                var serialised = JsonConvert.SerializeObject(document);

                JsonConvert.DeserializeObject<ValueObjectDocument<TestValueObject>>(serialised)
                           .Should()
                           .BeEquivalentTo(document);
            }
        }

        public class TestValueObject : IEquatable<TestValueObject>
        {
            public TestValueObject(string name)
            {
                Name = name;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public string Name { get; }

            public bool Equals(TestValueObject other) => string.Equals(Name, other?.Name);

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return obj is TestValueObject o && Equals(o);
            }

            public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        }

        public sealed class Type : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldReturnValueObjectType()
            {
                var document = ValueObjectDocument<TestValueObject>.Create("TestValueObject", new TestValueObject("A"));

                var type = document.Type;

                type.Should().Be("TestValueObject");
            }
        }

        public sealed class Value : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldReturnValueObject()
            {
                var document = ValueObjectDocument<TestValueObject>.Create("TestValueObject", new TestValueObject("A"));

                var value = document.Value;

                value.Should().Be(new TestValueObject("A"));
            }

            [Fact]
            private void ShouldSerializePropertyNameAsValueObject()
            {
                typeof(ValueObjectDocument<TestValueObject>)
                    .GetProperty(nameof(ValueObjectDocument<TestValueObject>.Value))
                    .Should()
                    .BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "ValueObject");
            }
        }
    }
}

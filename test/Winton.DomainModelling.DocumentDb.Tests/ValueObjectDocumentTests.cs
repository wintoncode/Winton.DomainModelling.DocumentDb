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
        private struct TestValueObject : IEquatable<TestValueObject>
        {
            public TestValueObject(string value)
            {
                Value = value;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public string Value { get; }

            public bool Equals(TestValueObject other)
            {
                return string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return obj is TestValueObject o && Equals(o);
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }
        }

        public sealed class GetDocumentType : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldReturnValueObjectType()
            {
                string type = ValueObjectDocument<TestValueObject>.GetDocumentType();

                type.Should().Be("TestValueObject");
            }
        }

        public sealed class Id : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldDefaultToNull()
            {
                var valueObject = new TestValueObject("A");
                ValueObjectDocument<TestValueObject> document = ValueObjectDocument<TestValueObject>.Create(valueObject);

                string id = document.Id;

                id.Should().BeNull();
            }

            [Fact]
            private void ShouldSerializePropertyNameAsLowercase()
            {
                typeof(ValueObjectDocument<TestValueObject>)
                    .GetProperty(nameof(ValueObjectDocument<TestValueObject>.Id))
                    .Should().BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "id");
            }
        }

        public sealed class Type : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldReturnValueObjectType()
            {
                var valueObject = new TestValueObject("A");
                ValueObjectDocument<TestValueObject> document = ValueObjectDocument<TestValueObject>.Create(valueObject);

                string type = document.Type;

                type.Should().Be("TestValueObject");
            }
        }

        public sealed class ValueObject : ValueObjectDocumentTests
        {
            [Fact]
            private void ShouldReturnValueObject()
            {
                var expected = new TestValueObject("A");
                ValueObjectDocument<TestValueObject> document = ValueObjectDocument<TestValueObject>.Create(expected);

                TestValueObject valueObject = document.ValueObject;

                valueObject.Should().Be(expected);
            }
        }
    }
}
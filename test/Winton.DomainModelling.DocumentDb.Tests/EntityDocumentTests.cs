// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Winton.Extensions.Serialization.Json;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityDocumentTests
    {
        [JsonConverter(typeof(SingleValueConverter))]
        private struct EntityId : IEquatable<EntityId>
        {
            private readonly int _value;

            private EntityId(int value)
            {
                _value = value;
            }

            public static explicit operator int(EntityId id)
            {
                return id._value;
            }

            public static explicit operator EntityId(int value)
            {
                return new EntityId(value);
            }

            public bool Equals(EntityId other)
            {
                return _value == other._value;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return obj is EntityId id && Equals(id);
            }

            public override int GetHashCode()
            {
                return _value;
            }
        }

        private struct TestDto
        {
            public TestDto(int id)
            {
                Id = id;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Id { get; }
        }

        public sealed class Dto : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnDto()
            {
                var expected = new TestDto(1);
                var document = new EntityDocument<TestEntity, EntityId, TestDto>(new TestEntity((EntityId)1), expected);

                TestDto dto = document.Dto;

                dto.Should().Be(expected);
            }

            [Fact]
            private void ShouldSerializePropertyNameAsEntity()
            {
                typeof(EntityDocument<TestEntity, EntityId, TestDto>)
                    .GetProperty(nameof(EntityDocument<TestEntity, EntityId, TestDto>.Dto))
                    .Should().BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "Entity");
            }
        }

        public sealed class GetDocumentId : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityTypeAndEntityId()
            {
                string id = EntityDocument<TestEntity, EntityId, TestDto>.GetDocumentId((EntityId)1);

                id.Should().Be("TestEntity_1");
            }
        }

        public sealed class GetDocumentType : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityType()
            {
                string type = EntityDocument<TestEntity, EntityId, TestDto>.GetDocumentType();

                type.Should().Be("TestEntity");
            }
        }

        public sealed class IdProperty : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityTypeAndEntityId()
            {
                var document = new EntityDocument<TestEntity, EntityId, TestDto>(
                    new TestEntity((EntityId)1),
                    new TestDto(1));

                string id = document.Id;

                id.Should().Be("TestEntity_1");
            }

            [Fact]
            private void ShouldSerializePropertyNameAsLowercase()
            {
                typeof(EntityDocument<TestEntity, EntityId, TestDto>)
                    .GetProperty(nameof(EntityDocument<TestEntity, EntityId, TestDto>.Id))
                    .Should().BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "id");
            }
        }

        public sealed class Type : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityType()
            {
                var document = new EntityDocument<TestEntity, EntityId, TestDto>(
                    new TestEntity((EntityId)1),
                    new TestDto(1));

                string type = document.Type;

                type.Should().Be("TestEntity");
            }
        }

        private sealed class TestEntity : Entity<EntityId>
        {
            public TestEntity(EntityId id)
                : base(id)
            {
            }
        }
    }
}
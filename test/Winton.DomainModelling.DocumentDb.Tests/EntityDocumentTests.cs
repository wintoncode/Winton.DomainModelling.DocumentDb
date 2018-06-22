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

        public sealed class Entity : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntity()
            {
                var expected = new TestEntity((EntityId)1);
                var document = new EntityDocument<TestEntity, EntityId>(expected);

                TestEntity entity = document.Entity;

                entity.Should().Be(expected);
            }
        }

        public sealed class GetDocumentId : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityTypeAndEntityId()
            {
                string id = EntityDocument<TestEntity, EntityId>.GetDocumentId((EntityId)1);

                id.Should().Be("TestEntity_1");
            }
        }

        public sealed class GetDocumentType : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityType()
            {
                string type = EntityDocument<TestEntity, EntityId>.GetDocumentType();

                type.Should().Be("TestEntity");
            }
        }

        public sealed class Id : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityTypeAndEntityId()
            {
                var entity = new TestEntity((EntityId)1);
                var document = new EntityDocument<TestEntity, EntityId>(entity);

                string id = document.Id;

                id.Should().Be("TestEntity_1");
            }

            [Fact]
            private void ShouldSerializePropertyNameAsLowercase()
            {
                typeof(EntityDocument<TestEntity, EntityId>)
                    .GetProperty(nameof(EntityDocument<TestEntity, EntityId>.Id))
                    .Should().BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "id");
            }
        }

        public sealed class Type : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityType()
            {
                var entity = new TestEntity((EntityId)1);
                var document = new EntityDocument<TestEntity, EntityId>(entity);

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
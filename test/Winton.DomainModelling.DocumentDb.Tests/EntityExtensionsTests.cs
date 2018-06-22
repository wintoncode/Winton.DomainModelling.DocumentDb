using System;
using FluentAssertions;
using Newtonsoft.Json;
using Winton.Extensions.Serialization.Json;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityExtensionsTests
    {
        public sealed class WithId : EntityExtensionsTests
        {
            [Fact]
            private void ShouldReturnEntityWithStringId()
            {
                var entityWithoutId = new EntityWithStringId(default(string));

                EntityWithStringId entity = entityWithoutId.WithId<EntityWithStringId, string>();

                entity.Id.Should().NotBe(default(string));
            }

            [Fact]
            private void ShouldReturnEntityWithTextualId()
            {
                var entityWithoutId = new EntityWithTextualId(default(TextualId));

                EntityWithTextualId entity = entityWithoutId.WithId<EntityWithTextualId, TextualId>();

                entity.Id.Should().NotBe(default(TextualId));
            }

            [Fact]
            private void ShouldReturnSameEntityWithIntId()
            {
                var entityWithId = new EntityWithIntId(1);

                EntityWithIntId entity = entityWithId.WithId<EntityWithIntId, int>();

                entity.Should().Be(entityWithId);
            }

            [Fact]
            private void ShouldReturnSameEntityWithNumericalId()
            {
                var entityWithId = new EntityWithNumericalId((NumericalId)1);

                EntityWithNumericalId entity = entityWithId.WithId<EntityWithNumericalId, NumericalId>();

                entity.Should().Be(entityWithId);
            }

            [Fact]
            private void ShouldReturnSameEntityWithStringId()
            {
                var entityWithId = new EntityWithStringId("A");

                EntityWithStringId entity = entityWithId.WithId<EntityWithStringId, string>();

                entity.Should().Be(entityWithId);
            }

            [Fact]
            private void ShouldReturnSameEntityWithTextualId()
            {
                var entityWithId = new EntityWithTextualId((TextualId)"A");

                EntityWithTextualId entity = entityWithId.WithId<EntityWithTextualId, TextualId>();

                entity.Should().Be(entityWithId);
            }

            [Fact]
            private void ShouldThrowForEntityWithoutIntId()
            {
                var entityWithoutId = new EntityWithIntId(default(int));

                Action action = entityWithoutId.Invoking(e => e.WithId<EntityWithIntId, int>());

                action.Should().Throw<NotSupportedException>()
                      .WithMessage("Automatic generation of Int32 ID not supported.");
            }

            [Fact]
            private void ShouldThrowForEntityWithoutNumericalId()
            {
                var entityWithoutId = new EntityWithNumericalId(default(NumericalId));

                Action action = entityWithoutId.Invoking(e => e.WithId<EntityWithNumericalId, NumericalId>());

                action.Should().Throw<NotSupportedException>()
                      .WithMessage("Automatic generation of NumericalId ID not supported.");
            }

            [JsonConverter(typeof(SingleValueConverter))]
            private struct NumericalId : IEquatable<NumericalId>
            {
                private readonly int _value;

                private NumericalId(int value)
                {
                    _value = value;
                }

                public static explicit operator int(NumericalId id)
                {
                    return id._value;
                }

                public static explicit operator NumericalId(int value)
                {
                    return new NumericalId(value);
                }

                public bool Equals(NumericalId other)
                {
                    return _value == other._value;
                }

                public override bool Equals(object obj)
                {
                    if (obj is null)
                    {
                        return false;
                    }

                    return obj is NumericalId id && Equals(id);
                }

                public override int GetHashCode()
                {
                    return _value;
                }
            }

            [JsonConverter(typeof(SingleValueConverter))]
            private struct TextualId : IEquatable<TextualId>
            {
                private readonly string _value;

                private TextualId(string value)
                {
                    _value = value;
                }

                public static explicit operator string(TextualId id)
                {
                    return id._value;
                }

                public static explicit operator TextualId(string value)
                {
                    return new TextualId(value);
                }

                public bool Equals(TextualId other)
                {
                    return string.Equals(_value, other._value);
                }

                public override bool Equals(object obj)
                {
                    if (obj is null)
                    {
                        return false;
                    }

                    return obj is TextualId id && Equals(id);
                }

                public override int GetHashCode()
                {
                    return _value?.GetHashCode() ?? 0;
                }
            }

            private sealed class EntityWithIntId : Entity<int>
            {
                public EntityWithIntId(int id)
                    : base(id)
                {
                }
            }

            private sealed class EntityWithNumericalId : Entity<NumericalId>
            {
                public EntityWithNumericalId(NumericalId id)
                    : base(id)
                {
                }
            }

            private sealed class EntityWithStringId : Entity<string>
            {
                public EntityWithStringId(string id)
                    : base(id)
                {
                }
            }

            private sealed class EntityWithTextualId : Entity<TextualId>
            {
                public EntityWithTextualId(TextualId id)
                    : base(id)
                {
                }
            }
        }
    }
}
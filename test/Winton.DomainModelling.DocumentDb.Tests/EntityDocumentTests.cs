// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityDocumentTests
    {
        public sealed class Entity : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntity()
            {
                var document = EntityDocument<TestEntity>.Create("1", "TestEntity", new TestEntity("1"));

                TestEntity entity = document.Entity;

                entity.Should().BeEquivalentTo(new TestEntity("1"));
            }
        }

        public sealed class GetDocumentId : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityTypeAndEntityId()
            {
                string id = EntityDocument<TestEntity>.CreateId("1", "TestEntity");

                id.Should().Be("TestEntity_1");
            }
        }

        public sealed class IdProperty : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityTypeAndEntityId()
            {
                var document = EntityDocument<TestEntity>.Create("1", "TestEntity", new TestEntity("1"));

                string id = document.Id;

                id.Should().Be("TestEntity_1");
            }

            [Fact]
            private void ShouldSerializePropertyNameAsLowercase()
            {
                typeof(EntityDocument<TestEntity>)
                    .GetProperty(nameof(EntityDocument<TestEntity>.Id))
                    .Should()
                    .BeDecoratedWith<JsonPropertyAttribute>(a => a.PropertyName == "id");
            }
        }

        public sealed class Serialisation : EntityDocumentTests
        {
            [Fact]
            private void ShouldDeserialiseFromJson()
            {
                const string json = @"{""Entity"":{""Id"":""1""},""id"":""TestEntity_1"",""Type"":""TestEntity""}";

                var document = JsonConvert.DeserializeObject<EntityDocument<TestEntity>>(json);

                document
                    .Should()
                    .BeEquivalentTo(EntityDocument<TestEntity>.Create("1", "TestEntity", new TestEntity("1")));
            }

            [Fact]
            private void ShouldSerialiseAsJson()
            {
                var document = EntityDocument<TestEntity>.Create("1", "TestEntity", new TestEntity("1"));

                string serialised = JsonConvert.SerializeObject(document);

                JsonConvert.DeserializeObject<EntityDocument<TestEntity>>(serialised)
                           .Should()
                           .BeEquivalentTo(document);
            }
        }

        public sealed class Type : EntityDocumentTests
        {
            [Fact]
            private void ShouldReturnEntityType()
            {
                var document = EntityDocument<TestEntity>.Create("1", "TestEntity", new TestEntity("1"));

                string type = document.Type;

                type.Should().Be("TestEntity");
            }
        }

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
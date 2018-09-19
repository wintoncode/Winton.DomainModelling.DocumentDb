// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Winton.Extensions.Serialization.Json;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityFacadeTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly EntityFacade _entityFacade;

        public EntityFacadeTests()
        {
            string documentDbUri = Environment.GetEnvironmentVariable("DOCUMENT_DB_URI");
            string documentDbKey = Environment.GetEnvironmentVariable("DOCUMENT_DB_KEY");

            var database = new Database { Id = nameof(EntityFacadeTests) };
            var documentCollection = new DocumentCollection { Id = nameof(EntityFacadeTests) };

            _documentClient = new DocumentClient(new Uri(documentDbUri), documentDbKey);
            _database = _documentClient.CreateDatabaseIfNotExistsAsync(database).Result.Resource;

            var requestOptions = new RequestOptions { OfferThroughput = 400 };
            _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                _database.SelfLink,
                documentCollection,
                requestOptions).Wait();

            _entityFacade = new EntityFacade(_database, documentCollection, _documentClient);
        }

        public void Dispose()
        {
            _documentClient.DeleteDatabaseAsync(_database.SelfLink).Wait();
        }

        [JsonConverter(typeof(SingleValueConverter))]
        private struct EntityId : IEquatable<EntityId>
        {
            private readonly string _value;

            private EntityId(string value)
            {
                _value = value;
            }

            public static explicit operator string(EntityId id)
            {
                return id._value;
            }

            public static explicit operator EntityId(string value)
            {
                return new EntityId(value);
            }

            public bool Equals(EntityId other)
            {
                return string.Equals(_value, other._value);
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
                return _value?.GetHashCode() ?? 0;
            }
        }

        public sealed class Create : EntityFacadeTests
        {
            [Fact]
            private async Task ShouldReturnCreatedEntityIfIdSet()
            {
                var entity = new TestEntity((EntityId)"A", 1);
                TestEntity createdEntity = await _entityFacade.Create<TestEntity, EntityId>(entity);

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnCreatedEntityIfIdUnset()
            {
                var entity = new TestEntity(default(EntityId), 1);
                TestEntity createdEntity = await _entityFacade.Create<TestEntity, EntityId>(entity);

                createdEntity.Should().BeEquivalentTo(
                    entity,
                    o => o.ComparingByMembers<TestEntity>().Excluding(e => e.Id));
            }

            [Fact]
            private async Task ShouldSetIdOnCreatedEntityIfUnset()
            {
                var entity = new TestEntity(default(EntityId), 1);
                TestEntity createdEntity = await _entityFacade.Create<TestEntity, EntityId>(entity);

                createdEntity.Id.Should().NotBe(default(EntityId));
            }
        }

        public sealed class Delete : EntityFacadeTests
        {
            [Fact]
            private async Task ShouldDeleteEntity()
            {
                var createdEntity = new TestEntity((EntityId)"A", 1);
                await _entityFacade.Create<TestEntity, EntityId>(createdEntity);

                await _entityFacade.Delete<TestEntity, EntityId>((EntityId)"A");

                TestEntity deletedEntity = await _entityFacade.Read<TestEntity, EntityId>((EntityId)"A");

                deletedEntity.Should().BeNull();
            }
        }

        public sealed class Query : EntityFacadeTests
        {
            [Fact]
            private async Task ShouldQueryEntitiesOfCorrectType()
            {
                var entities = new List<TestEntity>
                {
                    new TestEntity((EntityId)"C", 2),
                    new TestEntity((EntityId)"B", 3),
                    new TestEntity((EntityId)"A", 1)
                };

                var entitiesOfDifferentType = new List<OtherTestEntity>
                {
                    new OtherTestEntity((EntityId)"C", 5),
                    new OtherTestEntity((EntityId)"B", 6),
                    new OtherTestEntity((EntityId)"A", 4)
                };

                await Task.WhenAll(entities.Select(_entityFacade.Create<TestEntity, EntityId>));
                await Task.WhenAll(entitiesOfDifferentType.Select(_entityFacade.Create<OtherTestEntity, EntityId>));

                IQueryable<TestEntity> queriedEntities = _entityFacade.Query<TestEntity, EntityId>()
                                                                      .Where(e => e.Value > 1);

                queriedEntities.Should().BeEquivalentTo(
                    new List<TestEntity>
                    {
                        new TestEntity((EntityId)"C", 2),
                        new TestEntity((EntityId)"B", 3)
                    });
            }
        }

        public sealed class Read : EntityFacadeTests
        {
            [Fact]
            private async Task ShouldReturnEntityById()
            {
                var entity = new TestEntity((EntityId)"A", 1);
                await _entityFacade.Create<TestEntity, EntityId>(entity);

                var entityWithDifferentId = new TestEntity((EntityId)"B", 2);
                await _entityFacade.Create<TestEntity, EntityId>(entityWithDifferentId);

                var entityOfDifferentType = new OtherTestEntity((EntityId)"A", 3);
                await _entityFacade.Create<OtherTestEntity, EntityId>(entityOfDifferentType);

                TestEntity readEntity = await _entityFacade.Read<TestEntity, EntityId>((EntityId)"A");

                readEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnNullIfEntityWithIdNotFound()
            {
                var entityWithDifferentId = new TestEntity((EntityId)"B", 2);
                await _entityFacade.Create<TestEntity, EntityId>(entityWithDifferentId);

                var entityOfDifferentType = new OtherTestEntity((EntityId)"A", 3);
                await _entityFacade.Create<OtherTestEntity, EntityId>(entityOfDifferentType);

                TestEntity readEntity = await _entityFacade.Read<TestEntity, EntityId>((EntityId)"A");

                readEntity.Should().BeNull();
            }
        }

        public sealed class Upsert : EntityFacadeTests
        {
            [Fact]
            private async Task ShouldCreateEntity()
            {
                var entity = new TestEntity((EntityId)"A", 1);
                await _entityFacade.Upsert<TestEntity, EntityId>(entity);

                TestEntity createdEntity = await _entityFacade.Read<TestEntity, EntityId>((EntityId)"A");

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnCreatedEntity()
            {
                var entity = new TestEntity((EntityId)"A", 1);
                TestEntity createdEntity = await _entityFacade.Upsert<TestEntity, EntityId>(entity);

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnUpdatedEntity()
            {
                var createdEntity = new TestEntity((EntityId)"A", 1);
                await _entityFacade.Create<TestEntity, EntityId>(createdEntity);

                var entity = new TestEntity((EntityId)"A", 2);
                TestEntity updatedEntity = await _entityFacade.Upsert<TestEntity, EntityId>(entity);

                updatedEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private void ShouldThrowIfIdUnset()
            {
                var entity = new TestEntity(default(EntityId), 1);

                Func<Task> action = _entityFacade.Awaiting(ef => ef.Upsert<TestEntity, EntityId>(entity));

                action.Should().Throw<NotSupportedException>()
                      .WithMessage("Upserting with default ID is not supported.");
            }

            [Fact]
            private async Task ShouldUpdateEntity()
            {
                var createdEntity = new TestEntity((EntityId)"A", 1);
                await _entityFacade.Create<TestEntity, EntityId>(createdEntity);

                var entity = new TestEntity((EntityId)"A", 2);
                await _entityFacade.Upsert<TestEntity, EntityId>(entity);

                TestEntity updatedEntity = await _entityFacade.Read<TestEntity, EntityId>((EntityId)"A");

                updatedEntity.Should().BeEquivalentTo(entity);
            }
        }

        private sealed class OtherTestEntity : Entity<EntityId>
        {
            public OtherTestEntity(EntityId id, int value)
                : base(id)
            {
                Value = value;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Value { get; }
        }

        private sealed class TestEntity : Entity<EntityId>
        {
            public TestEntity(EntityId id, int value)
                : base(id)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
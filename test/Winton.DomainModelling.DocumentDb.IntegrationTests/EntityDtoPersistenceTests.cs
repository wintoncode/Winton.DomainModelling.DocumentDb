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
    [Trait("Integration", "true")]
    public class EntityDtoPersistenceTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly IEntityFacadeFactory _entityFacadeFactory;

        public EntityDtoPersistenceTests()
        {
            string documentDbUri = Environment.GetEnvironmentVariable("DOCUMENT_DB_URI");
            string documentDbKey = Environment.GetEnvironmentVariable("DOCUMENT_DB_KEY");

            var database = new Database { Id = nameof(EntityDtoPersistenceTests) };
            var documentCollection = new DocumentCollection { Id = nameof(EntityDtoPersistenceTests) };

            _documentClient = new DocumentClient(new Uri(documentDbUri), documentDbKey);
            _database = _documentClient.CreateDatabaseIfNotExistsAsync(database).Result.Resource;

            var requestOptions = new RequestOptions { OfferThroughput = 400 };
            _documentCollection = _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                _database.SelfLink,
                documentCollection,
                requestOptions).Result.Resource;

            _entityFacadeFactory = new EntityFacadeFactory(_documentClient);
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

        public sealed class Create : EntityDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldReturnCreatedEntityIfIdSet()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity((EntityId)"A", 1);
                TestEntity createdEntity = await entityFacade.Create(entity);

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnCreatedEntityIfIdUnset()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity(default(EntityId), 1);
                TestEntity createdEntity = await entityFacade.Create(entity);

                createdEntity.Should().BeEquivalentTo(
                    entity,
                    o => o.ComparingByMembers<TestEntity>().Excluding(e => e.Id));
            }

            [Fact]
            private async Task ShouldSetIdOnCreatedEntityIfUnset()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity(default(EntityId), 1);
                TestEntity createdEntity = await entityFacade.Create(entity);

                createdEntity.Id.Should().NotBe(default(EntityId));
            }
        }

        public sealed class Delete : EntityDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldDeleteEntity()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var createdEntity = new TestEntity((EntityId)"A", 1);
                await entityFacade.Create(createdEntity);

                await entityFacade.Delete((EntityId)"A");

                TestEntity deletedEntity = await entityFacade.Read((EntityId)"A");

                deletedEntity.Should().BeNull();
            }
        }

        public sealed class Query : EntityDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldQueryEntitiesOfCorrectType()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));
                IEntityFacade<OtherTestEntity, EntityId, TestDto> otherEntityFacade =
                    _entityFacadeFactory.Create<OtherTestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new OtherTestEntity((EntityId)d.Id, d.Value));

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

                await Task.WhenAll(entities.Select(entityFacade.Create));
                await Task.WhenAll(entitiesOfDifferentType.Select(otherEntityFacade.Create));

                IEnumerable<TestEntity> queriedEntities = entityFacade.Query(e => e.Value > 1);

                queriedEntities.Should().BeEquivalentTo(
                    new List<TestEntity>
                    {
                        new TestEntity((EntityId)"C", 2),
                        new TestEntity((EntityId)"B", 3)
                    });
            }
        }

        public sealed class Read : EntityDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldReturnEntityById()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));
                IEntityFacade<OtherTestEntity, EntityId, TestDto> otherEntityFacade =
                    _entityFacadeFactory.Create<OtherTestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new OtherTestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity((EntityId)"A", 1);
                await entityFacade.Create(entity);

                var entityWithDifferentId = new TestEntity((EntityId)"B", 2);
                await entityFacade.Create(entityWithDifferentId);

                var entityOfDifferentType = new OtherTestEntity((EntityId)"A", 3);
                await otherEntityFacade.Create(entityOfDifferentType);

                TestEntity readEntity = await entityFacade.Read((EntityId)"A");

                readEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnNullIfEntityWithIdNotFound()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));
                IEntityFacade<OtherTestEntity, EntityId, TestDto> otherEntityFacade =
                    _entityFacadeFactory.Create<OtherTestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new OtherTestEntity((EntityId)d.Id, d.Value));

                var entityWithDifferentId = new TestEntity((EntityId)"B", 2);
                await entityFacade.Create(entityWithDifferentId);

                var entityOfDifferentType = new OtherTestEntity((EntityId)"A", 3);
                await otherEntityFacade.Create(entityOfDifferentType);

                TestEntity readEntity = await entityFacade.Read((EntityId)"A");

                readEntity.Should().BeNull();
            }
        }

        public sealed class Upsert : EntityDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldCreateEntity()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity((EntityId)"A", 1);
                await entityFacade.Upsert(entity);

                TestEntity createdEntity = await entityFacade.Read((EntityId)"A");

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnCreatedEntity()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity((EntityId)"A", 1);
                TestEntity createdEntity = await entityFacade.Upsert(entity);

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnUpdatedEntity()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var createdEntity = new TestEntity((EntityId)"A", 1);
                await entityFacade.Create(createdEntity);

                var entity = new TestEntity((EntityId)"A", 2);
                TestEntity updatedEntity = await entityFacade.Upsert(entity);

                updatedEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private void ShouldThrowIfIdUnset()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var entity = new TestEntity(default(EntityId), 1);

                Func<Task> action = entityFacade.Awaiting(ef => ef.Upsert(entity));

                action.Should().Throw<NotSupportedException>()
                      .WithMessage("Upserting with default ID is not supported.");
            }

            [Fact]
            private async Task ShouldUpdateEntity()
            {
                IEntityFacade<TestEntity, EntityId, TestDto> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, EntityId, TestDto>(
                        _database,
                        _documentCollection,
                        e => new TestDto((string)e.Id, e.Value),
                        d => new TestEntity((EntityId)d.Id, d.Value));

                var createdEntity = new TestEntity((EntityId)"A", 1);
                await entityFacade.Create(createdEntity);

                var entity = new TestEntity((EntityId)"A", 2);
                await entityFacade.Upsert(entity);

                TestEntity updatedEntity = await entityFacade.Read((EntityId)"A");

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

            public int Value { get; }
        }

        private sealed class TestDto
        {
            public TestDto(string id, int value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; }

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
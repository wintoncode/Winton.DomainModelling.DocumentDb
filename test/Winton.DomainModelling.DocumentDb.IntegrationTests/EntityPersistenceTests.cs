using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    [Trait("Integration", "true")]
    public class EntityPersistenceTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly IEntityRepositoryFactory _entityRepositoryFactory;

        public EntityPersistenceTests()
        {
            static string GetRequiredEnvironmentVariable(string key)
            {
                return Environment.GetEnvironmentVariable(key) ?? throw new Exception($"{key} is not set");
            }

            var uri = GetRequiredEnvironmentVariable("DOCUMENT_DB_URI");
            var key = GetRequiredEnvironmentVariable("DOCUMENT_DB_KEY");

            _documentClient = new DocumentClient(new Uri(uri), key);
            _database = new Database { Id = nameof(EntityPersistenceTests) };
            _documentCollection = new DocumentCollection { Id = nameof(EntityPersistenceTests) };

            _entityRepositoryFactory = new ServiceCollection()
                .AddDomainModellingDocumentDb(
                    async _ =>
                    {
                        var databaseResponse = await _documentClient
                            .CreateDatabaseIfNotExistsAsync(_database);

                        await _documentClient
                            .CreateDocumentCollectionIfNotExistsAsync(
                                databaseResponse.Resource.SelfLink,
                                _documentCollection,
                                new RequestOptions { OfferThroughput = 400 });

                        return _documentClient;
                    })
                .BuildServiceProvider()
                .GetRequiredService<IEntityRepositoryFactory>();
        }

        public void Dispose()
        {
            Database database = _documentClient
                .CreateDatabaseIfNotExistsAsync(_database)
                .GetAwaiter()
                .GetResult();
            _documentClient.DeleteDatabaseAsync(database.SelfLink).GetAwaiter().GetResult();
        }

        public sealed class Delete : EntityPersistenceTests
        {
            [Fact]
            private async Task ShouldDeleteEntity()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);

                await entityRepository.Put(new TestEntity("A", 1));

                await entityRepository.Delete("A");

                var deletedEntity = await entityRepository.Read("A");

                deletedEntity.Should().BeNull();
            }
        }

        public sealed class Put : EntityPersistenceTests
        {
            [Fact]
            private async Task ShouldCreateEntity()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);

                var entity = new TestEntity("A", 1);
                await entityRepository.Put(entity);

                var createdEntity = await entityRepository.Read("A");

                createdEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldThrowIfIdUnset()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);

                var action = entityRepository.Awaiting(ef => ef.Put(new TestEntity(default!, 1)));

                action
                    .Should()
                    .Throw<NotSupportedException>()
                    .WithMessage("An id must be specified to put.");
            }

            [Fact]
            private async Task ShouldUpdateEntity()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);

                await entityRepository.Put(new TestEntity("A", 1));

                var entity = new TestEntity("A", 2);
                await entityRepository.Put(entity);

                var updatedEntity = await entityRepository.Read("A");

                updatedEntity.Should().BeEquivalentTo(entity);
            }
        }

        public sealed class Query : EntityPersistenceTests
        {
            [Fact]
            private async Task ShouldQueryEntitiesOfCorrectType()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);
                var otherEntityRepository =
                    await _entityRepositoryFactory.Create<OtherTestEntity>(
                        _database,
                        _documentCollection,
                        "OtherTestEntity",
                        e => e.Id);

                var entities = new List<TestEntity>
                {
                    new TestEntity("C", 2),
                    new TestEntity("B", 3),
                    new TestEntity("A", 1)
                };

                var otherEntities = new List<OtherTestEntity>
                {
                    new OtherTestEntity("C", 5),
                    new OtherTestEntity("B", 6),
                    new OtherTestEntity("A", 4)
                };

                await Task.WhenAll(entities.Select(entityRepository.Put));
                await Task.WhenAll(otherEntities.Select(otherEntityRepository.Put));

                var queriedEntities = entityRepository.Query(e => e.Value > 1);

                queriedEntities
                    .Should()
                    .BeEquivalentTo(
                        new List<TestEntity>
                        {
                            new TestEntity("C", 2),
                            new TestEntity("B", 3)
                        });
            }
        }

        public sealed class Read : EntityPersistenceTests
        {
            [Fact]
            private async Task ShouldReturnEntityById()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);
                var otherEntityRepository =
                    await _entityRepositoryFactory.Create<OtherTestEntity>(
                        _database,
                        _documentCollection,
                        "OtherTestEntity",
                        e => e.Id);

                var entity = new TestEntity("A", 1);
                await entityRepository.Put(entity);
                await entityRepository.Put(new TestEntity("B", 2));
                await otherEntityRepository.Put(new OtherTestEntity("A", 3));

                var readEntity = await entityRepository.Read("A");

                readEntity.Should().BeEquivalentTo(entity);
            }

            [Fact]
            private async Task ShouldReturnNullIfEntityWithIdNotFound()
            {
                var entityRepository =
                    await _entityRepositoryFactory.Create<TestEntity>(
                        _database,
                        _documentCollection,
                        "TestEntity",
                        e => e.Id);
                var otherEntityRepository =
                    await _entityRepositoryFactory.Create<OtherTestEntity>(
                        _database,
                        _documentCollection,
                        "OtherTestEntity",
                        e => e.Id);

                await entityRepository.Put(new TestEntity("B", 2));

                await otherEntityRepository.Put(new OtherTestEntity("A", 3));

                var readEntity = await entityRepository.Read("A");

                readEntity.Should().BeNull();
            }
        }

        private sealed class OtherTestEntity
        {
            public OtherTestEntity(string id, int value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; }

            public int Value { get; }
        }

        private sealed class TestEntity
        {
            public TestEntity(string id, int value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; }

            public int Value { get; }
        }
    }
}

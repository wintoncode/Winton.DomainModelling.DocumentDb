// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    [Trait("Integration", "true")]
    public class ValueObjectPersistenceTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly IValueRepositoryFactory _valueRepositoryFactory;

        public ValueObjectPersistenceTests()
        {
            string uri = Environment.GetEnvironmentVariable("DOCUMENT_DB_URI");
            string key = Environment.GetEnvironmentVariable("DOCUMENT_DB_KEY");

            _documentClient = new DocumentClient(new Uri(uri), key);
            _database = new Database { Id = nameof(EntityPersistenceTests) };
            _documentCollection = new DocumentCollection { Id = nameof(EntityPersistenceTests) };

            _valueRepositoryFactory = new ServiceCollection()
                .AddDomainModellingDocumentDb(
                    async _ =>
                    {
                        ResourceResponse<Database> databaseResponse = await _documentClient
                            .CreateDatabaseIfNotExistsAsync(_database);

                        await _documentClient
                            .CreateDocumentCollectionIfNotExistsAsync(
                                databaseResponse.Resource.SelfLink,
                                _documentCollection,
                                new RequestOptions { OfferThroughput = 400 });

                        return _documentClient;
                    })
                .BuildServiceProvider()
                .GetRequiredService<IValueRepositoryFactory>();
        }

        public void Dispose()
        {
            Database database = _documentClient
                .CreateDatabaseIfNotExistsAsync(_database)
                .GetAwaiter()
                .GetResult()
                .Resource;
            _documentClient.DeleteDatabaseAsync(database.SelfLink).GetAwaiter().GetResult();
        }

        public sealed class Delete : ValueObjectPersistenceTests
        {
            [Fact]
            private async Task ShouldDeleteValueObject()
            {
                IValueRepository<TestValueObject> valueRepository =
                    await _valueRepositoryFactory.Create<TestValueObject>(
                        _database,
                        _documentCollection,
                        "TestEntity");

                var valueObject = new TestValueObject(1);
                await valueRepository.Put(valueObject);

                await valueRepository.Delete(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueRepository.Query();

                queriedValueObjects.Should().BeEmpty();
            }

            [Fact]
            private async Task ShouldNotThrowIfValueObjectDoesNotExist()
            {
                IValueRepository<TestValueObject> valueRepository =
                    await _valueRepositoryFactory.Create<TestValueObject>(
                        _database,
                        _documentCollection,
                        "TestEntity");

                var valueObject = new TestValueObject(1);

                valueRepository.Awaiting(vof => vof.Delete(valueObject)).Should().NotThrow();
            }
        }

        public sealed class Put : ValueObjectPersistenceTests
        {
            [Fact]
            private async Task ShouldCreateValueObjectIfItDoesNotExist()
            {
                IValueRepository<TestValueObject> valueRepository =
                    await _valueRepositoryFactory.Create<TestValueObject>(
                        _database,
                        _documentCollection,
                        "TestEntity");

                var valueObject = new TestValueObject(1);
                await valueRepository.Put(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueRepository.Query();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }

            [Fact]
            private async Task ShouldNotCreateAnotherValueObjectIfItAlreadyExists()
            {
                IValueRepository<TestValueObject> valueRepository =
                    await _valueRepositoryFactory.Create<TestValueObject>(
                        _database,
                        _documentCollection,
                        "TestEntity");

                var valueObject = new TestValueObject(1);
                await valueRepository.Put(valueObject);
                await valueRepository.Put(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueRepository.Query();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }
        }

        public sealed class Query : ValueObjectPersistenceTests
        {
            [Fact]
            private async Task ShouldQueryValueObjectsOfCorrectType()
            {
                IValueRepository<TestValueObject> valueRepository =
                    await _valueRepositoryFactory.Create<TestValueObject>(
                        _database,
                        _documentCollection,
                        "TestEntity");
                IValueRepository<OtherTestValueObject> otherValueRepository =
                    await _valueRepositoryFactory.Create<OtherTestValueObject>(
                        _database,
                        _documentCollection,
                        "OtherTestEntity");

                var valueObjects = new List<TestValueObject>
                {
                    new TestValueObject(2),
                    new TestValueObject(3),
                    new TestValueObject(1)
                };

                var otherValueObjects = new List<OtherTestValueObject>
                {
                    new OtherTestValueObject(2),
                    new OtherTestValueObject(3),
                    new OtherTestValueObject(1)
                };

                await Task.WhenAll(valueObjects.Select(valueRepository.Put));
                await Task.WhenAll(otherValueObjects.Select(otherValueRepository.Put));

                IEnumerable<TestValueObject> queriedValueObjects = valueRepository.Query(vo => vo.Value > 1);

                queriedValueObjects
                    .Should()
                    .BeEquivalentTo(
                        new List<TestValueObject>
                        {
                            new TestValueObject(2),
                            new TestValueObject(3)
                        });
            }
        }

        private class OtherTestValueObject : IEquatable<OtherTestValueObject>
        {
            [JsonConstructor]
            public OtherTestValueObject(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public bool Equals(OtherTestValueObject other)
            {
                return Value == other?.Value;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return obj is OtherTestValueObject o && Equals(o);
            }

            public override int GetHashCode()
            {
                return Value;
            }
        }

        private class TestValueObject : IEquatable<TestValueObject>
        {
            [JsonConstructor]
            public TestValueObject(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public bool Equals(TestValueObject other)
            {
                return Value == other?.Value;
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
                return Value;
            }
        }
    }
}
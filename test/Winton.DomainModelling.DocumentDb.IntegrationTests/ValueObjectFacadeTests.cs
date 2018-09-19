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
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class ValueObjectFacadeTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly ValueObjectFacade _valueObjectFacade;

        public ValueObjectFacadeTests()
        {
            string documentDbUri = Environment.GetEnvironmentVariable("DOCUMENT_DB_URI");
            string documentDbKey = Environment.GetEnvironmentVariable("DOCUMENT_DB_KEY");

            var database = new Database { Id = nameof(ValueObjectFacadeTests) };
            var documentCollection = new DocumentCollection { Id = nameof(ValueObjectFacadeTests) };

            _documentClient = new DocumentClient(new Uri(documentDbUri), documentDbKey);
            _database = _documentClient.CreateDatabaseIfNotExistsAsync(database).Result.Resource;

            var requestOptions = new RequestOptions { OfferThroughput = 400 };
            _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                _database.SelfLink,
                documentCollection,
                requestOptions).Wait();

            _valueObjectFacade = new ValueObjectFacade(_database, documentCollection, _documentClient);
        }

        public void Dispose()
        {
            _documentClient.DeleteDatabaseAsync(_database.SelfLink).Wait();
        }

        private struct OtherTestValueObject : IEquatable<OtherTestValueObject>
        {
            [JsonConstructor]
            public OtherTestValueObject(int value)
            {
                Value = value;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public int Value { get; }

            public bool Equals(OtherTestValueObject other)
            {
                return Value == other.Value;
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

        private struct TestValueObject : IEquatable<TestValueObject>
        {
            [JsonConstructor]
            public TestValueObject(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public bool Equals(TestValueObject other)
            {
                return Value == other.Value;
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

        public sealed class Create : ValueObjectFacadeTests
        {
            [Fact]
            private async Task ShouldCreateValueObjectIfItDoesNotExist()
            {
                var valueObject = new TestValueObject(1);
                await _valueObjectFacade.Create(valueObject);

                IQueryable<TestValueObject> queriedValueObjects = _valueObjectFacade.Query<TestValueObject>();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }

            [Fact]
            private async Task ShouldNotCreateAnotherValueObjectIfItAlreadyExists()
            {
                var valueObject = new TestValueObject(1);
                await _valueObjectFacade.Create(valueObject);
                await _valueObjectFacade.Create(valueObject);

                IQueryable<TestValueObject> queriedValueObjects = _valueObjectFacade.Query<TestValueObject>();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }
        }

        public sealed class Delete : ValueObjectFacadeTests
        {
            [Fact]
            private async Task ShouldDeleteValueObject()
            {
                var valueObject = new TestValueObject(1);
                await _valueObjectFacade.Create(valueObject);

                await _valueObjectFacade.Delete(valueObject);

                IQueryable<TestValueObject> queriedValueObjects = _valueObjectFacade.Query<TestValueObject>();

                queriedValueObjects.Should().BeEmpty();
            }

            [Fact]
            private void ShouldNotThrowIfValueObjectDoesNotExist()
            {
                var valueObject = new TestValueObject(1);

                _valueObjectFacade.Awaiting(vof => vof.Delete(valueObject)).Should().NotThrow();
            }
        }

        public sealed class Query : ValueObjectFacadeTests
        {
            [Fact]
            private async Task ShouldQueryValueObjectsOfCorrectType()
            {
                var valueObjects = new List<TestValueObject>
                {
                    new TestValueObject(2),
                    new TestValueObject(3),
                    new TestValueObject(1)
                };

                var valueObjectsOfDifferentType = new List<OtherTestValueObject>
                {
                    new OtherTestValueObject(2),
                    new OtherTestValueObject(3),
                    new OtherTestValueObject(1)
                };

                await Task.WhenAll(valueObjects.Select(_valueObjectFacade.Create));
                await Task.WhenAll(valueObjectsOfDifferentType.Select(_valueObjectFacade.Create));

                IQueryable<TestValueObject> queriedValueObjects = _valueObjectFacade.Query<TestValueObject>()
                                                                                    .Where(vo => vo.Value > 1);

                queriedValueObjects.Should().BeEquivalentTo(
                    new List<TestValueObject>
                    {
                        new TestValueObject(2),
                        new TestValueObject(3)
                    });
            }
        }
    }
}
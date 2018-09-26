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
    public class ValueObjectOperationTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly IValueObjectFacadeFactory _valueObjectFacadeFactory;

        public ValueObjectOperationTests()
        {
            string documentDbUri = Environment.GetEnvironmentVariable("DOCUMENT_DB_URI");
            string documentDbKey = Environment.GetEnvironmentVariable("DOCUMENT_DB_KEY");

            var database = new Database { Id = nameof(ValueObjectOperationTests) };
            var documentCollection = new DocumentCollection { Id = nameof(ValueObjectOperationTests) };

            _documentClient = new DocumentClient(new Uri(documentDbUri), documentDbKey);
            _database = _documentClient.CreateDatabaseIfNotExistsAsync(database).Result.Resource;

            var requestOptions = new RequestOptions { OfferThroughput = 400 };
            _documentCollection = _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                _database.SelfLink,
                documentCollection,
                requestOptions).Result.Resource;

            _valueObjectFacadeFactory = new ValueObjectFacadeFactory(_documentClient);
        }

        public void Dispose()
        {
            _documentClient.DeleteDatabaseAsync(_database.SelfLink).Wait();
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

        public sealed class Create : ValueObjectOperationTests
        {
            [Fact]
            private async Task ShouldCreateValueObjectIfItDoesNotExist()
            {
                IValueObjectFacade<TestValueObject> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject>(_database, _documentCollection);

                var valueObject = new TestValueObject(1);
                await valueObjectFacade.Create(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }

            [Fact]
            private async Task ShouldNotCreateAnotherValueObjectIfItAlreadyExists()
            {
                IValueObjectFacade<TestValueObject> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject>(_database, _documentCollection);

                var valueObject = new TestValueObject(1);
                await valueObjectFacade.Create(valueObject);
                await valueObjectFacade.Create(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }
        }

        public sealed class Delete : ValueObjectOperationTests
        {
            [Fact]
            private async Task ShouldDeleteValueObject()
            {
                IValueObjectFacade<TestValueObject> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject>(_database, _documentCollection);

                var valueObject = new TestValueObject(1);
                await valueObjectFacade.Create(valueObject);

                await valueObjectFacade.Delete(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query();

                queriedValueObjects.Should().BeEmpty();
            }

            [Fact]
            private void ShouldNotThrowIfValueObjectDoesNotExist()
            {
                IValueObjectFacade<TestValueObject> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject>(_database, _documentCollection);

                var valueObject = new TestValueObject(1);

                valueObjectFacade.Awaiting(vof => vof.Delete(valueObject)).Should().NotThrow();
            }
        }

        public sealed class Query : ValueObjectOperationTests
        {
            [Fact]
            private async Task ShouldQueryValueObjectsOfCorrectType()
            {
                IValueObjectFacade<TestValueObject> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject>(_database, _documentCollection);
                IValueObjectFacade<OtherTestValueObject> otherValueObjectFacade =
                    _valueObjectFacadeFactory.Create<OtherTestValueObject>(_database, _documentCollection);

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

                await Task.WhenAll(valueObjects.Select(valueObjectFacade.Create));
                await Task.WhenAll(valueObjectsOfDifferentType.Select(otherValueObjectFacade.Create));

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query(vo => vo.Value > 1);

                queriedValueObjects.Should().BeEquivalentTo(
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

            // ReSharper disable once MemberCanBePrivate.Local
            public int Value { get; }

            public bool Equals(OtherTestValueObject other)
            {
                if (other is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((OtherTestValueObject)obj);
            }

            public override int GetHashCode()
            {
                return Value;
            }
        }
    }
}
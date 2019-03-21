// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    [Trait("Integration", "true")]
    public class ValueObjectDtoPersistenceTests : IDisposable
    {
        private readonly Database _database;
        private readonly DocumentClient _documentClient;
        private readonly DocumentCollection _documentCollection;
        private readonly IValueObjectFacadeFactory _valueObjectFacadeFactory;

        public ValueObjectDtoPersistenceTests()
        {
            string documentDbUri = Environment.GetEnvironmentVariable("DOCUMENT_DB_URI");
            string documentDbKey = Environment.GetEnvironmentVariable("DOCUMENT_DB_KEY");

            var database = new Database { Id = nameof(ValueObjectDtoPersistenceTests) };
            var documentCollection = new DocumentCollection { Id = nameof(ValueObjectDtoPersistenceTests) };

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
            public TestValueObject(int value)
            {
                Value = value;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public int Value { get; }

            public static explicit operator int(TestValueObject valueObject)
            {
                return valueObject.Value;
            }

            public static explicit operator TestValueObject(int value)
            {
                return new TestValueObject(value);
            }

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

        public sealed class Create : ValueObjectDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldCreateValueObjectIfItDoesNotExist()
            {
                IValueObjectFacade<TestValueObject, int> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject, int>(
                        _database,
                        _documentCollection,
                        vo => (int)vo,
                        d => (TestValueObject)d);

                var valueObject = new TestValueObject(1);
                await valueObjectFacade.Create(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }

            [Fact]
            private async Task ShouldNotCreateAnotherValueObjectIfItAlreadyExists()
            {
                IValueObjectFacade<TestValueObject, int> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject, int>(
                        _database,
                        _documentCollection,
                        vo => (int)vo,
                        d => (TestValueObject)d);

                var valueObject = new TestValueObject(1);
                await valueObjectFacade.Create(valueObject);
                await valueObjectFacade.Create(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query();

                queriedValueObjects.Should().BeEquivalentTo(new List<TestValueObject> { valueObject });
            }
        }

        public sealed class Delete : ValueObjectDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldDeleteValueObject()
            {
                IValueObjectFacade<TestValueObject, int> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject, int>(
                        _database,
                        _documentCollection,
                        vo => (int)vo,
                        d => (TestValueObject)d);

                var valueObject = new TestValueObject(1);
                await valueObjectFacade.Create(valueObject);

                await valueObjectFacade.Delete(valueObject);

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query();

                queriedValueObjects.Should().BeEmpty();
            }

            [Fact]
            private void ShouldNotThrowIfValueObjectDoesNotExist()
            {
                IValueObjectFacade<TestValueObject, int> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject, int>(
                        _database,
                        _documentCollection,
                        vo => (int)vo,
                        d => (TestValueObject)d);

                var valueObject = new TestValueObject(1);

                valueObjectFacade.Awaiting(vof => vof.Delete(valueObject)).Should().NotThrow();
            }
        }

        public sealed class Query : ValueObjectDtoPersistenceTests
        {
            [Fact]
            private async Task ShouldQueryValueObjectsOfCorrectType()
            {
                IValueObjectFacade<TestValueObject, int> valueObjectFacade =
                    _valueObjectFacadeFactory.Create<TestValueObject, int>(
                        _database,
                        _documentCollection,
                        vo => (int)vo,
                        d => (TestValueObject)d);
                IValueObjectFacade<OtherTestValueObject, int> otherValueObjectFacade =
                    _valueObjectFacadeFactory.Create<OtherTestValueObject, int>(
                        _database,
                        _documentCollection,
                        vo => (int)vo,
                        d => (OtherTestValueObject)d);

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

                IEnumerable<TestValueObject> queriedValueObjects = valueObjectFacade.Query(vo => vo > 1);

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
            public OtherTestValueObject(int value)
            {
                Value = value;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public int Value { get; }

            public static explicit operator int(OtherTestValueObject valueObject)
            {
                return valueObject.Value;
            }

            public static explicit operator OtherTestValueObject(int value)
            {
                return new OtherTestValueObject(value);
            }

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
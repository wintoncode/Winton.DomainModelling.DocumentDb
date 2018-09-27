// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.Azure.Documents;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class EntityFacadeFactoryTests
    {
        private readonly EntityFacadeFactory _entityFacadeFactory;

        public EntityFacadeFactoryTests()
        {
            _entityFacadeFactory = new EntityFacadeFactory(null);
        }

        public sealed class Create : EntityFacadeFactoryTests
        {
            [Fact]
            private void ShouldReturnEntityFacadeWithMapping()
            {
                IEntityFacade<TestEntity, string, string> entityFacade =
                    _entityFacadeFactory.Create<TestEntity, string, string>(
                        null,
                        new DocumentCollection(),
                        e => e.Id,
                        d => new TestEntity(d));

                entityFacade.Should().BeAssignableTo<EntityFacade<TestEntity, string, string>>();
            }

            [Fact]
            private void ShouldReturnEntityFacadeWithoutMapping()
            {
                IEntityFacade<TestEntity, string> entityFacade = _entityFacadeFactory.Create<TestEntity, string>(
                    null,
                    new DocumentCollection());

                entityFacade.Should().BeAssignableTo<EntityFacade<TestEntity, string, TestEntity>>();
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class TestEntity : Entity<string>
        {
            public TestEntity(string id)
                : base(id)
            {
            }
        }
    }
}
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
            private void ShouldReturnEntityFacade()
            {
                IEntityFacade<TestEntity, string> entityFacade = _entityFacadeFactory.Create<TestEntity, string>(
                    null,
                    new DocumentCollection());

                entityFacade.Should().BeOfType<EntityFacade<TestEntity, string>>();
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
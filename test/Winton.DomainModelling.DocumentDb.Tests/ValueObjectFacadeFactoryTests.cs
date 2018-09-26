// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.Azure.Documents;
using Xunit;

namespace Winton.DomainModelling.DocumentDb
{
    public class ValueObjectFacadeFactoryTests
    {
        private readonly ValueObjectFacadeFactory _valueObjectFacadeFactory;

        public ValueObjectFacadeFactoryTests()
        {
            _valueObjectFacadeFactory = new ValueObjectFacadeFactory(null);
        }

        public sealed class Create : ValueObjectFacadeFactoryTests
        {
            [Fact]
            private void ShouldReturnValueObjectFacade()
            {
                IValueObjectFacade<string> valueObjectFacade = _valueObjectFacadeFactory.Create<string>(
                    null,
                    new DocumentCollection());

                valueObjectFacade.Should().BeOfType<ValueObjectFacade<string>>();
            }
        }
    }
}
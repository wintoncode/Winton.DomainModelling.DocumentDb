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
            private void ShouldReturnValueObjectFacadeWithMapping()
            {
                IValueObjectFacade<string, int> valueObjectFacade = _valueObjectFacadeFactory.Create<string, int>(
                    null,
                    new DocumentCollection(),
                    int.Parse,
                    d => d.ToString());

                valueObjectFacade.Should().BeAssignableTo<ValueObjectFacade<string, int>>();
            }

            [Fact]
            private void ShouldReturnValueObjectFacadeWithoutMapping()
            {
                IValueObjectFacade<string> valueObjectFacade = _valueObjectFacadeFactory.Create<string>(
                    null,
                    new DocumentCollection());

                valueObjectFacade.Should().BeAssignableTo<ValueObjectFacade<string, string>>();
            }
        }
    }
}
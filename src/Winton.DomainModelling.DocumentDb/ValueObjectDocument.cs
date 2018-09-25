// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class ValueObjectDocument<TValueObject>
        where TValueObject : IEquatable<TValueObject>
    {
        [JsonConstructor]
        private ValueObjectDocument(TValueObject valueObject, string id)
        {
            ValueObject = valueObject;
            Id = id;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; }

        public string Type => GetDocumentType();

        public TValueObject ValueObject { get; }

        public static ValueObjectDocument<TValueObject> Create(TValueObject valueObject)
        {
            return new ValueObjectDocument<TValueObject>(valueObject, null);
        }

        public static string GetDocumentType()
        {
            return typeof(TValueObject).Name;
        }
    }
}
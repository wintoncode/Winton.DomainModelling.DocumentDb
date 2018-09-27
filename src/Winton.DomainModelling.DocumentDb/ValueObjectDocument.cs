// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class ValueObjectDocument<TValueObject, TDto>
        where TValueObject : IEquatable<TValueObject>
    {
        public ValueObjectDocument(TValueObject valueObject, TDto dto)
            : this(dto, null, GetDocumentType())
        {
        }

        [JsonConstructor]
        private ValueObjectDocument(TDto valueObject, string id, string type)
        {
            Dto = valueObject;
            Id = id;
            Type = type;
        }

        [JsonProperty(PropertyName = "ValueObject")]
        public TDto Dto { get; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; }

        public string Type { get; }

        public static string GetDocumentType()
        {
            return typeof(TValueObject).Name;
        }
    }
}
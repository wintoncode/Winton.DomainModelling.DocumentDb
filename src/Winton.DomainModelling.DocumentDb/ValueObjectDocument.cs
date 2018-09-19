// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class ValueObjectDocument<TValueObject>
        where TValueObject : struct, IEquatable<TValueObject>
    {
        public ValueObjectDocument(TValueObject valueObject)
        {
            ValueObject = valueObject;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Type => GetDocumentType();

        public TValueObject ValueObject { get; }

        public static string GetDocumentType()
        {
            return typeof(TValueObject).Name;
        }
    }
}
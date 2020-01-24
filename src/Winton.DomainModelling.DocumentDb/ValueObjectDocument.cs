// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Winton.DomainModelling.DocumentDb
{
    internal sealed class ValueObjectDocument<T>
    {
        [JsonConstructor]
        private ValueObjectDocument(string id, string type, T value)
        {
            Value = value;
            Id = id;
            Type = type;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; }

        public string Type { get; }

        [JsonProperty(PropertyName = "ValueObject")]
        public T Value { get; }

        internal static ValueObjectDocument<T> Create(string type, T value)
        {
            return new ValueObjectDocument<T>(null, type, value);
        }
    }
}
﻿// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            collection.Add(item);
        }
    }
}
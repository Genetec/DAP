// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;

public static class EnumerableExtensions
{
    public static IEnumerable<List<T>> Split<T>(this IEnumerable<T> source, int chunkSize)
    {
        var chunk = new List<T>(chunkSize);

        foreach (T item in source)
        {
            chunk.Add(item);

            if (chunk.Count == chunkSize)
            {
                yield return chunk;
                chunk = new List<T>(chunkSize);
            }
        }

        if (chunk.Count > 0)
        {
            yield return chunk;
        }
    }
}
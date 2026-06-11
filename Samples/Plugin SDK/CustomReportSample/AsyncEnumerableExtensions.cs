// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Buffers the elements of the <see cref="IAsyncEnumerable{T}"/> into chunks of a specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <see cref="IAsyncEnumerable{T}"/>.</typeparam>
    /// <param name="source">The source <see cref="IAsyncEnumerable{T}"/> to buffer.</param>
    /// <param name="bufferSize">The size of each buffer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="IReadOnlyList{T}"/> representing the buffered elements.</returns>
    public static async IAsyncEnumerable<IReadOnlyList<T>> Buffer<T>(this IAsyncEnumerable<T> source, int bufferSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        var batch = new List<T>(bufferSize);

        await foreach (T item in source.WithCancellation(cancellationToken))
        {
            batch.Add(item);

            if (batch.Count == bufferSize)
            {
                yield return batch;
                batch = new List<T>(bufferSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}

// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Buffers the elements of the <see cref="IAsyncEnumerable{T}"/> into chunks of a specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <see cref="IAsyncEnumerable{T}"/>.</typeparam>
    /// <param name="source">The source <see cref="IAsyncEnumerable{T}"/> to buffer.</param>
    /// <param name="bufferSize">The size of each buffer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="IAsyncEnumerable{T}"/> representing the buffered elements.</returns>
    public static async IAsyncEnumerable<IAsyncEnumerable<T>> Buffer<T>(this IAsyncEnumerable<T> source, int bufferSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);
        while (await enumerator.MoveNextAsync())
        {
            yield return BufferInternal();
        }

        async IAsyncEnumerable<T> BufferInternal()
        {
            for (var i = 0; i < bufferSize; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return enumerator.Current;
                if (!await enumerator.MoveNextAsync())
                    yield break;
            }
        }
    }
}
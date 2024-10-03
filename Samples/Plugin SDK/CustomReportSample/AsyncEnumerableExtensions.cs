// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

static class AsyncEnumerableExtensions
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
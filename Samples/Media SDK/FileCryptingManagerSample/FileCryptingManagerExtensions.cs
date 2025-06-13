// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.Media.Export;

namespace Genetec.Dap.CodeSamples;

public static class FileCryptingManagerExtensions
{
    /// <summary>
    /// Encrypts a file asynchronously using the specified password.
    /// </summary>
    /// <param name="manager">The FileCryptingManager instance.</param>
    /// <param name="filePath">The path of the file to encrypt.</param>
    /// <param name="password">The password to use for encryption. Must be 16, 24, or 32 ASCII characters long.</param>
    /// <param name="progress">An optional IProgress<int> to report the encryption progress.</param>
    /// <param name="token">An optional CancellationToken to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation, with a FileEncryptionResult containing the encryption outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the password is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the password is not 16, 24, or 32 ASCII characters long.</exception>
    public static async Task<FileEncryptionResult> EncryptAsync(this FileCryptingManager manager, string filePath, string password, IProgress<int> progress = default, CancellationToken token = default)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        if (Encoding.ASCII.GetByteCount(password) is not (16 or 24 or 32))
            throw new ArgumentException("Password must be 16, 24, or 32 ASCII characters long.", nameof(password));

        TaskCompletionSource<FileEncryptionResult> completion = new();

        object state = new();
        manager.ProgressUpdated += OnProgressUpdated;
        try
        {
            IAsyncResult asyncResult = manager.BeginEncryption(filePath, password, OnCompleted, state);
            using CancellationTokenRegistration register = token.Register(() => Cancel(asyncResult));
            return await completion.Task;
        }
        finally
        {
            manager.ProgressUpdated -= OnProgressUpdated;
        }

        void OnCompleted(IAsyncResult result)
        {
            try
            {
                completion.TrySetResult(manager.EndEncryption(result));
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        }

        void Cancel(IAsyncResult result)
        {
            manager.CancelEncryption(result);
            completion.TrySetCanceled();
        }

        void OnProgressUpdated(object sender, ProgressChangedEventArgs e)
        {
            if (ReferenceEquals(e.UserState, state))
            {
                progress?.Report(e.ProgressPercentage);
            }
        }
    }

    /// <summary>
    /// Decrypts a file asynchronously using the specified password.
    /// </summary>
    /// <param name="manager">The FileCryptingManager instance.</param>
    /// <param name="filePath">The path of the file to decrypt.</param>
    /// <param name="password">The password to use for decryption.</param>
    /// <param name="progress">An optional IProgress<int> to report the decryption progress.</param>
    /// <param name="token">An optional CancellationToken to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation, with a FileEncryptionResult containing the decryption outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the password is null.</exception>
    public static async Task<FileEncryptionResult> DecryptAsync(this FileCryptingManager manager, string filePath, string password, IProgress<int> progress = default, CancellationToken token = default)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        TaskCompletionSource<FileEncryptionResult> completion = new();

        object state = new();
        manager.ProgressUpdated += OnProgressUpdated;
        try
        {
            IAsyncResult asyncResult = manager.BeginDecryption(filePath, password, OnCompleted, state);
            using CancellationTokenRegistration register = token.Register(() => Cancel(asyncResult));
            return await completion.Task;
        }
        finally
        {
            manager.ProgressUpdated -= OnProgressUpdated;
        }

        void OnCompleted(IAsyncResult result)
        {
            try
            {
                completion.TrySetResult(manager.EndDecryption(result));
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        }

        void Cancel(IAsyncResult result)
        {
            manager.CancelDecryption(result);
            completion.TrySetCanceled();
        }

        void OnProgressUpdated(object sender, ProgressChangedEventArgs e)
        {
            if (ReferenceEquals(e.UserState, state))
            {
                progress?.Report(e.ProgressPercentage);
            }
        }
    }
}
// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.Diagnostics.Logging.Core;

/// <summary>
/// The local HTTP server hosted inside the plugin. It bounds request size, duration, and
/// concurrency, then hands the body to the ingestor. The listener binds only to 127.0.0.1 and is
/// intended for development, not remote or production ingestion.
/// </summary>
public sealed class HttpIngestionServer : IDisposable
{
    // Bounds the number of requests processed at once; the requests beyond the bound wait in
    // the HTTP.SYS queue instead of each spawning a task.
    private const int MaxConcurrentRequests = 32;

    // Requests larger than this are rejected before the body is read.
    private const long MaxRequestBytes = 4 * 1024 * 1024;

    // Caps how long a client may take to send headers and the body, so a slow client cannot
    // hold a processing slot indefinitely and starve the others.
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    private readonly Func<string, string, Stream, CancellationToken, Task<(HttpStatusCode Status, string Error)>> m_ingest;
    private readonly Logger m_logger;
    private readonly SemaphoreSlim m_concurrencyLimiter = new(MaxConcurrentRequests);
    private readonly CancellationTokenSource m_disposeTokenSource = new();
    private HttpListener m_listener;

    /// <param name="ingest">Handles a local request: (method, absolute path, body) to a
    /// status and optional error message.</param>
    public HttpIngestionServer(Func<string, string, Stream, CancellationToken, Task<(HttpStatusCode, string)>> ingest)
    {
        m_ingest = ingest;
        m_logger = Logger.CreateInstanceLogger(this);
    }

    /// <summary>Gets whether the server currently has an open listener.</summary>
    public bool IsListening => m_listener?.IsListening == true;

    /// <summary>
    /// Starts listening on the specified port at 127.0.0.1, stopping the previous listener first
    /// when the port changed. Throws <see cref="HttpListenerException"/> when the port cannot be bound.
    /// </summary>
    public void Start(int port)
    {
        Stop();

        var listener = new HttpListener();

        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.TimeoutManager.HeaderWait = RequestTimeout;
        listener.TimeoutManager.EntityBody = RequestTimeout;
        listener.Start();

        m_listener = listener;
        _ = AcceptRequestsAsync(listener);

        m_logger.TraceInformation($"Listening for local events at http://127.0.0.1:{port}");
    }

    /// <summary>
    /// Stops the listener. The requests already being processed complete; the pending
    /// <see cref="HttpListener.GetContextAsync"/> call is aborted.
    /// </summary>
    public void Stop()
    {
        HttpListener listener = m_listener;
        m_listener = null;

        if (listener is not null)
        {
            listener.Close();
            m_logger.TraceInformation("Stopped listening");
        }
    }

    public void Dispose()
    {
        Stop();
        m_disposeTokenSource.Cancel();

        // Wait for the in-flight handlers and the accept loop to release their slots before
        // disposing the semaphore they release into, so no handler faults on a disposed
        // semaphore. Each handler is bounded by the request timeout, so this returns promptly.
        for (int i = 0; i < MaxConcurrentRequests; i++)
        {
            m_concurrencyLimiter.Wait();
        }

        m_concurrencyLimiter.Dispose();
        m_disposeTokenSource.Dispose();
        m_logger.Dispose();
    }

    // The accept loop: waits for a processing slot, accepts the next request, and hands it off
    // so the loop can accept the next one. It ends when Stop() closes the listener.
    private async Task AcceptRequestsAsync(HttpListener listener)
    {
        while (listener.IsListening)
        {
            await m_concurrencyLimiter.WaitAsync().ConfigureAwait(false);

            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is HttpListenerException or ObjectDisposedException or InvalidOperationException)
            {
                m_concurrencyLimiter.Release();
                return; // Stop() was called; the listener is shutting down
            }

            _ = HandleRequestAsync(context, m_disposeTokenSource.Token);
        }
    }

    // Processes one request and always sends a response, releasing the processing slot at the end.
    private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            (HttpStatusCode status, string error) = await ProcessRequestAsync(context.Request, cancellationToken).ConfigureAwait(false);

            context.Response.StatusCode = (int)status;
            if (error is not null)
            {
                byte[] body = Encoding.UTF8.GetBytes(error);
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.ContentLength64 = body.Length;
                await context.Response.OutputStream.WriteAsync(body, 0, body.Length, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The server is being disposed.
        }
        catch (Exception exception)
        {
            m_logger.TraceError(exception, "Unhandled exception while processing a request");
            TrySetStatus(context, HttpStatusCode.InternalServerError);
        }
        finally
        {
            // Close the response and release the slot independently: closing throws when the
            // client already reset the connection, and that must not skip the release, or the
            // slot would leak permanently and eventually stall the accept loop.
            try
            {
                context.Response.Close();
            }
            catch (Exception exception)
            {
                m_logger.TraceError(exception, "Failed to close the response");
            }
            finally
            {
                m_concurrencyLimiter.Release();
            }
        }
    }

    // Bounds the request, then delegates to the ingestor.
    private async Task<(HttpStatusCode Status, string Error)> ProcessRequestAsync(HttpListenerRequest request, CancellationToken cancellationToken)
    {
        // Require a declared length and bound it, so the body cannot be buffered unbounded.
        // ContentLength64 is -1 for a chunked request (no Content-Length header); reject those
        // rather than read an unbounded stream into memory.
        if (request.ContentLength64 < 0)
        {
            return (HttpStatusCode.LengthRequired, "A Content-Length header is required.");
        }

        if (request.ContentLength64 > MaxRequestBytes)
        {
            return (HttpStatusCode.RequestEntityTooLarge, "The request exceeds the 4 MB limit.");
        }

        // Receive the body asynchronously so a slow upload does not block the accept loop.
        // The existing serializer then reads from memory instead of waiting on the network.
        using var body = new MemoryStream((int)request.ContentLength64);
        await request.InputStream.CopyToAsync(body, 81920, cancellationToken).ConfigureAwait(false);
        body.Position = 0;

        return await m_ingest(request.HttpMethod, request.Url.AbsolutePath, body, cancellationToken).ConfigureAwait(false);
    }

    // Sets a status code on a response that may already be closed by a client reset.
    private static void TrySetStatus(HttpListenerContext context, HttpStatusCode status)
    {
        try
        {
            context.Response.StatusCode = (int)status;
        }
        catch
        {
            // The response is no longer writable; nothing left to report to the caller
        }
    }
}

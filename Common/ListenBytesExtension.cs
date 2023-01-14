using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace TYTDLPCS.Common;

public static class ListenBytesExtension
{
    public static async IAsyncEnumerable<CommandEvent> ListenBytesAsync(
        this Command command,
        Encoding standardOutputEncoding,
        Encoding standardErrorEncoding,
        CancellationToken gracefulCancellationToken,
        [EnumeratorCancellation] CancellationToken forcefulCancellationToken)
    {
        using var channel = new Channel<CommandEvent>();

        var stdOutPipe = PipeTarget.Merge(
            command.StandardOutputPipe,
            PipeTarget.Create(async (stream, token) =>
            {
                using var reader = new AsyncBinaryReader(stream, Encoding.UTF8, false);
                var list = new List<byte>();
                while (true)
                {
                    // Console.WriteLine("aaa");
                    try
                    {
                        // Console.WriteLine("bbb");
                        list.Add(await reader.ReadByteAsync(token).ConfigureAwait(false));
                        // Console.WriteLine("ccc");
                    }
                    catch (Exception e)
                    {
                        await channel.PublishAsync(new StandardOutputBytesCommandEvent(list.ToArray()), token);//.ConfigureAwait(false);

                        break;
                    }

                    if (list.Count != 256) continue;
                    
                    // Console.WriteLine($"C: ${list.Count}");
                    
                    await channel.PublishAsync(new StandardOutputBytesCommandEvent(list.ToArray()), token);//.ConfigureAwait(false);
                    list.Clear();
                }

                // using var ms = new MemoryStream();
                // Console.WriteLine("aaa");
                // await stream.CopyToAsync(ms, token).ConfigureAwait(false);
                // channel.PublishAsync(new StandardOutputBytesCommandEvent(ms.ToArray()), token).ConfigureAwait(false);
                
                // return Task.CompletedTask;
            })
        );

        var stdErrPipe = PipeTarget.Merge(
            command.StandardErrorPipe,
            PipeTarget.ToDelegate(
                s => channel.PublishAsync(new StandardErrorCommandEvent(s), forcefulCancellationToken),
                standardErrorEncoding
            )
        );

        var commandWithPipes = command
            .WithStandardOutputPipe(stdOutPipe)
            .WithStandardErrorPipe(stdErrPipe);

        var commandTask = commandWithPipes.ExecuteAsync(forcefulCancellationToken, gracefulCancellationToken);
        yield return new StartedCommandEvent(commandTask.ProcessId);

        // Don't pass cancellation token to the continuation because we need it to
        // trigger regardless of how the task completed.
        _ = commandTask
            .Task
            .ContinueWith(_ => channel.Close(), TaskContinuationOptions.None);

        await foreach (var cmdEvent in channel.ReceiveAsync(forcefulCancellationToken).ConfigureAwait(false))
            yield return cmdEvent;

        var exitCode = await commandTask.Select(r => r.ExitCode).ConfigureAwait(false);
        yield return new ExitedCommandEvent(exitCode);
    }
    
    
// This is a very simple channel implementation used to convert push-based streams into pull-based ones.
// Back-pressure is performed using a write lock. Only one publisher may write at a time.
// Only one message is buffered and read at a time.

// Flow:
// - Write lock is released initially, read lock is not
// - Consumer waits for read lock
// - Publisher claims write lock, writes a message, releases a read lock
// - Consumer goes through, claims read lock, reads one message, releases write lock
// - Process repeats until the channel transmission is terminated

internal class Channel<T> : IDisposable where T : class
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _readLock = new(0, 1);
    private readonly TaskCompletionSource<object?> _closedTcs = new();

    private T? _lastItem;

    public async Task PublishAsync(T item, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        Debug.Assert(_lastItem is null, "Channel is overwriting the last item.");

        _lastItem = item;
        _readLock.Release();
    }

    public async IAsyncEnumerable<T> ReceiveAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (true)
        {
            var task = await Task
                .WhenAny(_readLock.WaitAsync(cancellationToken), _closedTcs.Task)
                .ConfigureAwait(false);

            // Task.WhenAny() does not throw if the underlying task was cancelled.
            // So we check it ourselves and propagate cancellation if it was requested.
            if (task.IsCanceled)
                await task.ConfigureAwait(false);

            // If the first task to complete was the closing signal, then we will need to break the loop.
            // However, WaitAsync() may have completed asynchronously at this point, so we try to
            // read from the queue one last time anyway.
            var isClosed = task == _closedTcs.Task;

            if (_lastItem is not null)
            {
                yield return _lastItem;
                _lastItem = null;

                if (!isClosed)
                    _writeLock.Release();
            }

            if (isClosed)
                yield break;
        }
    }

    public void Close() => _closedTcs.TrySetResult(null);

    public void Dispose()
    {
        Close();
        _writeLock.Dispose();
        _readLock.Dispose();
    }
}
}

public class StandardOutputBytesCommandEvent : CommandEvent
{
    public byte[] Bytes { get; }
    
    public StandardOutputBytesCommandEvent(byte[] bytes) => Bytes = bytes;
}

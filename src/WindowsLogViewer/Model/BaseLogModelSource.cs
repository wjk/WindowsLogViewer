// Copyright (c) William Kent and contributors. All rights reserved.

namespace WindowsLogViewer.Model;

/// <summary>
/// A source of log entries (either <see cref="ClassicLogModel"/> or EtwLogModel).
/// </summary>
internal abstract class BaseLogModelSource
{
    /// <summary>
    /// Starts the process of reading the log on a background thread.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the result of the asynchronous operation.
    /// </returns>
    public abstract Task PopulateAsync();

    /// <summary>
    /// Gets the entries in the log so far processed.
    /// </summary>
    public abstract IReadOnlyList<LogModelEntry> Entries { get; }

    /// <summary>
    /// Gets the name of the log.
    /// </summary>
    public abstract string LogName { get; }
}

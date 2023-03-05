// Copyright (c) William Kent and contributors. All rights reserved.

namespace WindowsLogViewer.Model;

/// <summary>
/// A source of log entries (either <see cref="ClassicLogModel"/> or EtwLogModel).
/// </summary>
internal abstract class BaseLogModelSource
{
    /// <summary>
    /// Reads the specified number of entries from the log source.
    /// </summary>
    /// <param name="entryCount">
    /// How many entries to read.
    /// </param>
    /// <returns>
    /// The entries read.
    /// </returns>
    public abstract IReadOnlyList<LogModelEntry> Read(int entryCount);

    /// <summary>
    /// Starts the process of reading the log on a background thread.
    /// </summary>
    public abstract void Populate();

    /// <summary>
    /// Gets the entries in the log so far processed.
    /// </summary>
    public abstract IReadOnlyList<LogModelEntry> Entries { get; }

    /// <summary>
    /// Gets the name of the log.
    /// </summary>
    public abstract string LogName { get; }
}

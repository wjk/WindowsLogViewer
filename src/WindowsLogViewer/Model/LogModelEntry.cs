﻿// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics.CodeAnalysis;

namespace WindowsLogViewer.Model;

/// <summary>
/// An entry in a Windows log, processed for display in the user interface.
/// </summary>
internal struct LogModelEntry : IEquatable<LogModelEntry>
{
    /// <summary>
    /// Gets or sets the type of message recorded (error, warning, and so on).
    /// </summary>
    public LogEntrySeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the numerical identifier of the type of this log entry.
    /// </summary>
    /// <remarks>
    /// In Windows, each event log entry has a unique numerical code, to assist
    /// programs that parse the system log in recognizing events of interest without
    /// having to do brittle string comparisons.
    /// </remarks>
    public long EventId { get; set; }

    /// <summary>
    /// Gets or sets the message of the log entry.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the name of the subsystem that logged this entry.
    /// Can be <see langword="null"/>.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the date and time the entry was logged.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <inheritdoc/>
    public bool Equals(LogModelEntry other)
    {
        return Severity == other.Severity && Message == other.Message && Source == other.Source && TimeStamp.Equals(other.TimeStamp);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is LogModelEntry other) return Equals(other);
        else return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int sourceHash = Source?.GetHashCode() ?? 0;
        return Severity.GetHashCode() ^ EventId.GetHashCode() ^ Message.GetHashCode() ^ sourceHash ^ TimeStamp.GetHashCode();
    }
}

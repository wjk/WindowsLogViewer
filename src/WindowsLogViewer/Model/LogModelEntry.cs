// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;

namespace WindowsLogViewer.Model;

/// <summary>
/// An entry in a Windows log, processed for display in the user interface.
/// </summary>
internal struct LogModelEntry : IEquatable<LogModelEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogModelEntry"/> struct.
    /// </summary>
    /// <param name="rawEvent">
    /// An <see cref="EventRecord"/> instance whose data will be translated into this instance.
    /// </param>
    public LogModelEntry(EventRecord rawEvent)
    {
        Source = rawEvent.ProviderName;
        EventId = rawEvent.Id;
        Message = rawEvent.FormatDescription();
        TimeStamp = rawEvent.TimeCreated;

        try
        {
            Severity = rawEvent.LevelDisplayName switch
            {
                "Information" => LogEntrySeverity.Informational,
                "Warning" => LogEntrySeverity.Warning,
                "Error" => LogEntrySeverity.Error,
                _ => LogEntrySeverity.Unknown
            };
        }
        catch
        {
            // Under certain circumstances, an otherwise-valid event log entry
            // can still not load the resources needed to describe it. The Event
            // Viewer displays an error message when this happens.
            Severity = LogEntrySeverity.Unknown;
        }
    }

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
    public DateTime? TimeStamp { get; set; }

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
        int messageHash = Message?.GetHashCode() ?? 0;
        int timeHash = TimeStamp?.GetHashCode() ?? 0;
        return Severity.GetHashCode() ^ EventId.GetHashCode() ^ messageHash ^ sourceHash ^ timeHash;
    }

    /// <summary>
    /// Gets a one-line string containing an overview of the entry.
    /// </summary>
    public string ShortTitle
    {
        get
        {
            return $"{Source} — {EventId}";
        }
    }
}

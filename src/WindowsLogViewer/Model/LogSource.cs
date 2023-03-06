// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics.Eventing.Reader;

namespace WindowsLogViewer.Model;

/// <summary>
/// A model object representing a single ETW log.
/// </summary>
internal sealed class LogSource
{
    private EventLogReader logReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSource"/> class.
    /// </summary>
    /// <param name="name">
    /// The name of the log to read.
    /// </param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor (value set in ResetReader() function)
    public LogSource(string name)
    {
        LogName = name;
        ResetReader();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor

    /// <summary>
    /// Gets the name of the log being read.
    /// </summary>
    public string LogName { get; }

    /// <summary>
    /// Reads events from the log.
    /// </summary>
    /// <param name="count">
    /// The number of events to read.
    /// </param>
    /// <returns>
    /// An array of <see cref="EventRecord"/> instances.
    /// </returns>
    public EventRecord[] Read(int count)
    {
        EventRecord[] retval = new EventRecord[count];

        for (int i = 0; i < count; i++)
        {
            EventRecord? record = logReader.ReadEvent();
            if (record == null) break;

            retval[i] = record;
        }

        return retval;
    }

    /// <summary>
    /// Resets the log source such that <see cref="Read(int)"/> reads from the start of the log.
    /// </summary>
    public void ResetReader()
    {
        EventLogQuery query = new EventLogQuery(LogName, PathType.LogName);
        query.ReverseDirection = true; // newest events first
        logReader = new EventLogReader(query);
    }
}

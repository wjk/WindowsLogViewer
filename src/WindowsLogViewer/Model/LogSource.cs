// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace WindowsLogViewer.Model;

/// <summary>
/// A model object representing a single ETW log.
/// </summary>
internal sealed class LogSource
{
    /// <summary>
    /// Gets a list of the names of all the ETW logs in the system, sorted appropriately for display to the user.
    /// This is calculated once and then cached.
    /// </summary>
    public static IReadOnlyList<LogSource> AllSources => new Lazy<IReadOnlyList<LogSource>>(() =>
    {
        List<LogSource> retval = new List<LogSource>();

        HashSet<string> seenSources = new HashSet<string>();
        retval.Add(new LogSource("Application"));
        seenSources.Add("Application");

        try
        {
            retval.Add(new LogSource("Security"));
        }
        catch (UnauthorizedAccessException)
        {
            // Access to this log requires administrator privileges. If the
            // app is not running elevated, just skip this log.
        }

        seenSources.Add("Security");

        retval.Add(new LogSource("Setup"));
        seenSources.Add("Setup");
        retval.Add(new LogSource("System"));
        seenSources.Add("System");

        EventLogSession session = EventLogSession.GlobalSession;
        var names = session.GetLogNames().ToList();
        names.Sort();

        foreach (string logName in names)
        {
            if (seenSources.Contains(logName)) continue;

            try
            {
                Debug.WriteLine($"Starting to process log {logName}");
                EventLogConfiguration config = new EventLogConfiguration(logName, session);

                // Skip analytical and debug logs, just as the built-in Event Viewer does.
                if (config.LogType == EventLogType.Analytical || config.LogType == EventLogType.Debug)
                {
                    continue;
                }

                retval.Add(new LogSource(logName));
                Debug.WriteLine($"Successfully processed log {logName}");
            }
            catch
            {
                // Some logs require elevation to read. Skip those.
            }
        }

        return retval;
    }).Value;

    private EventLogReader logReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSource"/> class.
    /// </summary>
    /// <param name="name">
    /// The name of the log to read.
    /// </param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor (set in ResetReader() method)
    public LogSource(string name)
    {
        LogName = name;
        ResetReader();
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    }

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

﻿// Copyright (c) William Kent and contributors. All rights reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace WindowsLogViewer.Model;

/// <summary>
/// The top-level model object of the application.
/// </summary>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:Field names should not begin with underscore", Justification = "Used to prevent collision with public properties")]
internal sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Dispatcher dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="mainThreadDispatcher">
    /// A dispatcher that will be used to serialize all access to the <see cref="Sources"/> property.
    /// </param>
    public MainWindowViewModel(Dispatcher mainThreadDispatcher)
    {
        this.dispatcher = mainThreadDispatcher;
    }

    /// <summary>
    /// Adds entries to the <see cref="Sources"/> property.
    /// This is a very slow operation and should be called on a background thread.
    /// </summary>
    public void PopulateSources()
    {
        EventLogSession session = EventLogSession.GlobalSession;
        HashSet<string> seenSources = new HashSet<string>()
        {
            "Application", "Security", "Setup", "System",
        };

        void AddSource(string sourceName)
        {
            LogSource source;

            try
            {
                source = new LogSource(sourceName);
            }
            catch
            {
                // new LogSource() can throw if the user does not have permission to access that log.
                return;
            }

            dispatcher.Invoke(() => Sources.Add(source));
        }

        AddSource("Application");
        AddSource("Security");
        AddSource("Setup");
        AddSource("System");

        var logNames = session.GetProviderNames().ToList();
        logNames.Sort();

        foreach (string logName in logNames)
        {
            if (seenSources.Contains(logName))
                continue;

            try
            {
                EventLogConfiguration configuration = new EventLogConfiguration(logName);

                // Skip analytical and debug logs, just as the built-in Event Viewer does.
                if (configuration.LogType == EventLogType.Analytical || configuration.LogType == EventLogType.Debug)
                    continue;

                AddSource(logName);
            }
            catch
            {
                // Some logs require elevation to access. Ignore those.
            }
        }
    }

    /// <summary>
    /// Gets or sets a collection of the log sources that are available.
    /// </summary>
    public ObservableCollection<LogSource> Sources { get; set; } = new ObservableCollection<LogSource>();

    private LogSource? _activeSource;

    /// <summary>
    /// Gets or sets the source currently being displayed to the user.
    /// </summary>
    public LogSource? ActiveSource
    {
        get => _activeSource;
        set => SetProperty(ref _activeSource, value);
    }

    /// <summary>
    /// Reads twenty entries from the <see cref="ActiveSource"/> into the <see cref="DisplayedEvents"/>.
    /// Does nothing if <see cref="ActiveSource"/> is <see langword="null"/>.
    /// </summary>
    public void ReadEvents()
    {
        if (ActiveSource == null) return;
        foreach (var rawEvent in ActiveSource.Read(20))
        {
            DisplayedEvents.Add(new LogModelEntry(rawEvent));
        }
    }

    private ObservableCollection<LogModelEntry> _displayedEvents = new ObservableCollection<LogModelEntry>();

    /// <summary>
    /// Gets or sets the list of <see cref="EventRecord"/> instances currently being displayed to the user.
    /// </summary>
    public ObservableCollection<LogModelEntry> DisplayedEvents
    {
        get => _displayedEvents;
        set => SetProperty(ref _displayedEvents, value);
    }

    private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
}
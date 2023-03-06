// Copyright (c) William Kent and contributors. All rights reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        dispatcher = mainThreadDispatcher;
    }

    /// <summary>
    /// Adds entries to the <see cref="Sources"/> property.
    /// This is a very slow operation and should be called on a background thread.
    /// </summary>
    public void PopulateSources()
    {
        Sources.Clear();
        Sources.AddRange(LogSource.AllSources);
        dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sources))));
    }

    /// <summary>
    /// Gets or sets a collection of the log sources that are available.
    /// </summary>
    public List<LogSource> Sources { get; set; } = new List<LogSource>();

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
            if (rawEvent != null)
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

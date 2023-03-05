// Copyright (c) William Kent and contributors. All rights reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using WindowsLogViewer.Model;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private sealed class DataHolder : INotifyPropertyChanged
        {
            public ObservableCollection<BaseLogModelSource> Sources { get; } = new ObservableCollection<BaseLogModelSource>();

            public BaseLogModelSource? CurrentSource { get; set; }

            public List<LogModelEntry> Entries { get; } = new List<LogModelEntry>();

            public void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataHolder holder = new DataHolder();
            var sources = holder.Sources;

            sources.Add(ClassicLogModel.ApplicationLog);
            sources.Add(ClassicLogModel.SecurityLog);
            sources.Add(ClassicLogModel.SetupLog);
            sources.Add(ClassicLogModel.SystemLog);

            DataContext = holder;

            Task.Run(() =>
            {
                foreach (var source in EtwLogModel.AllLogs)
                    Dispatcher.InvokeAsync(() => sources.Add(source));
            });
        }

        private void LogChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataHolder holder = (DataHolder)DataContext;
            holder.CurrentSource = (BaseLogModelSource?)LogChooser.SelectedItem;
            holder.OnPropertyChanged(nameof(holder.CurrentSource));

            if (holder.CurrentSource != null)
            {
                holder.Entries.Clear();
                holder.Entries.AddRange(holder.CurrentSource.Read(20));
                holder.OnPropertyChanged(nameof(holder.Entries));
            }
            else
            {
                EventList.SetCurrentValue(ItemsControl.ItemsSourceProperty, null);
            }
        }
    }
}

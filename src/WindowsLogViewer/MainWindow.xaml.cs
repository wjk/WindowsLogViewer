// Copyright (c) William Kent and contributors. All rights reserved.

using System.Collections.ObjectModel;
using System.Windows;

using WindowsLogViewer.Model;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<BaseLogModelSource> sources = new ObservableCollection<BaseLogModelSource>
            {
                ClassicLogModel.ApplicationLog,
                ClassicLogModel.SecurityLog,
                ClassicLogModel.SetupLog,
                ClassicLogModel.SystemLog,
            };

            DataContext = sources;

            Task.Run(() =>
            {
                foreach (var source in EtwLogModel.AllLogs)
                    Dispatcher.InvokeAsync(() => sources.Add(source));
            });
        }
    }
}

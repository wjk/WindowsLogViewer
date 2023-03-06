// Copyright (c) William Kent and contributors. All rights reserved.

using System.Collections.Immutable;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            viewModel.ActiveSource = (LogSource)LogChooser.SelectedItem;
            viewModel.DisplayedEvents.Clear();
            viewModel.ReadEvents();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel viewModel = new MainWindowViewModel(Dispatcher);
            DataContext = viewModel;

            Task task = Task.Run(viewModel.PopulateSources);
            task.GetAwaiter().OnCompleted(() => LoadingProgressSpinner.SetCurrentValue(VisibilityProperty, Visibility.Collapsed));
        }
    }
}

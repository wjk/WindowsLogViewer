// Copyright (c) William Kent and contributors. All rights reserved.

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
            task.GetAwaiter().OnCompleted(() =>
            {
                // This is run on the UI thread.
                LoadingLabel.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                LogChooser.SetCurrentValue(VisibilityProperty, Visibility.Visible);
                LogChooser.SetCurrentValue(IsEnabledProperty, true);

                // This line populates the list box with the contents of the first log in the list
                // at the time the log list combo box is enabled.
                LogChooser.SetCurrentValue(ComboBox.SelectedItemProperty, LogChooser.Items[0]);
            });
        }

        private void MainScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            if (viewModel == null) return;

            if (e.VerticalChange > 0)
            {
                if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
                {
                    viewModel.ReadEvents();
                }
            }
        }
    }
}

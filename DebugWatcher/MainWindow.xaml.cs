using System;
using System.ComponentModel;
using System.Windows;
using DebugWatcher.ViewModels;
using Microsoft.Expression.Interactivity.Core;

namespace DebugWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AppBootstrapper AppBootstrapper { get; protected set; }
        public MainWindow()
        {
            InitializeComponent();

            AppBootstrapper = new AppBootstrapper();
            DataContext = AppBootstrapper;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ShowOptionsStateButton(object sender, RoutedEventArgs e)
        {
            ExtendedVisualStateManager.GoToElementState(this.MainContentGrid as FrameworkElement, "ShowOptions", true);
        }

        private void UseStagingServerAddressButton_OnClick(object sender, RoutedEventArgs e)
        {
            DebugWatcher.Properties.Settings.Default.RedisAddress = "137.135.99.146";
        }

        private void UseProductionServerAddressButton_OnClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RedisAddress = "10.0.1.60";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}

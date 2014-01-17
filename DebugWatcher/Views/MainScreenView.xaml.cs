﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DebugWatcher.ViewModels;
using ReactiveUI;

namespace DebugWatcher
{
    /// <summary>
    /// Interaction logic for MainScreenView.xaml
    /// </summary>
    public partial class MainScreenView : IViewFor<IMainScreenViewModel>
    {
        public MainScreenView()
        {
            InitializeComponent();
            this.OneWayBind(ViewModel, x => x.RequestOutputMessages, x => x.RequestsOutputBox.ItemsSource);
            this.OneWayBind(ViewModel, x => x.DebugOutputMessages, x => x.DebugOutputBox.ItemsSource);
            this.OneWayBind(ViewModel, x => x.ExceptionOutputMessages, x => x.ExceptionsOutputBox.ItemsSource);
            //this.Bind(ViewModel, x => x.ExceptionForeground, x => x.ExceptionsHeader.Foreground);
            //this.Bind(ViewModel, x => x.RequestsForeground, x => x.RequestsHeader.Foreground);
            //this.OneWayBind(ViewModel, x => x.DebugForeground, x => x.DebugHeader.Foreground);
            //this.OneWayBind(ViewModel, x => x.SelectedTabIndex, x => x.ChannelsTabControl.);

            //this.ChannelsTabControl.SelectionChanged += (sender, args) =>
            //{
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            //    {
            //        ViewModel.SelectedTabIndex = ChannelsTabControl.SelectedIndex;
            //    }));
            //};
            
        }
        
        public static DependencyProperty ViewModelProperty =
       DependencyProperty.Register("ViewModel", typeof(IMainScreenViewModel), typeof(MainScreenView),
           new PropertyMetadata(null));

        public IMainScreenViewModel ViewModel
        {
            get { return (IMainScreenViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainScreenViewModel)value; }
        }

        private void ChannelsTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
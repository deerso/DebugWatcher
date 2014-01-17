using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using ServiceStack;
using ServiceStack.Redis;

namespace DebugWatcher.ViewModels
{
    public interface IMainScreenViewModel : IRoutableViewModel
    {
        IReactiveList<string> RequestOutputMessages { get; set; } 
        IReactiveList<string> DebugOutputMessages { get; set; } 
        IReactiveList<string> ExceptionOutputMessages { get; set; }
        IReactiveCommand AddMessageCommand { get; set; }

        int SelectedTabIndex { get; set; }

        Brush ExceptionForeground { get; set; }
        Brush DebugForeground { get; set; }
        Brush RequestsForeground { get; set; }
    }
    public class MainScreenViewModel : ReactiveObject, IMainScreenViewModel
    {
        private Dictionary<LiveLogChannel, string> ChannelNames = new Dictionary<LiveLogChannel, string>
        {
            {LiveLogChannel.Debug, "DeersoWebDebug"},
            {LiveLogChannel.Exception, "DeersoWebExceptions"},
            {LiveLogChannel.Requests, "DeersoWebRequests"}
        };

        private int currentIndex = 0;
        public MainScreenViewModel(IScreen screen)
        {
            HostScreen = screen;

            RequestOutputMessages = new ReactiveList<string>();
            DebugOutputMessages = new ReactiveList<string>();
            ExceptionOutputMessages = new ReactiveList<string>();

            var requestSubject = new Subject<string>();
            var debugSubject = new Subject<string>();
            var exceptionSubject = new Subject<string>();

            requestSubject.ObserveOnDispatcher().Subscribe(x => RequestOutputMessages.Add(x));
            debugSubject.ObserveOnDispatcher().Subscribe(x => DebugOutputMessages.Add(x));
            exceptionSubject.ObserveOnDispatcher().Subscribe(x => ExceptionOutputMessages.Add(x));

            ExceptionForeground = new SolidColorBrush(Colors.DimGray);
            DebugForeground = new SolidColorBrush(Colors.DimGray);
            RequestsForeground = new SolidColorBrush(Colors.DodgerBlue);

            this.ObservableForProperty(x => x.SelectedTabIndex).Subscribe(x =>
            {
                currentIndex = x.Value;
                switch (currentIndex)
                {
                    case 0:
                        MessageBox.Show(RequestsForeground.ToString());
                        break;
                    case 1:
                        RequestsForeground = new SolidColorBrush(Colors.DeepPink);
                        break;
                    case 2:
                        break;
                }
            });




            //137.135.99.146
            try
            {
                using (var redisConsumer = new RedisClient("10.0.1.60"))
                using (var subscription = redisConsumer.CreateSubscription())
                {
                    subscription.OnSubscribe = channel =>
                    {
                        var msg = "Subscribed to {0}".FormatWith(channel);
                        switch (channel)
                        {
                            case "DeersoWebDebug":
                                debugSubject.OnNext(msg);
                                break;
                            case "DeersoWebExceptions":
                                exceptionSubject.OnNext(msg);
                                break;
                            case "DeersoWebRequests":
                                requestSubject.OnNext(msg);
                                break;
                        }
                    };
                    subscription.OnUnSubscribe = channel =>
                    {
                        var msg = "Unsubscribed from {0}".FormatWith(channel);
                        switch (channel)
                        {
                            case "DeersoWebDebug":
                                debugSubject.OnNext(msg);
                                break;
                            case "DeersoWebExceptions":
                                exceptionSubject.OnNext(msg);
                                break;
                            case "DeersoWebRequests":
                                requestSubject.OnNext(msg);
                                break;
                        }
                    };

                    subscription.OnMessage = (channel, msg) =>
                    {
                        switch (channel)
                        {
                            case "DeersoWebDebug":
                                debugSubject.OnNext(msg);
                                break;
                            case "DeersoWebExceptions":
                                exceptionSubject.OnNext(msg);
                                break;
                            case "DeersoWebRequests":
                                requestSubject.OnNext(msg);
                                break;
                        }
                    };
                    App.SubScriptionThread = new Thread(() =>
                    {
                        subscription.SubscribeToChannels("DeersoWebDebug", "DeersoWebExceptions", "DeersoWebRequests");
                    });
                    App.SubScriptionThread.Start();
                }
            }
            catch (Exception x)
            {
                RequestOutputMessages.Add("Error: " + x.Message);
            }
        }


        public string UrlPathSegment
        {
            get { return "MainScreen"; }
        }

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { this.RaiseAndSetIfChanged(ref _selectedTabIndex, value); }
        }
        private Brush _exceptionForeground;
        public Brush ExceptionForeground
        {
            get { return _exceptionForeground; }
            set { this.RaiseAndSetIfChanged(ref _exceptionForeground, value); }
        }
        private Brush _debugForeground;
        public Brush DebugForeground
        {
            get { return _debugForeground; }
            set { this.RaiseAndSetIfChanged(ref _debugForeground, value); }
        }
        private Brush _requestsForeground;
        public Brush RequestsForeground
        {
            get { return _requestsForeground; }
            set { this.RaiseAndSetIfChanged(ref _requestsForeground, value); }
        }


        private IReactiveList<string> _debugOutputMessages;
        public IReactiveList<string> DebugOutputMessages
        {
            get { return _debugOutputMessages; }
            set { this.RaiseAndSetIfChanged(ref _debugOutputMessages, value); }
        } 

        private IReactiveList<string> _exceptionOutputMessages;

        public IReactiveList<string> ExceptionOutputMessages
        {
            get { return _exceptionOutputMessages; }
            set { this.RaiseAndSetIfChanged(ref _exceptionOutputMessages, value); }
        }

        private IReactiveList<string> _requestOutputMessages; 
        public IReactiveList<string> RequestOutputMessages
        {
            get { return _requestOutputMessages; } 
            set { this.RaiseAndSetIfChanged(ref _requestOutputMessages, value); }
        }
        public IReactiveCommand AddMessageCommand { get; set; }
        public IScreen HostScreen { get; protected set; }
    }
    public enum LiveLogChannel
    {
        Debug,
        Exception,
        Requests,
    }
    //public void Run()
    //{


    //}
}
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DebugWatcher.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace DebugWatcher.ViewModels
{
    public interface IMainScreenViewModel : IRoutableViewModel
    {
        IReactiveList<string> RequestOutputMessages { get; set; } 
        IReactiveList<string> DebugOutputMessages { get; set; } 
        IReactiveList<string> ExceptionOutputMessages { get; set; }

        IReactiveList<RequestInfo> RequestInfoList { get; set; } 
        IReactiveCommand AddMessageCommand { get; set; }
        IReactiveCommand ReConnectCommand { get; set; }

        IReactiveCommand ProductionRedisAddressCommand { get; set; }
        IReactiveCommand StagingRedisAddressCommand { get; set; }

        int SelectedTabIndex { get; set; }
        string RedisServerAddress { get; set; }

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

            var redis = new RedisSubs();

            RequestInfoList = new ReactiveList<RequestInfo>();
            RequestInfoList.Add(new RequestInfo
            {
                IsCrawler = false,
                RefererUrl = "http://www.google.com",
                RequestTime = DateTime.Now - TimeSpan.FromDays(1),
                Url = "http://www.deerso.com",
                UserAgent = "TestAgent"
            });
            RequestOutputMessages = new ReactiveList<string>();
            DebugOutputMessages = new ReactiveList<string>();
            ExceptionOutputMessages = new ReactiveList<string>();

            var requestSubject = new Subject<string>();
            var debugSubject = new Subject<string>();
            var exceptionSubject = new Subject<string>();

            requestSubject.ObserveOnDispatcher().Subscribe(x =>
            {
                try
                {
                    var requestInfo = JsonSerializer.DeserializeFromString<RequestInfo>(x);
                    RequestInfoList.Add(requestInfo);
                }
                catch (Exception)
                {

                }
            });
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

            ReConnectCommand = new ReactiveCommand();
            ReConnectCommand.ObserveOnDispatcher().Subscribe(x =>
            {
                redis.Disconnect();
                redis.Connect(RedisServerAddress, debugSubject, exceptionSubject, requestSubject);
            });

            ProductionRedisAddressCommand = new ReactiveCommand();
            ProductionRedisAddressCommand.Subscribe(x =>
            {
                RedisServerAddress = "10.0.1.50";
            });
            StagingRedisAddressCommand = new ReactiveCommand();
            StagingRedisAddressCommand.Subscribe(x =>
            {
                RedisServerAddress = "137.135.99.146";
            });

            RedisServerAddress = "137.135.99.146";
            redis.Connect(RedisServerAddress, debugSubject, exceptionSubject, requestSubject);
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


        private string _redisServerAddress;

        public string RedisServerAddress
        {
            get { return _redisServerAddress; }
            set { this.RaiseAndSetIfChanged(ref _redisServerAddress, value); }
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

        private IReactiveList<RequestInfo> _requestInfoList;

        public IReactiveList<RequestInfo> RequestInfoList
        {
            get { return _requestInfoList; }
            set { this.RaiseAndSetIfChanged(ref _requestInfoList, value); }
        }

        public IReactiveCommand AddMessageCommand { get; set; }
        public IReactiveCommand ReConnectCommand { get; set; }

        public IReactiveCommand ProductionRedisAddressCommand { get; set; }
        public IReactiveCommand StagingRedisAddressCommand { get; set; }
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
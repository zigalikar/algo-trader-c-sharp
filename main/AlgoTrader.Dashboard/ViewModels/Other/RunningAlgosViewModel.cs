using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;

using AlgoTrader.Algos.Core;
using AlgoTrader.Core.Model.Attributes;
using AlgoTrader.Dashboard.Model.Views;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels.Other
{
    public class RunningAlgosViewModel : Screen
    {
        private readonly Timer _algosRuntimeUpdateTimer;

        public Visibility Visibility => RunningAlgos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        private IList<RunningAlgoListItem> _runningAlgos = new List<RunningAlgoListItem>();
        public IList<RunningAlgoListItem> RunningAlgos
        {
            get => _runningAlgos;
            set
            {
                if (Set(ref _runningAlgos, value))
                    NotifyOfPropertyChange(() => Visibility);
            }
        }

        public ICommand AlgoStopCommand => new Command<AlgoBase>((e) =>
        {
            var shellVm = IoC.Get<ShellViewModel>();
            var confirmBtnStyle = Application.Current.TryFindResource("ButtonDanger") as Style;
            var margin = new Thickness(0, 10, 0, 0);
            var btnConfirm = new Button
            {
                Content = "STOP",
                Style = confirmBtnStyle,
                Margin = margin,
                Command = new Command(() =>
                {
                    shellVm.SetProgressBarStatus(true);
                    AlgoManager.StopAlgo(e);
                    UpdateRunningAlgos();
                    shellVm.HidePopup();
                    shellVm.SetProgressBarStatus(false);
                })
            };
            
            var btnCancel = new Button
            {
                Content = "CANCEL",
                Margin = margin,
                Command = new Command(() => shellVm.HidePopup()),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            shellVm.ShowPopup(new GenericPopupContent
            {
                Title = "Confirm",
                Description = string.Format("Are you sure you want to stop algo '{0}'?", e.Name),
                Buttons = new List<Button> { btnConfirm, btnCancel }
            });
        });

        public RunningAlgosViewModel()
        {
            _algosRuntimeUpdateTimer = new Timer((s) =>
            {
                foreach (var algo in _runningAlgos)
                    algo.UpdateRunningTime();
            });
        }

        ~RunningAlgosViewModel()
        {
            _algosRuntimeUpdateTimer.Change(-1, -1);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            UpdateRunningAlgos();
        }

        public void UpdateRunningAlgos()
        {
            RunningAlgos = AlgoManager.GetRunningAlgos().Select(x => new RunningAlgoListItem(x)).ToList();
            _algosRuntimeUpdateTimer.Change(0, 1000);
        }

        public class RunningAlgoListItem : PropertyChangedBase
        {
            public string Name { get; }
            public AlgoBase Data { get; }

            private string _runningTime;
            public string RunningTime { get => _runningTime; set => Set(ref _runningTime, value); }

            public RunningAlgoListItem(AlgoBase algo)
            {
                Name = algo.Exchange.GetType().GetCustomAttribute<Exchange>().Name;
                Data = algo;
                UpdateRunningTime();
            }

            public void UpdateRunningTime()
            {
                if (Data.InitializedTime.HasValue)
                {
                    var diff = DateTime.UtcNow - Data.InitializedTime.Value;
                    RunningTime = string.Format("{0:00}:{1:00}:{2:00}", diff.Hours, diff.Minutes, diff.Seconds);
                }
            }
        }
    }
}

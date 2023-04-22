using System;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Dashboard.Views;
using AlgoTrader.Dashboard.Model.Common;
using AlgoTrader.Dashboard.Model.Views;

using TwitterAnalyser;
using TwitterAnalyser.Core.DTO;
using TwitterAnalyser.Core.Model;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class TwitterViewModel : ScreenList<TwitterViewModel.TwitterTabItem>, IDataErrorInfo
    {
        private TwitterView _twitterView;

        private readonly Mentions _mentions;

        #region Items

        private IList<Timeframe> _timeframes = Enum.GetValues(typeof(Timeframe)).Cast<Timeframe>().ToList();
        public IList<Timeframe> Timeframes { get => _timeframes; set => Set(ref _timeframes, value); }

        #endregion

        #region Commands

        public ICommand AnalyseCommand => new Command(() =>
        {
            var item = new TwitterTabItem(Ticker);
            AddItem(item);

            Task.Run(async () =>
            {
                Execute.OnUIThread(() => InProgress = true);
                _twitterView.ClearChart();
                await _mentions.Analyse(Ticker, SelectedTimeframe, new DateTime(2020, 8, 15), new Action<TickerMentionData>(e => _twitterView.AddChartData(e)));
                Execute.OnUIThread(() => InProgress = false);
            });
        });

        #endregion

        #region Form

        private string _ticker = "DOS";
        public string Ticker { get => _ticker; set => Set(ref _ticker, value); }

        #region Validation

        public string Error => Validation.Error;
        public string this[string columnName] => Validation[columnName];

        public ATDataErrorInfo Validation { get; }

        #endregion

        #endregion

        private bool _inProgress = true;
        public bool InProgress { get => _inProgress; set => Set(ref _inProgress, value); }

        private Timeframe _selectedTimeframe = Timeframe.Daily;
        public Timeframe SelectedTimeframe { get => _selectedTimeframe; set => Set(ref _selectedTimeframe, value); }

        private DateTime _selectedDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(14));
        public DateTime SelectedDate { get => _selectedDate; set => Set(ref _selectedDate, value); }

        public TwitterViewModel()
        {
            Validation = new ATDataErrorInfo(this, new Dictionary<string, string>
            {
                { nameof(Ticker), "Please select a currency ticker." }
            });

            _mentions = new Mentions("AAAAAAAAAAAAAAAAAAAAABlQGwEAAAAAiYtnpHLhgiAEK1G%2FksnuNEINFoA%3DCaFiQ8CcFYTkLeevF9LgcZkOsCLs1iGdqrh6L5GAKHcS0gk1ia");
        }

        #region Override

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            if (view is TwitterView twitterView)
                _twitterView = twitterView;
        }

        #endregion

        public class TwitterTabItem : PropertyChangedBase, INotifyPropertyChanged
        {
            public string Label { get; }

            public TwitterTabItem(string label)
            {
                Label = label;
            }
        }
    }
}

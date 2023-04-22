using System;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Algos.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Dashboard.Services;
using AlgoTrader.Core.Model.Attributes;
using AlgoTrader.Dashboard.Model.Views;
using AlgoTrader.Dashboard.ViewModels.Other;

using Caliburn.Micro;
using System.Collections.ObjectModel;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class AlgosViewModel : Screen
    {
        private readonly Settings _settings;

        #region Lists

        private IList<AlgoListItem> _allAlgos = new List<AlgoListItem>();
        public IList<AlgoListItem> AllAlgos { get => _allAlgos; private set => Set(ref _allAlgos, value); }

        private IList<ExchangeAccessData> _allAccounts = new List<ExchangeAccessData>();
        public IList<ExchangeAccessData> AllAccounts { get => _allAccounts; private set => Set(ref _allAccounts, value); }

        private IList<TimeFrameEnum> _allTimeFrames = new List<TimeFrameEnum>();
        public IList<TimeFrameEnum> AllTimeFrames { get => _allTimeFrames; private set => Set(ref _allTimeFrames, value); }

        private ObservableCollection<AlgoBase> _recentAlgos = new ObservableCollection<AlgoBase>();
        public ObservableCollection<AlgoBase> RecentAlgos { get => _recentAlgos; private set => Set(ref _recentAlgos, value); }

        #endregion

        #region Commands

        public ICommand AlgoStartCommand => new Command<AlgoListItem>(async (a) =>
        {
            var shellVm = IoC.Get<ShellViewModel>();
            shellVm.SetProgressBarStatus(true);
            var algo = await AlgoManager.RunAlgo(a.AlgoName, a.Data, a.SelectedAccount, a.SelectedCurrencyPair, a.SelectedTimeFrame);
            RunningAlgos.UpdateRunningAlgos();
            RecentAlgos.Insert(0, algo);
            while (RecentAlgos.Count > 5)
                RecentAlgos.RemoveAt(RecentAlgos.Count - 1);

            //_settings.RecentAlgos = _recentAlgos.ToList();
            await _settings.Save();
            NotifyOfPropertyChange(() => RecentAlgosVisibility);
            shellVm.SetProgressBarStatus(false);
        });

        #endregion

        private RunningAlgosViewModel _runningAlgos;
        public RunningAlgosViewModel RunningAlgos
        {
            get => _runningAlgos;
            set
            {
                if (_runningAlgos is IDeactivate oldVm)
                    oldVm.Deactivate(true);

                Set(ref _runningAlgos, value);
                if (value is IActivate newVm)
                    newVm.Activate();
            }
        }

        public Visibility RecentAlgosVisibility => _recentAlgos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public AlgosViewModel(Settings settings, RunningAlgosViewModel runningAlgos)
        {
            _settings = settings;
            RunningAlgos = runningAlgos;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            AllAlgos = AlgoManager.GetAllAlgos().Select(x => new AlgoListItem(x)).ToList();
            AllAccounts = AccountsManager.Accounts;
            AllTimeFrames = Enum.GetValues(typeof(TimeFrameEnum)).Cast<TimeFrameEnum>().ToList();
            RecentAlgos = new ObservableCollection<AlgoBase>(_settings.RecentAlgos);
        }

        public class AlgoListItem : PropertyChangedBase, IDataErrorInfo
        {
            public Algo Data { get; set; }

            private ExchangeAccessData _selectedAccount;
            public ExchangeAccessData SelectedAccount
            {
                get => _selectedAccount;
                set
                {
                    _selectedAccount = value;
                    var pairs = value.Exchange.CurrencyPairsDefinitions.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.PropertyType == typeof(ICurrencyPair)).Select(x => x.GetValue(null) as ICurrencyPair);
                    CurrencyPairOptions = pairs.ToList();
                    SelectedCurrencyPair = CurrencyPairOptions.FirstOrDefault();
                    CurrencyPairListEnabled = value != null;
                }
            }

            private bool _currencyPairListEnabled;
            public bool CurrencyPairListEnabled { get => _currencyPairListEnabled; set => Set(ref _currencyPairListEnabled, value); }

            private IList<ICurrencyPair> _currencyPairOptions = new List<ICurrencyPair>();
            public IList<ICurrencyPair> CurrencyPairOptions { get => _currencyPairOptions; private set => Set(ref _currencyPairOptions, value); }

            private ICurrencyPair _selectedCurrencyPair;
            public ICurrencyPair SelectedCurrencyPair { get => _selectedCurrencyPair; set => Set(ref _selectedCurrencyPair, value); }

            public TimeFrameEnum SelectedTimeFrame { get; set; }

            public string AlgoName { get; set; }
            
            #region Validation

            public string Error => Validation.Error;
            public string this[string columnName] => Validation[columnName];

            public ATDataErrorInfo Validation { get; }

            #endregion

            public AlgoListItem(Algo algo)
            {
                Data = algo;

                Validation = new ATDataErrorInfo(this, new Dictionary<string, string>
                {
                    { nameof(SelectedAccount), "Please select an account." },
                    { nameof(SelectedCurrencyPair), "Please select a currency pair." },
                    { nameof(SelectedTimeFrame), "Please select a time frame." },
                    { nameof(AlgoName), "Please select an algo name." }
                });
            }
        }
    }
}

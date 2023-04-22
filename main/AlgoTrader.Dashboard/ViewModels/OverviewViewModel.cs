using System;
using System.Windows;
using System.Collections.Generic;

using AlgoTrader.Core.DTO;
using AlgoTrader.Core.Model;
using AlgoTrader.Dashboard.ViewModels.Other;

using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Configurations;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class OverviewViewModel : Screen
    {
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

        private ExchangeAccessData _selectedAccount;
        public ExchangeAccessData SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (Set(ref _selectedAccount, value))
                    UpdateAccountChart();
            }
        }

        private string _selectedAccountBalance;
        public string SelectedAccountBalance { get => _selectedAccountBalance; set => Set(ref _selectedAccountBalance, value); }

        private SeriesCollection _accountBalanceSeries;
        public SeriesCollection AccountBalanceSeries { get => _accountBalanceSeries; set => Set(ref _accountBalanceSeries, value); }
        
        public Func<double, string> AccountBalanceSeriesFormatter { get; set; }

        public IList<ExchangeAccessData> Accounts { get; }
        
        public Visibility AccountsBalanceVisibility => Accounts?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public OverviewViewModel(RunningAlgosViewModel runningAlgos)
        {
            RunningAlgos = runningAlgos;
            Accounts = AccountsManager.Accounts;
            if (Accounts.Count > 0)
                SelectedAccount = Accounts[0];

            Execute.OnUIThread(() =>
            {
                AccountBalanceSeries = new SeriesCollection(Mappers.Xy<ExchangeAccessDataBalance>()
                   .X(x => (double) x.Timestamp.Ticks / TimeSpan.FromHours(1).Ticks)
                   .Y(y => y.Balance))
                {
                    new LineSeries
                    {
                        Values = new ChartValues<ExchangeAccessDataBalance>
                        {
                            new ExchangeAccessDataBalance
                            {
                                Timestamp = DateTime.UtcNow,
                                Balance = 10000
                            },
                            new ExchangeAccessDataBalance
                            {
                                Timestamp = DateTime.UtcNow.AddHours(1),
                                Balance = 11000
                            },
                            new ExchangeAccessDataBalance
                            {
                                Timestamp = DateTime.UtcNow.AddHours(3),
                                Balance = 17000
                            },
                            new ExchangeAccessDataBalance
                            {
                                Timestamp = DateTime.UtcNow.AddHours(4),
                                Balance = 13000
                            }
                        }
                    }
                };

                AccountBalanceSeriesFormatter = v => new DateTime((long) (v * TimeSpan.FromHours(1).Ticks)).ToString("dd/MM/yyyy H:mm:ss");
            });
        }

        private void UpdateAccountChart()
        {
            SelectedAccountBalance = string.Format("Account balance: {0}$", 12403);
        }
    }
}

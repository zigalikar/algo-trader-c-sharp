using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Dashboard.Views;
using AlgoTrader.Dashboard.Model.Views;
using AlgoTrader.Core.Model.Attributes;
using AlgoTrader.Dashboard.Model.Common;

using Microsoft.Win32;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class BacktestingViewModel : ScreenList<BacktestingViewModel.BacktestTabItem>, IDataErrorInfo
    {
        private BacktestingView _backtestingView;

        #region Lists

        private IList<Algo> _algos = new List<Algo>();
        public IList<Algo> Algos { get => _algos; set => Set(ref _algos, value); }

        #endregion

        #region Commands

        public ICommand BrowseCommand => new Command(() =>
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
                SelectedFile = dialog.FileName;
        });

        public ICommand BacktestCommand => new Command(() =>
        {
            var item = new BacktestTabItem(BacktestLabel);
            AddItem(item);

            var opts = new BacktestOptions { PartialResultCallback = new System.Action<BacktestResultCollection>(e => _backtestingView.AddChartData(e)) };
            if (int.TryParse(StartingBalance, out int startingBalance))
                opts.StartingBalance = startingBalance;

            Task.Run(async () =>
            {
                Execute.OnUIThread(() => InProgress = true);
                _backtestingView.ClearChart();
                await BacktestingManager.Backtest(SelectedAlgo, BacktestLabel, SelectedFile, opts);
                Execute.OnUIThread(() => InProgress = false);
            });
        });

        #endregion

        #region Form

        private Algo _selectedAlgo;
        public Algo SelectedAlgo { get => _selectedAlgo; set => Set(ref _selectedAlgo, value); }
        
        private string _selectedFile;
        public string SelectedFile { get => _selectedFile; set => Set(ref _selectedFile, value); }
        
        private string _backtestLabel;
        public string BacktestLabel { get => _backtestLabel; set => Set(ref _backtestLabel, value); }
        
        private string _startingBalance = 10000.ToString();
        public string StartingBalance { get => _startingBalance; set => Set(ref _startingBalance, value); }

        #region Validation
        
        public string Error => Validation.Error;
        public string this[string columnName] => Validation[columnName];
        
        public ATDataErrorInfo Validation { get; }

        #endregion

        #endregion

        private bool _inProgress = true;
        public bool InProgress { get => _inProgress; set => Set(ref _inProgress, value); }

        public BacktestingViewModel()
        {
            Validation = new ATDataErrorInfo(this, new Dictionary<string, string>
            {
                { nameof(SelectedAlgo), "Please select an algo." },
                { nameof(SelectedFile), "Please select a dataset." },
                { nameof(BacktestLabel), "Please label this backtest." }
            });
        }

        #region Override

        protected override IEnumerable<BacktestTabItem> GetItems()
        {
            return new List<BacktestTabItem>
            {
                new BacktestTabItem("sds"),
                new BacktestTabItem("asaasasas")
            };
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Algos = AlgoManager.GetAllAlgos().ToList();

            // debugging
            SelectedAlgo = Algos.Last();
            SelectedFile = "C:\\Users\\Žiga\\Desktop\\tmp\\exchange-csv\\BTCUSDT-1h-data.csv";
            BacktestLabel = "sss";
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            if (view is BacktestingView btView)
                _backtestingView = btView;
        }

        #endregion
        
        public class BacktestTabItem : PropertyChangedBase, INotifyPropertyChanged
        {
            public string Label { get; }

            public BacktestTabItem(string label)
            {
                Label = label;
            }
        }
    }
}

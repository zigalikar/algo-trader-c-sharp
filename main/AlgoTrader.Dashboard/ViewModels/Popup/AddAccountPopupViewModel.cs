using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Dashboard.Services;
using AlgoTrader.Dashboard.Model.Views;
using AlgoTrader.Core.Model.Attributes;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels.Popup
{
    public class AddAccountPopupViewModel : PopupBaseViewModel, IDataErrorInfo
    {
        #region Lists

        private IList<Exchange> _exchanges = new List<Exchange>();
        public IList<Exchange> Exchanges { get => _exchanges; set => Set(ref _exchanges, value); }

        #endregion

        #region Commands

        public ICommand TestConnectionCommand => new Command(async () =>
        {
            var shellVm = IoC.Get<ShellViewModel>();
            FormEnabled = false;
            shellVm.SetProgressBarStatus(true);
            bool result = await ExchangesManager.TestAPIKeys(SelectedExchange, GetCurrentExchangeAccessData());
            shellVm.SetProgressBarStatus(false);
            FormEnabled = true;

            if (result)
            {
                ShowNotice("Test successful.", NoticeStyle.Success);
                SaveButtonEnabled = true;
            }
            else
                ShowNotice("Test failed.", NoticeStyle.Error);
        });

        public ICommand SaveCommand => new Command(async () =>
        {
            var ead = GetCurrentExchangeAccessData();
            if (await AccountsManager.AddAccount(ead))
                Close();
            else
                ShowNotice("Save failed", NoticeStyle.Error);
        });

        #endregion

        #region Form

        private bool _formEnabled = true;
        public bool FormEnabled { get => _formEnabled; set { Set(ref _formEnabled, value); NotifyOfPropertyChange(() => TestConnectionButtonEnabled); } }

        private Exchange _selectedExchange;
        public Exchange SelectedExchange
        {
            get => _selectedExchange; set
            {
                if (Set(ref _selectedExchange, value))
                    SaveButtonEnabled = false;
            }
        }
        
        private string _apiKey;
        public string APIKey
        {
            get => _apiKey;
            set
            {
                if (Set(ref _apiKey, value))
                    SaveButtonEnabled = false;
            }
        }
        
        private string _apiSecret;
        public string APISecret
        {
            get => _apiSecret;
            set
            {
                if (Set(ref _apiSecret, value))
                    SaveButtonEnabled = false;
            }
        }

        private string _accountName;
        public string AccountName { get => _accountName; set => Set(ref _accountName, value); }

        public bool TestConnectionButtonEnabled => FormEnabled && Validation.ValidationResult;

        public bool _saveButtonEnabled = false;
        public bool SaveButtonEnabled { get => _saveButtonEnabled; set => Set(ref _saveButtonEnabled, value); }

        #region Validation

        public string Error => Validation.Error;
        public string this[string columnName] => Validation[columnName];

        public ATDataErrorInfo Validation { get; }

        #endregion

        #endregion

        private Brush _noticeForeground;
        public Brush NoticeForeground { get => _noticeForeground; set => Set(ref _noticeForeground, value); }

        private Visibility _noticeVisibility = Visibility.Collapsed;
        public Visibility NoticeVisibility { get => _noticeVisibility; set => Set(ref _noticeVisibility, value); }

        private string _noticeText;
        public string NoticeText { get => _noticeText; set => Set(ref _noticeText, value); }

        public AddAccountPopupViewModel(IEventAggregator eventAggregator, Settings settings) : base(eventAggregator)
        {
            Validation = new ATDataErrorInfo(this, new Dictionary<string, string>
            {
                { nameof(SelectedExchange), "Please select an exchange." },
                { nameof(APIKey), "Please enter a valid API key." },
                { nameof(APISecret), "Please enter a valid API secret." },
                { nameof(AccountName), "Please select an account name." }
            });

            Validation.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName.Equals(nameof(Validation.ValidationResult)))
                    NotifyOfPropertyChange(() => TestConnectionButtonEnabled);
            };
        }

        #region Override

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Exchanges = ExchangesManager.GetAllExchanges().ToList();
        }

        public override void CanClose(Action<bool> callback)
        {
            var shellVm = IoC.Get<ShellViewModel>();
            callback.Invoke(shellVm.GetProgressBarStatus() == false);
        }

        #endregion

        private void ShowNotice(string text, NoticeStyle style)
        {
            NoticeText = text;
            NoticeForeground = new System.Windows.Controls.ContentControl().FindResource(style == NoticeStyle.Error ? "ColorRed" : "ColorGreen") as Brush;
            NoticeVisibility = Visibility.Visible;
        }

        private void HideNotice()
        {
            NoticeVisibility = Visibility.Collapsed;
            NoticeText = string.Empty;
        }

        private ExchangeAccessData GetCurrentExchangeAccessData() => new ExchangeAccessData
        {
            Exchange = SelectedExchange,
            APIKey = APIKey,
            APISecret = APISecret,
            Name = AccountName
        };

        private enum NoticeStyle
        {
            Error = 0,
            Success = 1
        }
    }
}

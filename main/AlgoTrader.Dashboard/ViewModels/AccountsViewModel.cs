using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using AlgoTrader.Core.Model;
using AlgoTrader.Dashboard.Model.Views;
using AlgoTrader.Dashboard.ViewModels.Popup;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class AccountsViewModel : ATScreen
    {
        #region Lists

        private ObservableCollection<ExchangeAccessData> _accounts = new ObservableCollection<ExchangeAccessData>();
        public ObservableCollection<ExchangeAccessData> Accounts
        {
            get => _accounts;
            set
            {
                if (Set(ref _accounts, value))
                {
                    NotifyOfPropertyChange(() => NoAccountsVisibility);
                    NotifyOfPropertyChange(() => AccountsVisibility);
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddAccountCommand => new Command(() =>
        {
            var shellVm = IoC.Get<ShellViewModel>();
            shellVm.ShowPopup(new PopupContent<AddAccountPopupViewModel>());
        });

        public ICommand ShowAccount => new Command<ExchangeAccessData>(e =>
        {
            var shellVm = IoC.Get<ShellViewModel>();
            shellVm.ShowPopup(new PopupContent<ShowAccountPopupViewModel>(e));
        });

        public ICommand RemoveAccount => new Command<ExchangeAccessData>(async (e) =>
        {
            if (await AccountsManager.RemoveAccount(e) == false)
            {
                var shellVm = IoC.Get<ShellViewModel>();
                shellVm.ShowPopup(new GenericPopupContent
                {
                    Title = "ERROR",
                    Description = string.Format("Could not remove account '{0}'.", e.Name),
                    Buttons = new List<Button> { new Button { Content = "CLOSE", Command = new Command(() => shellVm.HidePopup()), Margin = new Thickness(0, 10, 0, 0) } }
                });
            }
        });

        #endregion

        public Visibility NoAccountsVisibility => _accounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AccountsVisibility => _accounts.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public AccountsViewModel(IEventAggregator eventAggregator) : base(eventAggregator) { }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            FetchAccounts();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            AccountsManager.Changed -= OnAccountsChanged;
            AccountsManager.Changed += OnAccountsChanged;
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            AccountsManager.Changed -= OnAccountsChanged;
        }

        private void OnAccountsChanged(object sender, IList<ExchangeAccessData> e) => FetchAccounts();

        private void FetchAccounts() => Accounts = new ObservableCollection<ExchangeAccessData>(AccountsManager.Accounts);
    }
}

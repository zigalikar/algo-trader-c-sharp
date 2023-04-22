using System;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using AlgoTrader.Dashboard.Views;
using AlgoTrader.Dashboard.Services;
using AlgoTrader.Dashboard.Interfaces;
using AlgoTrader.Dashboard.Model.Views;
using AlgoTrader.Dashboard.Model.Common;

using MaterialDesignThemes.Wpf;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class ShellViewModel : Conductor<Screen>, INavigationPage
    {
        private readonly Settings _settings;

        #region Command

        //public ICommand BackCommand => new Command(() => GoBack());

        public ICommand MenuClickCommand => new Command(() => MenuOpen = !MenuOpen);

        public ICommand SettingsClickCommand => new Command(() => Navigate<SettingsViewModel>(true));

        public ICommand MenuItemClickCommand => new Command<MenuNavigationItem>((item) =>
        {
            Navigate(item.PageType, true);
            MenuOpen = false;
        });

        public ICommand PopupBackgroundBlockerClickCommand => new Command(() =>
        {
            if (_shellView.IsMouseOnPopup() == false)
                HidePopup();
        });

        #endregion

        private ObservableCollection<MenuNavigationItem> _menuItems = new ObservableCollection<MenuNavigationItem>();
        public ObservableCollection<MenuNavigationItem> MenuItems { get => _menuItems; set => Set(ref _menuItems, value); }

        private bool _menuOpen;
        public bool MenuOpen { get => _menuOpen; set => Set(ref _menuOpen, value); }

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility { get => _progressBarVisibility; set => Set(ref _progressBarVisibility, value); }

        public Brush PopupBackgroundBlockerBackground => new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        public Visibility PopupVisibility => PopupContent != null ? Visibility.Visible : Visibility.Collapsed;
        
        private GenericPopupContent _popupContent;
        public GenericPopupContent PopupContent
        {
            get => _popupContent;
            private set
            {
                if (_popupContent is PopupContent oldPc && oldPc.Content != null)
                {
                    oldPc.Content.CloseRequest -= OnPopupCloseRequest;
                    if (oldPc.Content is IDeactivate oldVm)
                        oldVm.Deactivate(true);
                }
                
                if (Set(ref _popupContent, value))
                {
                    if (value is PopupContent newPc && newPc.Content != null)
                    {
                        newPc.Content.CloseRequest += OnPopupCloseRequest;
                        if (newPc.Content is IActivate vm)
                            vm.Activate();
                    }

                    NotifyOfPropertyChange(() => PopupVisibility);
                }
            }
        }

        private readonly ObservableCollection<Screen> _navigationStack = new ObservableCollection<Screen>();

        public ShellViewModel(Settings settings, AccountsPersister accountsPersister)
        {
            DisplayName = "AlgoTrader";

            _settings = settings;

            MenuItems = new ObservableCollection<MenuNavigationItem>
            {
                new MenuNavigationItem<OverviewViewModel>(PackIconKind.Home, "Overview"),
                new MenuNavigationItem<AlgosViewModel>(PackIconKind.Robot, "Algos"),
                new MenuNavigationItem<AccountsViewModel>(PackIconKind.Accounts, "Accounts"),
                new MenuNavigationItem<BacktestingViewModel>(PackIconKind.Cogs, "Backtesting"),
                //new MenuNavigationItem<TwitterViewModel>(PackIconKind.Twitter, "Twitter"),
                new MenuNavigationItem<LogViewModel>(PackIconKind.Notes, "Log"),
            };

            PropertyChanged += OnPropertyChanged;
            _settings.IsBusyChanged += OnSettingsIsBusyChanged;

            Task.Run(async () =>
            {
                SetProgressBarStatus(true);
                await AccountsManager.Initialize(accountsPersister);
                await _settings.Load();
                Navigate(MenuItems.ElementAt(3).PageType);
                SetProgressBarStatus(false);
            });
        }

        ~ShellViewModel()
        {
            PropertyChanged -= OnPropertyChanged;
            _settings.IsBusyChanged -= OnSettingsIsBusyChanged;
        }

        #region Implementation

        public void Navigate<T>(bool clear = false) where T : Screen
        {
            var item = IoC.Get<T>();
            ActiveItem = item;

            if (clear == true)
                _navigationStack.Clear();

            _navigationStack.Add(item);
        }

        public void Navigate(Type type, bool clear = false)
        {
            MethodInfo method = typeof(ShellViewModel).GetMethods().First(m => m.IsGenericMethod && m.Name.Equals(nameof(Navigate)));
            MethodInfo generic = method.MakeGenericMethod(type);
            generic.Invoke(this, new object[] { clear });
        }

        //public void GoBack()
        //{
        //    if (_navigationStack.Count > 1)
        //    {
        //        _navigationStack.RemoveAt(_navigationStack.Count - 1);
        //        Navigate(_navigationStack.Last().GetType());
        //    }
        //}

        //public void GoBackToRoot()
        //{
        //    _navigationStack.Clear();
        //    Navigate(MenuItems.First().PageType);
        //}

        #endregion

        private ShellView _shellView;
        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            if (view is ShellView sv)
                _shellView = sv;
        }

        public bool GetProgressBarStatus() => ProgressBarVisibility == Visibility.Visible;
        public void SetProgressBarStatus(bool visible) => ProgressBarVisibility = visible ? Visibility.Visible : Visibility.Collapsed;

        public void ShowPopup(GenericPopupContent content) => PopupContent = content;

        public void HidePopup()
        {
            if (PopupContent != null)
            {
                if (PopupContent is PopupContent pc && pc.Content != null)
                    pc.Content.Close();
                else
                    PopupContent = null;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(ActiveItem)))
            {
                foreach (var item in MenuItems)
                {
                    if (item.PageType != ActiveItem.GetType())
                        item.Unfocus();
                    else
                        item.Focus();
                }
            }
        }

        private void OnPopupCloseRequest(object sender, EventArgs e) => PopupContent = null;

        private void OnSettingsIsBusyChanged(object sender, bool e) => SetProgressBarStatus(e);
    }
}

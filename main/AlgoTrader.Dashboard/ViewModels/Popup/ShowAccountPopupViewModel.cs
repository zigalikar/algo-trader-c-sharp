using System.Windows.Input;

using AlgoTrader.Core.Model;
using AlgoTrader.Dashboard.Model.Views;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels.Popup
{
    public class ShowAccountPopupViewModel : PopupBaseViewModel
    {
        private ExchangeAccessData _parameter;
        public ExchangeAccessData Parameter
        {
            get => _parameter;
            set => Set(ref _parameter, value);
        }

        public ICommand CloseCommand => new Command(() => Close());

        public ShowAccountPopupViewModel(IEventAggregator eventAggregator) : base(eventAggregator) { }
    }
}

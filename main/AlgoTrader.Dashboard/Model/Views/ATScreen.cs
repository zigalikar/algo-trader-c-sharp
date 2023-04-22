using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Model.Views
{
    public abstract class ATScreen : Screen
    {
        protected readonly IEventAggregator _eventAggregator;

        public ATScreen(IEventAggregator eventAggregator = null)
        {
            _eventAggregator = eventAggregator;

            if (_eventAggregator != null)
                _eventAggregator.Subscribe(this);
        }
    }
}

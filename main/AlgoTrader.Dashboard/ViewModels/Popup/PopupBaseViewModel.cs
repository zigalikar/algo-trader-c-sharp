using System;

using AlgoTrader.Dashboard.Model.Views;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels.Popup
{
    public class PopupBaseViewModel : ATScreen
    {
        private EventHandler _closeRequest = delegate { };
        public event EventHandler CloseRequest
        {
            add { _closeRequest += value; }
            remove { _closeRequest -= value; }
        }

        public PopupBaseViewModel(IEventAggregator eventAggregator) : base(eventAggregator) { }

        public void Close()
        {
            CanClose(new System.Action<bool>(e =>
            {
                if (e)
                    _closeRequest.Invoke(this, null);
            }));
        }
    }
}

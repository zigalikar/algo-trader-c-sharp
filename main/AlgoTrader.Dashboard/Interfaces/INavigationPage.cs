using System;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Interfaces
{
    public interface INavigationPage
    {
        void Navigate<T>(bool clear = false) where T : Screen;
        void Navigate(Type type, bool clear = false);
        //void GoBack();
        //void GoBackToRoot();
        void SetProgressBarStatus(bool visible);
    }
}

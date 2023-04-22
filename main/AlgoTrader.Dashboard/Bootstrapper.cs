using System;
using System.Windows;
using System.Collections.Generic;

using AlgoTrader.Dashboard.Services;
using AlgoTrader.Dashboard.ViewModels;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container
                .Singleton<ShellViewModel>()
                .PerRequest<OverviewViewModel>()
                .PerRequest<AlgosViewModel>()
                .PerRequest<AccountsViewModel>()
                .Singleton<BacktestingViewModel>()
                .Singleton<TwitterViewModel>()
                .PerRequest<LogViewModel>()
                .PerRequest<SettingsViewModel>()
                .PerRequest<ViewModels.Other.RunningAlgosViewModel>()
                .PerRequest<ViewModels.Popup.ShowAccountPopupViewModel>()
                .PerRequest<ViewModels.Popup.AddAccountPopupViewModel>();

            container
                .Singleton<Settings>()
                .Singleton<AccountsPersister>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}

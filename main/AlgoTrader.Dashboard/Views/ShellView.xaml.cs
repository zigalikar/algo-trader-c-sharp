using System.Windows;
using System.ComponentModel;

using AlgoTrader.Dashboard.ViewModels;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Views
{
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ShellViewModel vmOld)
                vmOld.PropertyChanged -= OnPropertyChanged;

            if (DataContext is ShellViewModel vm)
                vm.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DataContext is ShellViewModel vm)
            {
                if (e.PropertyName.Equals(nameof(vm.PopupContent)))
                    ViewModelBinder.Bind(vm.PopupContent, Content, null);
            }
        }
        
        public bool IsMouseOnPopup() => PopupElement.IsMouseOver;
    }
}

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using MaterialDesignThemes.Wpf;

namespace AlgoTrader.Dashboard.Controls
{
    public class ATListView : ListView
    {
        public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.Register(nameof(SelectedCommand), typeof(ICommand), typeof(ATListView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        
        public ICommand SelectedCommand
        {
            get => (ICommand) GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public ATListView()
        {
            Focusable = false;
            RippleAssist.SetIsDisabled(this, true);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name.Equals(nameof(SelectedItem)) && SelectedItem != null)
            {
                var item = SelectedItem;
                SelectedItem = null;
                SelectedCommand?.Execute(item);
            }

            base.OnPropertyChanged(e);
        }
    }
}

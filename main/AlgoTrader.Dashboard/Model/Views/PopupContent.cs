using System.Reflection;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

using AlgoTrader.Dashboard.ViewModels.Popup;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Model.Views
{
    public class GenericPopupContent
    {
        public string Title { get; set; }
        public Brush TitleForeground { get; set; } = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public string Description { get; set; }
        public Brush DescriptionForeground { get; set; } = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public IList<Button> Buttons { get; set; }
    }

    public abstract class PopupContent : GenericPopupContent
    {
        public PopupBaseViewModel Content { get; protected set; }
    }

    public class PopupContent<T> : PopupContent where T : PopupBaseViewModel
    {
        public PopupContent(object parameter = null)
        {
            var content = IoC.Get<T>();
            if (parameter != null)
            {
                var prop = content.GetType().GetProperty("Parameter", BindingFlags.Public | BindingFlags.Instance);
                if (null != prop && prop.CanWrite)
                    prop.SetValue(content, parameter, null);
            }

            Content = content;
        }
    }
}

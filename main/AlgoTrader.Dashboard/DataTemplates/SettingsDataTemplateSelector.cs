using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace AlgoTrader.Dashboard.DataTemplates
{
    public class SettingsDataTemplateSelector : DataTemplateSelector
    {
        private static Dictionary<Type, DataTemplate> cache = new Dictionary<Type, DataTemplate>();

        static SettingsDataTemplateSelector()
        {
            cache[typeof(ViewModels.SettingsViewModel.SettingsComboboxItem)] = new DataTemplate(typeof(DtSettingsComboboxView));
            cache[typeof(ViewModels.SettingsViewModel.SettingsItem)] = new DataTemplate(typeof(DtSettingsItemView));
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                var itemType = item.GetType();
                if (cache.ContainsKey(itemType))
                    return cache[itemType];
            }

            return cache[typeof(ViewModels.SettingsViewModel.SettingsItem)];
        }
    }
}

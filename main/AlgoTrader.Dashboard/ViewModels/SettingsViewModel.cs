using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private IList<SettingsGroupItem> _items = new List<SettingsGroupItem>();
        public IList<SettingsGroupItem> Items { get => _items; set => Set(ref _items, value); }

        public SettingsViewModel()
        {
            Items = new List<SettingsGroupItem>
            {
                new SettingsGroupItem("General", new List<SettingsItem>
                {
                    new SettingsComboboxItem<object>("Theme", null, "Set your theme")
                })
            };
        }

        public class SettingsGroupItem : List<SettingsItem>
        {
            public string Title { get; set; }

            public SettingsGroupItem(string title, IEnumerable<SettingsItem> items)
            {
                Title = title;
                AddRange(items);
            }
        }

        public abstract class SettingsComboboxItem : SettingsItem
        {
            protected SettingsComboboxItem(string title, string description) : base(title, description) { }
        }

        public class SettingsComboboxItem<T> : SettingsComboboxItem where T : class
        {
            public IEnumerable<T> Items { get; set; }
            public T SelectedItem { get; set; }
            public string DisplayMemberName { get; set; }

            public SettingsComboboxItem(string title, IEnumerable<T> items, string description = null, string displayMemberName = null) : base(title, description)
            {
                Items = items;
                DisplayMemberName = displayMemberName;
            }
        }

        public abstract class SettingsItem
        {
            public string Title { get; set; }
            public string Description { get; set; }

            protected SettingsItem(string title, string description)
            {
                Title = title;
                Description = description;
            }
        }
    }
}

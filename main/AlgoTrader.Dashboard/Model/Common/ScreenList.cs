using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Model.Common
{
    public abstract class ScreenList<T> : Screen where T : class
    {
        private ObservableCollection<T> _items = new ObservableCollection<T>();
        public ObservableCollection<T> Items
        {
            get => _items;
            set
            {
                if (Set(ref _items, value))
                    NotifyOfPropertyChange(() => AnyItems);
            }
        }

        private T _selectedItem;
        public T SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

        public bool AnyItems => _items.Any();

        public ScreenList()
        {
            Items = new ObservableCollection<T>(GetItems());
        }

        protected virtual IEnumerable<T> GetItems() => new List<T>();

        protected void AddItem(T item, bool select = true)
        {
            Items.Add(item);
            NotifyOfPropertyChange(() => AnyItems);
            if (select)
                SelectedItem = Items.Last();
        }
    }
}

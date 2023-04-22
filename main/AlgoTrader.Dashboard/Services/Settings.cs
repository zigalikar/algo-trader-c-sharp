using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

using AlgoTrader.Algos.Core;

using Newtonsoft.Json;

namespace AlgoTrader.Dashboard.Services
{
    public class Settings
    {
        private string SettingsFileName => "algo_trader.settings.dat";
        private string SettingsKeyValueDivider => "%;-:,31#;.";

        private EventHandler<bool> _isBusyChanged = delegate { };
        public event EventHandler<bool> IsBusyChanged
        {
            add => _isBusyChanged += value;
            remove => _isBusyChanged -= value;
        }

        private IList<AlgoBase> _recentAlgos = new List<AlgoBase>();
        [PersistingProperty]
        public IList<AlgoBase> RecentAlgos { get => _recentAlgos.ToList(); set => _recentAlgos = value; }

        public async Task Save()
        {
            _isBusyChanged?.Invoke(this, true);
            var persistingSettings = GetPersistingSettingsList().Where(x => x.CanRead).ToList();
            var storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.Create, storage))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach (var prop in persistingSettings)
                    await writer.WriteLineAsync(string.Format("{0}{1}{2}", prop.Name, SettingsKeyValueDivider, JsonConvert.SerializeObject(prop.GetValue(this))));
            }
            _isBusyChanged?.Invoke(this, false);
        }

        public async Task Load()
        {
            _isBusyChanged?.Invoke(this, true);
            var persistingSettings = GetPersistingSettingsList().Where(x => x.CanWrite).ToList();
            var storage = IsolatedStorageFile.GetUserStoreForDomain();
            if (storage.FileExists(SettingsFileName))
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.Open, FileAccess.Read, storage))
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] keyValue = (await reader.ReadLineAsync()).Split(new string[] { SettingsKeyValueDivider }, 2, StringSplitOptions.RemoveEmptyEntries);
                        var prop = persistingSettings.FirstOrDefault(x => x.Name == keyValue[0]);
                        if (prop != null && prop.CanWrite)
                            prop.SetValue(this, JsonConvert.DeserializeObject(keyValue[1], prop.PropertyType));
                    }
                }
            }
            _isBusyChanged?.Invoke(this, false);
        }

        private IList<PropertyInfo> GetPersistingSettingsList() => GetType().GetProperties().Where(x => x.GetCustomAttribute<PersistingProperty>() != null).ToList();

        private class PersistingProperty : Attribute { }
    }
}

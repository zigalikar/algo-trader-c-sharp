using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Security.Cryptography;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;

using Newtonsoft.Json;

namespace AlgoTrader.Dashboard.Services
{
    public class AccountsPersister : IAccountsPersister
    {
        private string FileName => "algoTrader-persisted-data.dat";
        private string KeyValueDivider => "%;-:,31#;.";

        public async Task<IList<ExchangeAccessData>> Load()
        {
            var storage = IsolatedStorageFile.GetUserStoreForDomain();
            if (storage.FileExists(FileName) == false)
                return null;

            var accs = new List<ExchangeAccessData>();
            using (var stream = new IsolatedStorageFileStream(FileName, FileMode.Open, FileAccess.Read, storage))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var acc = JsonConvert.DeserializeObject<ExchangeAccessData>(Unprotect(await reader.ReadLineAsync()));
                    accs.Add(acc);
                }
            }

            return accs;
        }

        public async Task Persist(IList<ExchangeAccessData> accounts)
        {
            var storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (var stream = new IsolatedStorageFileStream(FileName, FileMode.Create, storage))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var acc in accounts)
                {
                    var entry = Protect(JsonConvert.SerializeObject(acc));
                    await writer.WriteLineAsync(entry);
                }
            }
        }

        private string FormatLine(string key, string value) => string.Format("{0}{1}{2}", key, KeyValueDivider, value);

        private string Protect(string str)
        {
            var entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            var data = Encoding.ASCII.GetBytes(str);
            var protectedData = Convert.ToBase64String(ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser));
            return protectedData;
        }

        private string Unprotect(string str)
        {
            var protectedData = Convert.FromBase64String(str);
            var entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            var data = Encoding.ASCII.GetString(ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser));
            return data;
        }
    }
}

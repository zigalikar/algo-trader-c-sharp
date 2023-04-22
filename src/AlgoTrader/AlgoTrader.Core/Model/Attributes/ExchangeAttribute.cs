using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using AlgoTrader.Core.DTO;

using Newtonsoft.Json;

namespace AlgoTrader.Core.Model.Attributes
{
    /// <summary>
    /// Attribute for an exchange
    /// </summary>
    public class Exchange : Attribute
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string ConfigPath { get; private set; }
        public Type CurrencyPairsDefinitions { get; private set; }
        public Type ImplementationClass { get; private set; }

        /// <summary>
        /// Initiates a new instance of the attribute
        /// </summary>
        /// <param name="configPath">Path to the JSON config file</param>
        /// <param name="currencyPairsDefinitions">Location of pair currency definitions</param>
        /// <param name="implementationClass">Location of the implementation class</param>
        public Exchange(string configPath, Type currencyPairsDefinitions, Type implementationClass)
        {
            ConfigPath = configPath;
            CurrencyPairsDefinitions = currencyPairsDefinitions;
            ImplementationClass = implementationClass;
        }

        private static IDictionary<Type, ExchangeConfig> _exchangeConfigCache = new Dictionary<Type, ExchangeConfig>();
        /// <summary>
        /// Gets configuration of the exchange
        /// </summary>
        /// <returns>Exchange configuration</returns>
        public ExchangeConfig GetConfig()
        {
            if (_exchangeConfigCache.ContainsKey(ImplementationClass))
                return _exchangeConfigCache[ImplementationClass];

            try
            {
                var assemblyName = "AlgoTrader.Exchanges";
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name.Equals(assemblyName));
                var resourceName = string.Format("{0}.Config.{1}.json", assemblyName, ConfigPath);
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(json) == false)
                    {
                        var config = JsonConvert.DeserializeObject<ExchangeConfig>(json);
                        _exchangeConfigCache[ImplementationClass] = config;
                        return config;
                    }
                }
                return null;
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, string.Format("Error in {0}.", nameof(GetConfig)));
                return null;
            }
        }
    }
}

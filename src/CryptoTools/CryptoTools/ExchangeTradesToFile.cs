using System;
using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Interfaces;

using CryptoTools.Core.Extensions;

namespace CryptoTools
{
    /// <summary>
    /// Listens to trades through a websocket and writes them to files in batches.
    /// </summary>
    public class ExchangeTradesToFile : IExchangeTradesToFile
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _saveFolder;
        private readonly IFeed<Trade> _feed;

        public int BatchSize => 500;
        private readonly List<Trade> _data = new List<Trade>();
        private static readonly object lockObject = new object();

        public ExchangeTradesToFile(string saveFolder, IFeed<Trade> websocket)
        {
            _saveFolder = saveFolder;
            _feed = websocket;

            websocket.Subscribe((s, e) =>
            {
                lock (lockObject)
                {
                    try
                    {
                        _data.Add(e);
                        var count = _data.Count;
                        if (count % 100 == 0)
                            logger.Info(string.Format("Received {0}/{1} trades.", count, BatchSize));

                        if (count >= BatchSize)
                        {
                            // format date
                            var now = DateTime.UtcNow;
                            var dateString = string.Format("{0:00}-{1:00}-{2}_{3:00}-{4:00}-{5:00}-{6:000}", now.Day, now.Month, now.Year, now.Hour, now.Minute, now.Second, now.Millisecond);

                            // write to file
                            var filePath = Path.Combine(saveFolder, string.Format("Trades_{0}.csv.gz", dateString));
                            logger.Info(string.Format("Writing trades batch to file {0}", filePath));
                            Directory.CreateDirectory(saveFolder);
                            
                            using (var compressedFileStream = File.Create(filePath))
                            using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                            {
                                compressionStream.WriteLine("DATE;PRICE;AMOUNT;MAKERSIDE;LIQUIDATION");
                                foreach (var d in _data)
                                    compressionStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4}", d.Timestamp.ToMillisecondsFromEpoch(), d.Price, d.TradeSize, d.MakerSide, d.Liquidation));
                            }

                            logger.Info(string.Format("Saved {0} trades to file {1}", count, filePath));
                            _data.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, string.Format("Error in {0}.", nameof(ExchangeTradesToFile)));
                    }
                }
            });
        }

        public async Task Start()
        {
            logger.Trace("Start");
            await _feed.Start();
        }

        public async Task Stop()
        {
            logger.Trace("Stop.");
            await _feed.Stop();
        }
    }
}

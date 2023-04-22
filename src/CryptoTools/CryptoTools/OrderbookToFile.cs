using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.IO.Compression;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;

using CryptoTools.Core.Extensions;

namespace CryptoTools
{
    /// <summary>
    /// Listens to orderbook data from the exchange and saves them to the hard drive
    /// </summary>
    public class OrderbookToFile
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _saveFolder;
        private readonly IFeed<Orderbook> _feed;
        private readonly TimeSpan _interval;
        
        private static readonly object lockObject = new object();
        private DateTime _lastSave;

        public OrderbookToFile(string saveFolder, IFeed<Orderbook> feed, TimeSpan? interval = null)
        {
            _saveFolder = saveFolder;
            _feed = feed;
            _interval = interval ?? TimeSpan.FromMinutes(15);

            _feed.Subscribe((s, e) =>
            {
                lock (lockObject)
                {
                    try
                    {
                        if (DateTime.UtcNow - _lastSave < _interval)
                            return;

                        // format date
                        var ts = e.Timestamp;
                        var dateString = string.Format("{0:00}-{1:00}-{2}_{3:00}-{4:00}-{5:00}", ts.Day, ts.Month, ts.Year, ts.Hour, ts.Minute, ts.Second);

                        // write to file
                        var filePath = Path.Combine(saveFolder, string.Format("Orderbook_{0}.csv.gz", dateString));
                        logger.Info(string.Format("Writing orderbook to file {0}", filePath));
                        Directory.CreateDirectory(saveFolder);

                        using (var compressedFileStream = File.Create(filePath))
                        using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            compressionStream.WriteLine(ts.ToString("o"));
                            compressionStream.WriteLine("BID;ASK");

                            var asksCount = e.Asks.Count();
                            var bidsCount = e.Bids.Count();
                            var count = Math.Max(asksCount, bidsCount);
                            for (var i = 0; i < count; i++)
                            {
                                var writeStr = "";
                                if (i < bidsCount)
                                {
                                    var bid = e.Bids.ElementAt(i);
                                    writeStr += string.Format("{0}@{1}", bid.Quantity, bid.Price);
                                }

                                writeStr += ";";
                                if (i < asksCount)
                                {
                                    var ask = e.Asks.ElementAt(i);
                                    writeStr += string.Format("{0}@{1}", ask.Quantity, ask.Price);
                                }

                                compressionStream.WriteLine(string.Format(CultureInfo.InvariantCulture, writeStr));
                            }
                        }

                        _lastSave = DateTime.UtcNow;
                        logger.Info(string.Format("Saved orderbook to file {0}", filePath));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, string.Format("Error in {0}.", nameof(OrderbookToFile)));
                    }
                }
            });
        }

        public void Start()
        {
            logger.Info("Start orderbook listener.");
            _feed.Start();
        }

        public void Stop()
        {
            logger.Info("Stop orderbook listener.");
            _feed.Stop();
        }
    }
}

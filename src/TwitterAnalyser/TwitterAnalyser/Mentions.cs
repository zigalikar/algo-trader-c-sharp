using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.HttpClient;

using TwitterAnalyser.Core.DTO;
using TwitterAnalyser.Core.Model;

namespace TwitterAnalyser
{
    public class Mentions
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _accessToken;
        private readonly AlgoTraderHttpClient _httpClient;
        
        private static SemaphoreSlim analyseSemaphore;

        public Mentions(string accessToken)
        {
            _accessToken = accessToken;
            _httpClient = new AlgoTraderHttpClient();
            _httpClient.SetBearerToken(accessToken);

            analyseSemaphore = new SemaphoreSlim(0, 1);
        }

        public async Task Analyse(string ticker, Timeframe timeframe, DateTime minDate, Action<TickerMentionData> callback)
        {
            DateTime? date = null;
            var batch = new List<TwitterSearchResponse_v1.Status>();

            var invokeCallback = new Action<TwitterSearchResponse_v1.Status>(next =>
            {
                callback.Invoke(new TickerMentionData
                {
                    Date = date.Value,
                    Mentions = batch.Count,
                    TimeFrame = timeframe
                });

                batch.Clear();
                if (next != null)
                {
                    date = next.CreatedAt; // set date for next
                    batch.Add(next);
                }
            });

            await GetData(ticker, minDate, new Action<IList<TwitterSearchResponse_v1.Status>>(tweets =>
            {
                try
                {
                    //analyseSemaphore.Wait();

                    foreach (var tweet in tweets)
                    {
                        if (date == null)
                            date = tweet.CreatedAt;

                        // filter by timeframe
                        if (timeframe == Timeframe.Hourly)
                        {
                            if (tweet.CreatedAt.Year == date.Value.Year && tweet.CreatedAt.Month == date.Value.Month && tweet.CreatedAt.Day == date.Value.Day && tweet.CreatedAt.Hour == date.Value.Hour)
                                batch.Add(tweet);
                            else
                                invokeCallback.Invoke(tweet);
                        }
                        else if (timeframe == Timeframe.Daily)
                        {
                            if (tweet.CreatedAt.Year == date.Value.Year && tweet.CreatedAt.Month == date.Value.Month && tweet.CreatedAt.Day == date.Value.Day)
                                batch.Add(tweet);
                            else
                                invokeCallback.Invoke(tweet);
                        }
                        else
                            throw new NotImplementedException();
                    }

                    //analyseSemaphore.Release();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }));

            if (batch.Count > 0)
                invokeCallback.Invoke(null);
        }

        private Task<TwitterSearchResponse_v1> GetSearchHistory(string param)
        {
            var url = string.Format("https://api.twitter.com/1.1/search/tweets.json{0}", param);
            return _httpClient.GetAsync<TwitterSearchResponse_v1>(url);
        }

        private string GetRequestQuery(string ticker, string maxId = null)
        {
            var url = string.Format("?q=${0}&result_type=recent&count=100", ticker);
            if (string.IsNullOrWhiteSpace(maxId) == false)
                url += string.Format("&max_id={0}", maxId);
            return url;
        }

        private async Task GetData(string ticker, DateTime minDate, Action<List<TwitterSearchResponse_v1.Status>> callback)
        {
            var lastDate = DateTime.UtcNow;
            var tweets = await GetSearchHistory(GetRequestQuery(ticker)); // get tweets
            while (lastDate >= minDate)
            {
                var mentions = new List<TwitterSearchResponse_v1.Status>();
                foreach (var tweet in tweets.Statuses)
                {
                    lastDate = tweet.CreatedAt;
                    if (lastDate < minDate) // reached min date
                        break;

                    // not past min date - add to analysed data
                    mentions.Add(tweet);
                }

                callback.Invoke(mentions);

                // no mentions
                if (mentions.Count == 0)
                    break;

                // not past min date - fetch next page
                if (lastDate >= minDate)
                {
                    var last = mentions.Last();
                    var meta = tweets.Metadata;
                    if (string.IsNullOrWhiteSpace(meta.NextResults))
                    {
                        // TODO: neka omejitev API-ja? tukaj ponovno searchaj z navadnimi parametri in max_id-jem namesto z next_results queryjem
                        var maxId = last.Id;
                        tweets = await GetSearchHistory(GetRequestQuery(ticker, maxId));
                        if (tweets.Statuses.Count() == 0) // no new tweets
                            break;
                    }
                    else
                    {
                        var next = meta.NextResults;
                        logger.Debug(string.Format("Getting next page: {0}", next));
                        logger.Debug(string.Format("Last date: {0}", last.CreatedAt.ToShortTimeString()));
                        tweets = await GetSearchHistory(next); // next page
                    }
                }

                mentions.Clear();
            }
        }
    }
}

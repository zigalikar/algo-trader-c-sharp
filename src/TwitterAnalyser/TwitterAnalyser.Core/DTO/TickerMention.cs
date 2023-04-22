using System;

using TwitterAnalyser.Core.Model;

namespace TwitterAnalyser.Core.DTO
{
    public class TickerMentionData
    {
        public DateTime Date { get; set; }
        public int Mentions { get; set; }
        public Timeframe TimeFrame { get; set; }
    }
}

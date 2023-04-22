using System;

namespace FancyCandles
{
    public enum WholeContainerCandleOrderType
    {
        Buy = 0,
        Sell = 1
    }
    
    struct WholeContainerCandle
    {
        public DateTime t; // Момент времени включая дату и время
        public double O;
        public double H;
        public double L;
        public double C;
        public long V;

        public ICandlePrices Portfolio;

        public int Index;
        public double LeftMargin;
        public double BodyWidth;

        // Далее как доля от (High-Low) видимых свечей. Т.е. данные значения нужно домножить на высоту видимой области:
        public double ShadowsHeight;
        public double PortfolioShadowsHeight;
        public double BodyHeight;
        public double PortfolioBodyHeight;
        public double ShadowsBottomMargin;
        public double PortfolioShadowsBottomMargin;
        public double BodyBottomMargin;
        public double PortfolioBodyBottomMargin;
        public double OrderBottomMargin;

        public double VolumeBarHeight; // Как доля от максимального значения Volume видимых свечей.

        // Текст всплывающей подсказки для свечки: 
        public string ToolTipText;
        public string VolumeToolTipText;

        // Логическая сумма всех DateTimeMilestones для данной свечи:
        public byte DateTimeMilestonesBitwiseSum;

        public ICandleOrderInfo Order { get; }

        //public WholeContainerCandle(DateTime t, double O, double H, double L, double C, double P_O, double P_H, double P_L, double P_C, long V, int index, double visibleCandlesRangeLH, double visibleCandlesLow, double bodyWidth, double betweenCandlesWidth, long visibleCandlesMaxVolume, byte dateTimeMilestonesBitwiseSum, WholeContainerCandleOrderType orderType, double orderPrice)
        //{
        //    this.t = t;
        //    this.O = O;
        //    this.H = H;
        //    this.L = L;
        //    this.C = C;
        //    this.P_O = P_O;
        //    this.P_H = P_H;
        //    this.P_L = P_L;
        //    this.P_C = P_C;
        //    this.V = V;
        //    this.OrderType = orderType;
        //    this.OrderPrice = orderPrice;
        //    Index = index;
        //    ShadowsHeight = (H - L) / visibleCandlesRangeLH;
        //    PortfolioShadowsHeight = (P_H - P_L) / visibleCandlesRangeLH;
        //    BodyHeight = Math.Abs(O - C) / visibleCandlesRangeLH;
        //    PortfolioBodyHeight = Math.Abs(P_O - P_C) / visibleCandlesRangeLH;
        //    ShadowsBottomMargin = (L - visibleCandlesLow) / visibleCandlesRangeLH;
        //    //PortfolioShadowsBottomMargin = (P_L - visibleCandlesLow) / visibleCandlesRangeLH;
        //    PortfolioShadowsBottomMargin = 0;
        //    BodyBottomMargin = (Math.Min(O, C) - visibleCandlesLow) / visibleCandlesRangeLH;
        //    //PortfolioBodyBottomMargin = (Math.Min(P_O, P_C) - visibleCandlesLow) / visibleCandlesRangeLH;
        //    PortfolioBodyBottomMargin = 0;
        //    OrderBottomMargin = (OrderPrice - visibleCandlesLow) / visibleCandlesRangeLH;
        //    ToolTipText = $"{t.ToString("g", CultureInfo.CurrentCulture)}\nO={O}\nH={H}\nL={L}\nC={C}\nV={V}"; //"d.MM.yyyy H:mm"
        //    VolumeToolTipText = $"{t.ToString("g", CultureInfo.CurrentCulture)}\nV={V}";
        //    BodyWidth = bodyWidth;
        //    LeftMargin = (bodyWidth + betweenCandlesWidth) * index;
        //    VolumeBarHeight = V / (double)visibleCandlesMaxVolume;
        //    DateTimeMilestonesBitwiseSum = dateTimeMilestonesBitwiseSum;
        //}

        public WholeContainerCandle(ICandle cndl, int index, double visibleCandlesRangeLH, double visibleCandlesLow, double visiblePortfolioCandlesRangeLH, double visiblePortfolioCandlesLow, double bodyWidth, double betweenCandlesWidth, long visibleCandlesMaxVolume, byte dateTimeMilestonesBitwiseSum)
        {
            t = cndl.t;
            O = cndl.O;
            H = cndl.H;
            L = cndl.L;
            C = cndl.C;
            V = cndl.V;
            Order = cndl.Order;
            Index = index;
            Portfolio = cndl.Portfolio;

            ShadowsHeight = (H - L) / visibleCandlesRangeLH;
            BodyHeight = Math.Abs(O - C) / visibleCandlesRangeLH;
            ShadowsBottomMargin = (L - visibleCandlesLow) / visibleCandlesRangeLH;
            BodyBottomMargin = (Math.Min(O, C) - visibleCandlesLow) / visibleCandlesRangeLH;

            if (Portfolio != null)
            {
                PortfolioShadowsHeight = (Portfolio.H - Portfolio.L) / visiblePortfolioCandlesRangeLH;
                PortfolioShadowsBottomMargin = (Portfolio.L - visiblePortfolioCandlesLow) / visiblePortfolioCandlesRangeLH;
                PortfolioBodyHeight = Math.Abs(Portfolio.O - Portfolio.C) / visiblePortfolioCandlesRangeLH;
                PortfolioBodyBottomMargin = (Math.Min(Portfolio.O, Portfolio.C) - visiblePortfolioCandlesLow) / visiblePortfolioCandlesRangeLH;
            }
            else
            {
                PortfolioShadowsHeight = 0;
                PortfolioShadowsBottomMargin = 0;
                PortfolioBodyHeight = 0;
                PortfolioBodyBottomMargin = 0;
            }

            OrderBottomMargin = Order != null ? ((Order.Price - visibleCandlesLow) / visibleCandlesRangeLH) : 0;

            ToolTipText = $"{t.ToString("d.MM.yyyy H:mm")}\nO={O}\nH={H}\nL={L}\nC={C}\nV={V}";
            VolumeToolTipText = $"{t.ToString("d.MM.yyyy H:mm")}\nV={V}";
            BodyWidth = bodyWidth;
            LeftMargin = (bodyWidth + betweenCandlesWidth) * index;
            VolumeBarHeight = V / (double)visibleCandlesMaxVolume;
            DateTimeMilestonesBitwiseSum = dateTimeMilestonesBitwiseSum;
        }
    }
}

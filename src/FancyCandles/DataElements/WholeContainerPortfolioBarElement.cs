using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using FancyCandles.Model;

namespace FancyCandles.DataElements
{
    class WholeContainerPortfolioBarElement : FrameworkElement
    {
        public WholeContainerPortfolioBarElement()
        {
            ToolTip tt = new ToolTip() { FontSize = CandleChart.candleToolTipFontSize, BorderBrush = Brushes.Beige };
            tt.Content = "";
            ToolTip = tt;

            // Зададим время задержки появления подсказок здесь, а расположение подсказок (если его нужно поменять) зададим в XAML:
            ToolTipService.SetShowDuration(this, int.MaxValue);
            ToolTipService.SetInitialShowDelay(this, 0);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        static WholeContainerPortfolioBarElement()
        {
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(new WholeContainerCandle(), new PropertyChangedCallback(OnCandleDataChanged)) { AffectsRender = true };
            CandleDataProperty = DependencyProperty.Register("CandleData", typeof(WholeContainerCandle), typeof(WholeContainerPortfolioBarElement), metadata);

            metadata = new FrameworkPropertyMetadata(Config.DownBrush) { AffectsRender = true };
            BearishCandleBrushProperty = DependencyProperty.Register("BearishCandleBrush", typeof(Brush), typeof(WholeContainerPortfolioBarElement), metadata);

            metadata = new FrameworkPropertyMetadata(Config.UpBrush) { AffectsRender = true };
            BullishCandleBrushProperty = DependencyProperty.Register("BullishCandleBrush", typeof(Brush), typeof(WholeContainerPortfolioBarElement), metadata);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        private static void OnCandleDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WholeContainerPortfolioBarElement thisElement = (WholeContainerPortfolioBarElement) obj;
            ((ToolTip)thisElement.ToolTip).Content = thisElement.CandleData.ToolTipText;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bullish candle (when the Close is higher than the Open).</summary>
        public Brush BullishCandleBrush
        {
            get { return (Brush)GetValue(BullishCandleBrushProperty); }
            set { SetValue(BullishCandleBrushProperty, value); }
        }
        public static readonly DependencyProperty BullishCandleBrushProperty;
        //---------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bearish candle (when the Close is lower than the Open).</summary>
        public Brush BearishCandleBrush
        {
            get { return (Brush)GetValue(BearishCandleBrushProperty); }
            set { SetValue(BearishCandleBrushProperty, value); }
        }
        public static readonly DependencyProperty BearishCandleBrushProperty;
        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CandleDataProperty;
        public WholeContainerCandle CandleData
        {
            get { return (WholeContainerCandle)GetValue(CandleDataProperty); }
            set { SetValue(CandleDataProperty, value); }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (CandleData.Portfolio == null)
                return;

            Brush transparentBrush = Brushes.Transparent;
            Pen transparentPen = new Pen(transparentBrush, 0);
            Brush brush = CandleData.Portfolio.C > CandleData.Portfolio.O ? BullishCandleBrush : BearishCandleBrush;
            Pen pen = new Pen(brush, 1);
            
            if (CandleData.BodyWidth >= 2.5)
            {
                double bodyHeighth = CandleData.PortfolioBodyHeight * RenderSize.Height;
                if (bodyHeighth == 0.0) bodyHeighth = 1.0;
                double bodyBottomMargin = CandleData.PortfolioBodyBottomMargin * RenderSize.Height;
                drawingContext.DrawRectangle(brush, transparentPen, new Rect(CandleData.LeftMargin, RenderSize.Height - bodyHeighth - bodyBottomMargin, CandleData.BodyWidth, bodyHeighth));
                //drawingContext.DrawRectangle(brush, transparentPen, new Rect(CandleData.LeftMargin, 0.5, CandleData.BodyWidth, 10));
            }
            
            double shadowsHeighth = CandleData.PortfolioShadowsHeight * RenderSize.Height;
            double shadowsBottomMargin = CandleData.PortfolioShadowsBottomMargin * RenderSize.Height;
            double y1 = RenderSize.Height - shadowsBottomMargin;
            double y2 = y1 - shadowsHeighth;
            double x = CandleData.LeftMargin + CandleData.BodyWidth / 2.0;
            drawingContext.DrawLine(pen, new Point(x, y1), new Point(x, y2));
            
            drawingContext.DrawRectangle(transparentBrush, transparentPen, new Rect(CandleData.LeftMargin, y2, CandleData.BodyWidth, shadowsHeighth));
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
    }
}

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using FancyCandles.Model;

namespace FancyCandles.DataElements
{
    class WholeContainerCandleElement : FrameworkElement
    {
        public WholeContainerCandleElement()
        {
            ToolTip tt = new ToolTip { FontSize = CandleChart.candleToolTipFontSize, BorderBrush = Brushes.Beige };
            tt.Content = "";
            ToolTip = tt;
            
            ToolTipService.SetShowDuration(this, int.MaxValue);
            ToolTipService.SetInitialShowDelay(this, 0);
        }

        private static void OnCandleDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is WholeContainerCandleElement el && el.ToolTip is ToolTip tp)
                tp.Content = el.CandleData.ToolTipText;
        }

        /// <summary>
        /// Data to display
        /// </summary>
        public static readonly DependencyProperty CandleDataProperty = DependencyProperty.Register(nameof(CandleData), typeof(WholeContainerCandle), typeof(WholeContainerCandleElement), new FrameworkPropertyMetadata(new WholeContainerCandle(), new PropertyChangedCallback(OnCandleDataChanged)) { AffectsRender = true });
        public WholeContainerCandle CandleData
        {
            get => (WholeContainerCandle)GetValue(CandleDataProperty);
            set => SetValue(CandleDataProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var transparentBrush = Brushes.Transparent;
            var transparentPen = new Pen(transparentBrush, 0);
            var brush = CandleData.C > CandleData.O ? Config.UpBrush : Config.DownBrush;
            var pen = new Pen(brush, 1);
            
            if (CandleData.BodyWidth >= 2.5)
            {
                var bodyHeight = CandleData.BodyHeight * RenderSize.Height;
                if (bodyHeight == 0.0)
                    bodyHeight = 1.0;

                var bodyBottomMargin = CandleData.BodyBottomMargin * RenderSize.Height;
                drawingContext.DrawRectangle(brush, transparentPen, new Rect(CandleData.LeftMargin, RenderSize.Height - bodyHeight - bodyBottomMargin, CandleData.BodyWidth, bodyHeight));
            }
            
            var shadowsHeighth = CandleData.ShadowsHeight * RenderSize.Height;
            var shadowsBottomMargin = CandleData.ShadowsBottomMargin * RenderSize.Height;
            var y1 = RenderSize.Height - shadowsBottomMargin;
            var y2 = y1 - shadowsHeighth;
            var x = CandleData.LeftMargin + CandleData.BodyWidth / 2.0;
            drawingContext.DrawLine(pen, new Point(x, y1), new Point(x, y2));
            
            drawingContext.DrawRectangle(transparentBrush, transparentPen, new Rect(CandleData.LeftMargin, y2, CandleData.BodyWidth, shadowsHeighth));
            if (CandleData.Order != null)
            {
                var orderCircleRadius = 3;
                var orderBrush = new SolidColorBrush(CandleData.Order.Type == WholeContainerCandleOrderType.Buy ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0));
                var orderBottomMargin = CandleData.OrderBottomMargin * RenderSize.Height;
                var orderY = RenderSize.Height - orderBottomMargin;
                drawingContext.DrawRoundedRectangle(orderBrush, new Pen(orderBrush, 2), new Rect(CandleData.LeftMargin + CandleData.BodyWidth / 2 - orderCircleRadius, orderY - orderCircleRadius, orderCircleRadius * 2, orderCircleRadius * 2), orderCircleRadius, orderCircleRadius);
            }
        }
    }
}

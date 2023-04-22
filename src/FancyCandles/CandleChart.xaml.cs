﻿/* 
    Copyright 2019 Dennis Geller.

    This file is part of FancyCandles.

    FancyCandles is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FancyCandles is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FancyCandles.  If not, see<https://www.gnu.org/licenses/>. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices; // [CallerMemberName]

using FancyCandles.Model;
using FancyCandles.TickElements;
using FancyCandles.DataElements;

namespace FancyCandles
{
    /// <summary>
    /// Candle chart control derived from UserControl.
    /// </summary>
    public partial class CandleChart : UserControl, INotifyPropertyChanged
    {
        //----------------------------------------------------------------------------------------------------------------------------------
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CandleChart.candleToolTipFontSize'
        public static double candleToolTipFontSize = 9.0;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CandleChart.candleToolTipFontSize'
        //----------------------------------------------------------------------------------------------------------------------------------
        void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            //IsAlreadyLoaded = true;
            ReCalc_TimeFrame();
            ReCalc_MaxNumberOfCharsInPrice();
            ReCalc_MaxNumberOfDigitsAfterPointInPrice();
            CandleWidth = InitialCandleWidth;
            GapBetweenCandles = InitialGapBetweenCandles;
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Default constructor.</summary>
        public CandleChart()
        {
            InitialCandleWidth = DefaultInitialCandleWidth;
            InitialGapBetweenCandles = DefaultInitialGapBetweenCandles;

            InitializeComponent();
            //IsAlreadyLoaded = false;

            VisibleCandlesRange = IntRange.Undefined;
            VisibleCandles = new ObservableCollection<WholeContainerCandle>();
            Loaded += new RoutedEventHandler(OnUserControlLoaded);
            //Dispatcher.Invoke(OnMyCandleChartLoaded, System.Windows.Threading.DispatcherPriority.Loaded);
            //Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
            //SizeChanged += OnSizeChanged;
            
            UpdatePanelRowDefinitions(1, 2, IsVolumePanelVisible);
            UpdatePanelRowDefinitions(3, 4, IsPortfolioPanelVisible);

            RefreshRowSpans();
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the background for the price chart and volume diagram areas.</summary>
        ///<value>The background for the price chart and volume diagram areas. The default is determined by the <see cref="DefaultChartAreaBackground"/>values.</value>
        ///<remarks>
        ///This background is not applied to the horizontal and vertical axis areas, which contain tick marks and labels.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="ChartAreaBackgroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush ChartAreaBackground
        {
            get { return (Brush)GetValue(ChartAreaBackgroundProperty); }
            set { SetValue(ChartAreaBackgroundProperty, value); }
        }
        /// <summary>Identifies the <see cref="ChartAreaBackground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty ChartAreaBackgroundProperty =
            DependencyProperty.Register("ChartAreaBackground", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultChartAreaBackground));

        ///<summary>Gets the default value for the ChartAreaBackground property.</summary>
        ///<value>The default value for the <see cref="ChartAreaBackground"/> property: <c>#FFFFFDE9</c>.</value>
        public static Brush DefaultChartAreaBackground { get { return new SolidColorBrush(Color.FromRgb(0, 0, 0)); } } // #FFFFFDE9
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the fill brush for the rectangle, that covers this chart control if it has been disabled.</summary>
        ///<value>The fill brush for the rectangle, that covers this chart control if it has been disabled. The default is determined by the <see cref="DefaultDisabledFill"/>values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="DisabledFillProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush DisabledFill
        {
            get { return (Brush)GetValue(DisabledFillProperty); }
            set { SetValue(DisabledFillProperty, value); }
        }
        /// <summary>Identifies the <see cref="DisabledFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty DisabledFillProperty =
            DependencyProperty.Register("DisabledFill", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultDisabledFill));

        ///<summary>Gets the default value for the DisabledFill property.</summary>
        ///<value>The default value for the <see cref="DisabledFill"/> property: <c>#CCAAAAAA</c>.</value>
        public static Brush DefaultDisabledFill { get { return new SolidColorBrush(Color.FromArgb(204, 170, 170, 170)); } } // #CCAAAAAA
        //----------------------------------------------------------------------------------------------------------------------------------
        #region LEGEND PROPERTIES *******************************************************************************************************************************

        ///<summary>Gets or sets the text of the legend.</summary>
        ///<value>The text of the legend. The default is determined by the <see cref="DefaultLegendText"/> value.</value>
        ///<remarks>
        ///The legend could contain any text, describing this chart. Usually it contains a ticker symbol (a name of the security) and a timeframe, for example: <em>"AAPL"</em>, <em>"GOOGL, M5"</em>, <em>"BTC/USD, D"</em> etc.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendTextProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultLegendText">DefaultLegendText</seealso>
        public string LegendText
        {
            get { return (string)GetValue(LegendTextProperty); }
            set { SetValue(LegendTextProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendText"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendTextProperty =
            DependencyProperty.Register("LegendText", typeof(string), typeof(CandleChart), new PropertyMetadata(DefaultLegendText));

        ///<summary>Gets the default value for the LegendText property.</summary>
        ///<value>The default value for the LegendText property: <c>"DefaultLegend"</c>.</value>
        ///<seealso cref = "LegendText">LegendText</seealso>
        public static string DefaultLegendText { get { return "DefaultLegend"; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the font family of the legend.</summary>
        ///<value>The font family of the legend. The default value is equal to the default value of the <see cref="TextBlock.FontFamilyProperty">TextBlock.FontFamilyProperty</see>.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendFontFamilyProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public FontFamily LegendFontFamily
        {
            get { return (FontFamily)GetValue(LegendFontFamilyProperty); }
            set { SetValue(LegendFontFamilyProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendFontFamily"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendFontFamilyProperty =
            DependencyProperty.Register("LegendFontFamily", typeof(FontFamily), typeof(CandleChart), new PropertyMetadata(TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue));
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the font size of the legend. The legend locates inside the price chart area.</summary>
        ///<value>The font size of the legend. The default is determined by the <see cref="DefaultLegendFontSize"/> value.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendFontSizeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultLegendFontSize">DefaultLegendFontSize</seealso>
        public double LegendFontSize
        {
            get { return (double)GetValue(LegendFontSizeProperty); }
            set { SetValue(LegendFontSizeProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendFontSize">LegendFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendFontSizeProperty =
            DependencyProperty.Register("LegendFontSize", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultLegendFontSize));

        ///<summary>Gets the default value for the LegendFontSize property.</summary>
        ///<value>The default value for the LegendFontSize property: <c>30.0</c>.</value>
        ///<seealso cref = "LegendFontSize">LegendFontSize</seealso>
        public static double DefaultLegendFontSize { get { return 30.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the font style of the legend.</summary>
        ///<value>The font style of the legend. The default is determined by the <see cref="DefaultLegendFontStyle"/> value.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendFontStyleProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultLegendFontStyle">DefaultLegendFontStyle</seealso>
        public FontStyle LegendFontStyle
        {
            get { return (FontStyle)GetValue(LegendFontStyleProperty); }
            set { SetValue(LegendFontStyleProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendFontStyle"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendFontStyleProperty =
            DependencyProperty.Register("LegendFontStyle", typeof(FontStyle), typeof(CandleChart), new PropertyMetadata(DefaultLegendFontStyle));

        ///<summary>Gets the default value for the LegendFontStyle property.</summary>
        ///<value>The default value for the LegendFontStyle property: <c>FontStyles.Normal</c>.</value>
        ///<seealso cref = "LegendFontStyle">LegendFontStyle</seealso>
        public static FontStyle DefaultLegendFontStyle { get { return FontStyles.Normal; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the font weight of the legend. The legend locates inside the price chart area.</summary>
        /// <value>The font weight of the legend. The default is determined by the <see cref="DefaultLegendFontWeight"/> value.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendFontWeightProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        /// <seealso cref = "DefaultLegendFontWeight">DefaultLegendFontWeight</seealso>
        public FontWeight LegendFontWeight
        {
            get { return (FontWeight)GetValue(LegendFontWeightProperty); }
            set { SetValue(LegendFontWeightProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendFontWeight"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendFontWeightProperty =
            DependencyProperty.Register("LegendFontWeight", typeof(FontWeight), typeof(CandleChart), new PropertyMetadata(DefaultLegendFontWeight));

        ///<summary>Gets the default value for the LegendFontWeight property.</summary>
        ///<value>The default value for the LegendFontWeight property: <c>FontWeights.Bold</c>.</value>
        ///<seealso cref = "LegendFontWeight">LegendFontWeight</seealso>
        public static FontWeight DefaultLegendFontWeight { get { return FontWeights.Bold; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the foreground of the legend. The legend locates inside the price chart area.</summary>
        /// <value>The foreground of the legend. The default is determined by the <see cref="DefaultLegendForeground"/> value.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendForegroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        /// <seealso cref = "DefaultLegendForeground">DefaultLegendForeground</seealso>
        public Brush LegendForeground
        {
            get { return (Brush)GetValue(LegendForegroundProperty); }
            set { SetValue(LegendForegroundProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendForeground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendForegroundProperty =
            DependencyProperty.Register("LegendForeground", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultLegendForeground));

        ///<summary>Gets the default value for the LegendForeground property.</summary>
        ///<value>The default value for the LegendForeground property: <c>#3C000000</c>.</value>
        ///<seealso cref = "LegendForeground">LegendForeground</seealso>
        public static Brush DefaultLegendForeground { get { return new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)); } } // #3C000000
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the horizontal alignment for the legend inside the price chart area.</summary>
        /// <value>The horizontal alignment of the legend. The default is determined by the <see cref="DefaultLegendHorizontalAlignment"/> value.</value>
        ///<remarks>
            ///The legend locates inside the price chart area and could be horizontally and vertically aligned.
            ///<h3>Dependency Property Information</h3>
            ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
            ///<tr><td>Identifier field</td><td><see cref="LegendHorizontalAlignmentProperty"/></td></tr> 
            ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        /// <seealso cref = "DefaultLegendHorizontalAlignment">DefaultLegendHorizontalAlignment</seealso>
        /// <seealso cref = "LegendVerticalAlignment">LegendVerticalAlignment</seealso>
        public HorizontalAlignment LegendHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(LegendHorizontalAlignmentProperty); }
            set { SetValue(LegendHorizontalAlignmentProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendHorizontalAlignment"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendHorizontalAlignmentProperty =
            DependencyProperty.Register("LegendHorizontalAlignment", typeof(HorizontalAlignment), typeof(CandleChart), new PropertyMetadata(DefaultLegendHorizontalAlignment));

        ///<summary>Gets the default value for the LegendHorizontalAlignment property.</summary>
        ///<value>The default value for the LegendHorizontalAlignment property: <c>HorizontalAlignment.Left</c>.</value>
        ///<seealso cref = "LegendHorizontalAlignment">LegendHorizontalAlignment</seealso>
        public static HorizontalAlignment DefaultLegendHorizontalAlignment { get { return HorizontalAlignment.Left; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the vertical alignment for the legend inside the price chart area.</summary>
        /// <value>The vertical alignment of the legend. The default is determined by the <see cref="DefaultLegendVerticalAlignment"/> value.</value>
        ///<remarks>
            ///The legend locates inside the price chart area and could be horizontally and vertically aligned.
            ///<h3>Dependency Property Information</h3>
            ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
            ///<tr><td>Identifier field</td><td><see cref="LegendVerticalAlignmentProperty"/></td></tr> 
            ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        /// <seealso cref = "DefaultLegendVerticalAlignment">DefaultLegendVerticalAlignment</seealso>
        /// <seealso cref = "LegendHorizontalAlignment">LegendHorizontalAlignment</seealso>
        public VerticalAlignment LegendVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(LegendVerticalAlignmentProperty); }
            set { SetValue(LegendVerticalAlignmentProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendVerticalAlignment"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendVerticalAlignmentProperty =
            DependencyProperty.Register("LegendVerticalAlignment", typeof(VerticalAlignment), typeof(CandleChart), new PropertyMetadata(DefaultLegendVerticalAlignment));

        ///<summary>Gets the default value for the LegendVerticalAlignment property.</summary>
        ///<value>The default value for the LegendVerticalAlignment property: <c>VerticalAlignment.Bottom</c>.</value>
        ///<seealso cref = "LegendVerticalAlignment">LegendVerticalAlignment</seealso>
        public static VerticalAlignment DefaultLegendVerticalAlignment { get { return VerticalAlignment.Bottom; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the margins for the legend. The legend locates inside the price chart area.</summary>
        /// <value>The legend margin. The default is determined by the <see cref="DefaultLegendMargin"/> value.</value>
        ///<remarks>
        ///The legend locates inside the price chart area and could be horizontally and vertically aligned. It could contain any text, describing this chart. Usually it contains a ticker symbol (a name of the security) and a timeframe, for example: <em>"AAPL"</em>, <em>"GOOGL, M5"</em>, <em>"BTC/USD, D"</em> etc.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="LegendMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        /// <seealso cref = "DefaultLegendMargin">DefaultLegendMargin</seealso>
        public Thickness LegendMargin
        {
            get { return (Thickness)GetValue(LegendMarginProperty); }
            set { SetValue(LegendMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="LegendMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty LegendMarginProperty =
            DependencyProperty.Register("LegendMargin", typeof(Thickness), typeof(CandleChart), new PropertyMetadata(DefaultLegendMargin));

        ///<summary>Gets the default value for the LegendMargin property.</summary>
        ///<value>The default value for the LegendMargin property: <c>(10, 0, 10, 0)</c>.</value>
        ///<seealso cref = "LegendMargin">LegendMargin</seealso>
        public static Thickness DefaultLegendMargin { get { return new Thickness(10, 0, 10, 0); } }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region PRICE CHART PROPERTIES **************************************************************************************************************************

        /// <summary>Gets or sets the top margin for the price chart.</summary>
        ///<value>The top margin of the price chart, in device-independent units. The default is determined by the <see cref="DefaultPriceChartTopMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the price chart inside its area by setting the <see cref="PriceChartTopMargin"/> and <see cref="PriceChartBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="PriceChartTopMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultPriceChartTopMargin">DefaultPriceChartTopMargin</seealso>
        ///<seealso cref = "PriceChartBottomMargin">PriceChartBottomMargin</seealso>
        public double PriceChartTopMargin
        {
            get { return (double)GetValue(PriceChartTopMarginProperty); }
            set { SetValue(PriceChartTopMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="PriceChartTopMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty PriceChartTopMarginProperty =
            DependencyProperty.Register("PriceChartTopMargin", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultPriceChartTopMargin));

        ///<summary>Gets the default value for the PriceChartTopMargin property.</summary>
        ///<value>The default value for the PriceChartTopMargin property: <c>15.0</c>.</value>
        ///<seealso cref = "PriceChartTopMargin">PriceChartTopMargin</seealso>
        public static double DefaultPriceChartTopMargin { get { return 15.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the bottom margin for the price chart.</summary>
        ///<value>The bottom margin of the price chart, in device-independent units. The default is determined by the <see cref="DefaultPriceChartBottomMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the price chart inside its area by setting the <see cref="PriceChartTopMargin"/> and <see cref="PriceChartBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="PriceChartBottomMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultPriceChartBottomMargin">DefaultPriceChartBottomMargin</seealso>
        ///<seealso cref = "PriceChartTopMargin">PriceChartTopMargin</seealso>
        public double PriceChartBottomMargin
        {
            get { return (double)GetValue(PriceChartBottomMarginProperty); }
            set { SetValue(PriceChartBottomMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="PriceChartBottomMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty PriceChartBottomMarginProperty =
            DependencyProperty.Register("PriceChartBottomMargin", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultPriceChartBottomMargin));

        ///<summary>Gets the default value for the PriceChartBottomMargin property.</summary>
        ///<value>The default value for the PriceChartBottomMargin property: <c>15.0</c>.</value>
        ///<seealso cref = "PriceChartBottomMargin">PriceChartBottomMargin</seealso>
        public static double DefaultPriceChartBottomMargin { get { return 15.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bullish candle (when the Close is higher than the Open).</summary>
        ///<value>The brush to draw and fill all bullish candles. The default is determined by the <see cref="DefaultBullishCandleBrush"/> value.</value>
        ///<remarks> 
        /// We separate all candles into two types - bullish and bearish. The Bullish candle has its Close higher than its Open. The Bearish candle has its Close lower than its Open. To visualize that separation usually candles are painted into two different colors - 
        /// <see cref="BullishCandleBrush"/> and <see cref="BearishCandleBrush"/> for bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BullishCandleBrushProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush BullishCandleBrush
        {
            get { return (Brush)GetValue(BullishCandleBrushProperty); }
            set { SetValue(BullishCandleBrushProperty, value); }
        }
        /// <summary>Identifies the <see cref="BullishCandleBrush"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BullishCandleBrushProperty =
            DependencyProperty.Register("BullishCandleBrush", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultBullishCandleBrush));

        ///<summary>Gets the default value for the BullishCandleBrush property.</summary>
        ///<value>The default value for the BullishCandleBrush property: <c>Brushes.Green</c>.</value>
        ///<seealso cref = "BullishCandleBrush">BullishCandleBrush</seealso>
        public static Brush DefaultBullishCandleBrush => Config.UpBrush;
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bearish candle (when the Close is lower than the Open).</summary>
        ///<value>The brush to draw and fill all bearish candles. The default is determined by the <see cref="DefaultBearishCandleBrush"/> value.</value>
        ///<remarks> 
        /// We separate all candles into two types - bullish and bearish. The Bullish candle has its Close higher than its Open. The Bearish candle has its Close lower than its Open. To visualize that separation usually candles are painted into two different colors - 
        /// <see cref="BullishCandleBrush"/> and <see cref="BearishCandleBrush"/> for bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BearishCandleBrushProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush BearishCandleBrush
        {
            get { return (Brush)GetValue(BearishCandleBrushProperty); }
            set { SetValue(BearishCandleBrushProperty, value); }
        }
        /// <summary>Identifies the <see cref="BearishCandleBrush"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BearishCandleBrushProperty =
            DependencyProperty.Register("BearishCandleBrush", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultBearishCandleBrush));

        ///<summary>Gets the default value for the BearishCandleBrush property.</summary>
        ///<value>The default value for the BearishCandleBrush property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BearishCandleBrush">BearishCandleBrush</seealso>
        public static Brush DefaultBearishCandleBrush => Config.DownBrush;
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the initial gap between adjacent candles.</summary>
        ///<value>The initial width of the horizontal gap between adjacent candles, in device-independent units (1/96th inch per unit). 
        ///The default is determined by the <see cref="DefaultInitialGapBetweenCandles"/> value.</value>
        ///<remarks>Initially the gap between candles <see cref="GapBetweenCandles"/> is equal to this property value, but then the <see cref="GapBetweenCandles"/> property value changes due to user's manipulations.</remarks>
        public double InitialGapBetweenCandles { get; set; }

        ///<summary>Gets the default value for the InitialGapBetweenCandles property.</summary>
        ///<value>The default value for the <see cref="InitialGapBetweenCandles"/> property, in device-independent units: <c>1.0</c>.</value>
        ///<seealso cref = "GapBetweenCandles">GapBetweenCandles</seealso>
        public static double DefaultInitialGapBetweenCandles { get { return 1.0; } }

        double gapBetweenCandles;
        /// <summary>Gets the horizontal gap width between adjacent candles.</summary>
        ///<value>The width of the horizontal gap between adjacent candles, in device-independent units (1/96th inch per unit).</value>
        ///<remarks>Initially after loading this property value is equal to the <see cref="InitialGapBetweenCandles"/>, but then it changes due to user's manipulations.</remarks>
        ///<seealso cref = "InitialGapBetweenCandles">InitialGapBetweenCandles</seealso>
        ///<seealso cref = "DefaultInitialGapBetweenCandles">DefaultInitialGapBetweenCandles</seealso>
        public double GapBetweenCandles
        {
            get { return gapBetweenCandles; }
            private set
            {
                if (value == gapBetweenCandles) return;
                gapBetweenCandles = value;
                OnPropertyChanged();
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the initial candle width.</summary>
        ///<value>The initial width of the candle, in device-independent units (1/96th inch per unit). 
        ///The default is determined by the <see cref="DefaultInitialCandleWidth"/> value.</value>
        ///<remarks>Initially the width of a candle <see cref="CandleWidth"/> is equal to this property value, but then the <see cref="CandleWidth"/> property value changes due to user's manipulations.</remarks>
        public double InitialCandleWidth { get; set; }

        ///<summary>Gets the default value for the InitialCandleWidth property.</summary>
        ///<value>The default value for the <see cref="InitialCandleWidth"/> property, in device-independent units: <c>3.0</c>.</value>
        ///<seealso cref = "GapBetweenCandles">GapBetweenCandles</seealso>
        public static double DefaultInitialCandleWidth { get { return 3.0; } }

        double candleWidth;
        ///<summary>Gets the width of the candle.</summary>
        ///<value>The width of the candle, in device-independent units (1/96th inch per unit).</value>
        ///<remarks>Initially after loading this property value is equal to the <see cref="InitialCandleWidth"/>, but then it changes due to user's manipulations.</remarks>
        ///<seealso cref = "InitialCandleWidth">InitialCandleWidth</seealso>
        ///<seealso cref = "DefaultInitialCandleWidth">DefaultInitialCandleWidth</seealso>
        public double CandleWidth
        {
            get { return candleWidth; }
            private set
            {
                if (value == candleWidth) return;
                candleWidth = value;
                OnPropertyChanged();
            }
        }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region PORTFOLIO PROPERTIES
        
        /// <summary>
        /// Whether the portfolio panel is visible or not
        /// </summary>
        public bool IsPortfolioPanelVisible
        {
            get { return (bool) GetValue(IsPortfolioPanelVisibleProperty); }
            set
            {
                SetValue(IsPortfolioPanelVisibleProperty, value);
                UpdatePanelRowDefinitions(3, 4, IsPortfolioPanelVisible);
            }
        }

        public static readonly DependencyProperty IsPortfolioPanelVisibleProperty =
            DependencyProperty.Register(nameof(IsPortfolioPanelVisible), typeof(bool), typeof(CandleChart), new PropertyMetadata(false));

        private void UpdatePanelRowDefinitions(int row1, int row2, bool value)
        {
            ContainerGrid.RowDefinitions[row1].Height = value ? GridLength.Auto : new GridLength(0);
            ContainerGrid.RowDefinitions[row2].Height = value ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            RefreshRowSpans();
        }

        #endregion

        private void RefreshRowSpans()
        {
            var span = 3;
            if (IsPortfolioPanelVisible && IsVolumePanelVisible)
                span = 1;
            else if ((IsPortfolioPanelVisible && !IsVolumePanelVisible) || (!IsPortfolioPanelVisible && IsVolumePanelVisible))
                span = 2;
            
            var els = new FrameworkElement[] { PriceChartBorder, priceChartContainer, verticalPriceTicksElement, priceChartCrosshairBorder };
            foreach (var el in els)
                Grid.SetRowSpan(el, span);
        }



        #region VOLUME HISTOGRAM PROPERTIES *********************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility for the volume histogram panel.</summary>
        ///<value>The boolean value that means whether the volume histogram panel is visible or not. The default is determined by the <see cref="DefaultIsVolumePanelVisible"/> value.</value>
        ///<remarks> 
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsVolumePanelVisibleProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public bool IsVolumePanelVisible
        {
            get { return (bool)GetValue(IsVolumePanelVisibleProperty); }
            set
            {
                SetValue(IsVolumePanelVisibleProperty, value);
                UpdatePanelRowDefinitions(1, 2, IsVolumePanelVisible);
            }
        }
        /// <summary>Identifies the <see cref="IsVolumePanelVisible"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsVolumePanelVisibleProperty =
            DependencyProperty.Register("IsVolumePanelVisible", typeof(bool), typeof(CandleChart), new PropertyMetadata(DefaultIsVolumePanelVisible));

        ///<summary>Gets the default value for the IsVolumePanelVisible property.</summary>
        ///<value>The default value for the <see cref="IsVolumePanelVisible"/> property: <c>True</c>.</value>
        public static bool DefaultIsVolumePanelVisible { get { return false; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the volume bar width to the candle width ratio that eventually defines the width of the volume bar.</summary>
        ///<value>The ratio of the volume bar width to the candle width. The default is determined by the <see cref="DefaultVolumeBarWidthToCandleWidthRatio"/> value.</value>
        ///<remarks> 
        ///We define the width of the volume bar as a variable that is dependent on the candle width as follows:
        ///<p style="margin: 0 0 0 20"><em>Volume bar width</em> = <see cref="VolumeBarWidthToCandleWidthRatio"/> * <see cref="CandleWidth"/></p>
        ///The value of this property must be in the range [0, 1]. If the value of this property is zero then the volume bar width will be 1.0 in device-independent units, irrespective of the candle width.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VolumeBarWidthToCandleWidthRatioProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double VolumeBarWidthToCandleWidthRatio
        {
            get { return (double)GetValue(VolumeBarWidthToCandleWidthRatioProperty); }
            set { SetValue(VolumeBarWidthToCandleWidthRatioProperty, value); }
        }
        /// <summary>Identifies the <see cref="VolumeBarWidthToCandleWidthRatio"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VolumeBarWidthToCandleWidthRatioProperty =
            DependencyProperty.Register("VolumeBarWidthToCandleWidthRatio", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultVolumeBarWidthToCandleWidthRatio, null, CoerceVolumeBarWidthToCandleWidthRatio));

        private static object CoerceVolumeBarWidthToCandleWidthRatio(DependencyObject objWithOldDP, object newDPValue)
        {
            //CandleChart thisCandleChart = (CandleChart)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            double newValue = (double)newDPValue;
            return Math.Min(1.0, Math.Max(0.0, newValue));
        }

        ///<summary>Gets the default value for the VolumeBarWidthToCandleWidthRatio property.</summary>
        ///<value>The default value for the <see cref="VolumeBarWidthToCandleWidthRatio"/> property: <c>0.3</c>.</value>
        ///<seealso cref = "VolumeBarWidthToCandleWidthRatio">VolumeBarWidthToCandleWidthRatio</seealso>
        public static double DefaultVolumeBarWidthToCandleWidthRatio { get { return 0.3; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the top margin for the volume histogram.</summary>
        ///<value>The top margin of the volume histogram, in device-independent units. The default is determined by the <see cref="DefaultVolumeHistogramTopMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the volume histogram inside its area by setting the <see cref="VolumeHistogramTopMargin"/> and <see cref="VolumeHistogramBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VolumeHistogramTopMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double VolumeHistogramTopMargin
        {
            get { return (double)GetValue(VolumeHistogramTopMarginProperty); }
            set { SetValue(VolumeHistogramTopMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="VolumeHistogramTopMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VolumeHistogramTopMarginProperty =
            DependencyProperty.Register("VolumeHistogramTopMargin", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultVolumeHistogramTopMargin));

        ///<summary>Gets the default value for VolumeHistogramTopMargin property.</summary>
        ///<value>The default value for the <see cref="VolumeHistogramTopMargin"/> property, in device-independent units: <c>10.0</c>.</value>
        public static double DefaultVolumeHistogramTopMargin { get { return 10.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the bottom margin for the volume histogram.</summary>
        ///<value>The bottom margin of the volume histogram, in device-independent units. The default is determined by the <see cref="DefaultVolumeHistogramBottomMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the volume histogram inside its area by setting the <see cref="VolumeHistogramTopMargin"/> and <see cref="VolumeHistogramBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VolumeHistogramBottomMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double VolumeHistogramBottomMargin
        {
            get { return (double)GetValue(VolumeHistogramBottomMarginProperty); }
            set { SetValue(VolumeHistogramBottomMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="VolumeHistogramBottomMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VolumeHistogramBottomMarginProperty =
            DependencyProperty.Register("VolumeHistogramBottomMargin", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultVolumeHistogramBottomMargin));

        ///<summary>Gets the default value for VolumeHistogramBottomMargin property.</summary>
        ///<value>The default value for the <see cref="VolumeHistogramBottomMargin"/> property, in device-independent units: <c>5.0</c>.</value>
        public static double DefaultVolumeHistogramBottomMargin { get { return 5.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bullish volume bar.</summary>
        ///<value>The brush to fill all bullish volume bars. The default is determined by the <see cref="DefaultBullishVolumeBarBrush"/> value.</value>
        ///<remarks> 
        /// We separate all volume bars to "bullish" or "bearish" according to whether the correspondent candle is bullish or bearish. A candle is bullish if its Close higher than its Open. A candle is Bearish if its Close lower than its Open. To visualize such a separation all bars are painted into two different colors - 
        /// <see cref="BullishVolumeBarBrush"/> and <see cref="BearishVolumeBarBrush"/> for bullish and bearish bars respectively. Likewise you can set the <see cref="BullishCandleBrush"/> and <see cref="BearishCandleBrush"/> properties to change the appearance of bullish and bearish price candles.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BullishVolumeBarBrushProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush BullishVolumeBarBrush
        {
            get { return (Brush)GetValue(BullishVolumeBarBrushProperty); }
            set { SetValue(BullishVolumeBarBrushProperty, value); }
        }
        /// <summary>Identifies the <see cref="BullishVolumeBarBrush"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BullishVolumeBarBrushProperty =
            DependencyProperty.Register("BullishVolumeBarBrush", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultBullishVolumeBarBrush));

        ///<summary>Gets the default value for the BullishVolumeBarBrush property.</summary>
        ///<value>The default value for the BullishVolumeBarBrush property: <c>Brushes.Green</c>.</value>
        ///<seealso cref = "BullishVolumeBarBrush">BullishVolumeBarBrush</seealso>
        public static Brush DefaultBullishVolumeBarBrush { get { return Brushes.Green; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bearish volume bar.</summary>
        ///<value>The brush to fill all bearish volume bars. The default is determined by the <see cref="DefaultBearishVolumeBarBrush"/> value.</value>
        ///<remarks> 
        /// We separate all volume bars to "bullish" or "bearish" according to whether the correspondent candle is bullish or bearish. The Bullish candle has its Close higher than its Open. The Bearish candle has its Close lower than its Open. To visualize such a separation all bars are painted into two different colors - 
        /// <see cref="BullishVolumeBarBrush"/> and <see cref="BearishVolumeBarBrush"/> for bullish and bearish bars respectively. Likewise you can set the <see cref="BullishCandleBrush"/> and <see cref="BearishCandleBrush"/> properties to change the appearance of bullish and bearish price candles.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BearishVolumeBarBrushProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush BearishVolumeBarBrush
        {
            get { return (Brush)GetValue(BearishVolumeBarBrushProperty); }
            set { SetValue(BearishVolumeBarBrushProperty, value); }
        }
        /// <summary>Identifies the <see cref="BearishVolumeBarBrush"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BearishVolumeBarBrushProperty =
            DependencyProperty.Register("BearishVolumeBarBrush", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultBearishVolumeBarBrush));

        ///<summary>Gets the default value for the BearishVolumeBarBrush property.</summary>
        ///<value>The default value for the BearishVolumeBarBrush property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BearishVolumeBarBrush">BearishVolumeBarBrush</seealso>
        public static Brush DefaultBearishVolumeBarBrush { get { return Brushes.Red; } }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region COMMON PROPERTIES FOR THE PRICE AXIS AND THE TIME AXIS*******************************************************************************************
        ///<summary>Gets or sets a color of lines, ticks and its labels for the time axis, the price axis and the volume axis.</summary>
        ///<value>The color of lines, ticks and its labels for the time axis, the price axis and the volume axis. The default is determined by the <see cref="DefaultAxisTickColor"/> value.</value>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="AxisTickColorProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush AxisTickColor
        {
            get { return (Brush)GetValue(AxisTickColorProperty); }
            set { SetValue(AxisTickColorProperty, value); }
        }
        /// <summary>Identifies the <see cref="AxisTickColor">AxisTickColor</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty AxisTickColorProperty =
            DependencyProperty.RegisterAttached("AxisTickColor", typeof(Brush), typeof(CandleChart), new FrameworkPropertyMetadata(DefaultAxisTickColor, FrameworkPropertyMetadataOptions.Inherits));

        ///<summary>Gets the default value for the <see cref="AxisTickColor">AxisTickColor</see> property.</summary>
        ///<value>The default value for the <see cref="AxisTickColor"/> property: <c>Brushes.Black</c>.</value>
        public static Brush DefaultAxisTickColor { get { return new SolidColorBrush(Color.FromRgb(178, 181, 190)); } }
        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region PROPERTIES OF THE PRICE AXIS (AND OF THE VOLUME AXIS, WHICH DOESN'T HAVE ITS OWN PROPERTIES) ****************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the font size of the tick labels for the price and volume axis.</summary>
        ///<value>The font size of the tick labels for the price and volume axis. The default is determined by the <see cref="DefaultPriceTickFontSize"/> value.</value>
        ///<remarks>
        /// The volume axis doesn't have its own appearance properties. Therefore, the volume axis appearance depends on price axis properties. 
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="PriceTickFontSizeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double PriceTickFontSize
        {
            get { return (double)GetValue(PriceTickFontSizeProperty); }
            set { SetValue(PriceTickFontSizeProperty, value); }
        }
        /// <summary>Identifies the <see cref="PriceTickFontSize">PriceTickFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty PriceTickFontSizeProperty =
            DependencyProperty.RegisterAttached("PriceTickFontSize", typeof(double), typeof(CandleChart), new FrameworkPropertyMetadata(DefaultPriceTickFontSize, FrameworkPropertyMetadataOptions.Inherits, OnPriceTickFontSizeChanged));
        static void OnPriceTickFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CandleChart thisCandleChart = obj as CandleChart;
            thisCandleChart?.OnPropertyChanged("PriceAxisWidth");
            thisCandleChart?.OnPropertyChanged("PriceTickTextHeight");
        }

        ///<summary>Gets the default value for the <see cref="PriceTickFontSize">PriceTickFontSize</see> property.</summary>
        ///<value>The default value for the <see cref="PriceTickFontSize"/> property: <c>11.0</c>.</value>
        public static double DefaultPriceTickFontSize { get { return 11.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets the width of the price and volume axis area.</summary>
        ///<value>The width of the price and volume axis area, which contains the ticks and its labels.</value>
        ///<remarks>
        /// The volume axis area has the same width as the price axis area.
        ///</remarks>
        public double PriceAxisWidth
        {
            get
            {
                double priceTextWidth = (new FormattedText(new string('A', MaxNumberOfCharsInPrice), CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), PriceTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)).Width;
                return priceTextWidth + PriceTicksElement.TICK_LINE_WIDTH + PriceTicksElement.TICK_LEFT_MARGIN + PriceTicksElement.TICK_RIGHT_MARGIN;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets the height of the price or volume tick label.</summary>
        ///<value>The height of the price or volume tick label. This value is equals to the height of the text of the label.</value>
        ///<remarks>
        /// The volume tick label has the same height as the price tick label.
        ///</remarks>
        public double PriceTickTextHeight
        {
            get
            {
                return (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), PriceTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the minimal gap between the adjacent price or volume tick labels.</summary>
        ///<value>The minimal gap between adjacent labels for the price and volume axis. It must be non-negative value. The default is determined by the <see cref="DefaultGapBetweenPriceTickLabels"/> value.</value>
        ///<remarks>
        ///<para>This property regulates the density of the tick labels inside the price or volume axis area. The higher the <see cref="GapBetweenPriceTickLabels"/>, the less close adjacent labels are located.</para>
        ///<para>The volume axis doesn't have its own appearance properties. Therefore, the volume axis appearance depends on price axis properties.</para>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="GapBetweenPriceTickLabelsProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double GapBetweenPriceTickLabels
        {
            get { return (double)GetValue(GapBetweenPriceTickLabelsProperty); }
            set { SetValue(GapBetweenPriceTickLabelsProperty, value); }
        }
        /// <summary>Identifies the <see cref="GapBetweenPriceTickLabels">PriceTickFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty GapBetweenPriceTickLabelsProperty =
            DependencyProperty.RegisterAttached("GapBetweenPriceTickLabels", typeof(double), typeof(CandleChart), new FrameworkPropertyMetadata(DefaultGapBetweenPriceTickLabels, FrameworkPropertyMetadataOptions.Inherits));

        ///<summary>Gets the default value for the <see cref="GapBetweenPriceTickLabels">GapBetweenPriceTickLabels</see> property.</summary>
        ///<value>The default value for the <see cref="GapBetweenPriceTickLabels"/> property: <c>0.0</c>.</value>
        public static double DefaultGapBetweenPriceTickLabels { get { return 0.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        int maxNumberOfCharsInPrice = 0;
        /// <summary>Gets the maximal number of chars in a price for the current candle collection.</summary>
        ///<value>The maximal number of chars in a price for the current candle collection.</value>
        ///<remarks>
        ///This value is recalculated every time the candle collection source is changed, and remains constant until next change.
        ///</remarks>
        public int MaxNumberOfCharsInPrice
        {
            get { return maxNumberOfCharsInPrice; }
            private set
            {
                if (value == maxNumberOfCharsInPrice) return;
                maxNumberOfCharsInPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("PriceAxisWidth");
            }
        }

        // Просматривает CandlesSource и пересчитывает maxNumberOfCharsInPrice
        void ReCalc_MaxNumberOfCharsInPrice()
        {
            if (CandlesSource == null) return;
            int charsInPrice = CandlesSource.Select(c => c.H.ToString().Length).Max();
            int charsInVolume = IsVolumePanelVisible ? CandlesSource.Select(c => c.V.ToString().Length).Max() : 0;
            MaxNumberOfCharsInPrice = Math.Max(charsInPrice, charsInVolume);
        }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region PROPERTIES OF THE TIME AXIS *********************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the font size of the tick labels for the time axis.</summary>
        ///<value>The font size of the tick labels for the time (and date) axis. The default is determined by the <see cref="DefaultTimeTickFontSize"/> value.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="TimeTickFontSizeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double TimeTickFontSize
        {
            get { return (double)GetValue(TimeTickFontSizeProperty); }
            set { SetValue(TimeTickFontSizeProperty, value); }
        }
        /// <summary>Identifies the <see cref="TimeTickFontSize">TimeTickFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty TimeTickFontSizeProperty =
            DependencyProperty.RegisterAttached("TimeTickFontSize", typeof(double), typeof(CandleChart), new FrameworkPropertyMetadata(DefaultTimeTickFontSize, FrameworkPropertyMetadataOptions.Inherits, OnTimeTickFontSizeChanged));
        static void OnTimeTickFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CandleChart thisCandleChart = obj as CandleChart;
            thisCandleChart?.OnPropertyChanged("TimeAxisHeight");
        }

        ///<summary>Gets the default value for the <see cref="TimeTickFontSize">TimeTickFontSize</see> property.</summary>
        ///<value>The default value for the <see cref="TimeTickFontSize"/> property: <c>10.0</c>.</value>
        public static double DefaultTimeTickFontSize { get { return 10.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets the height of the time axis area.</summary>
        ///<value>The height of the time axis area, which contains the time and date ticks with its labels.</value>
        public double TimeAxisHeight
        {
            get
            {
                double timeTextHeight = (new FormattedText("1Ajl", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), TimeTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
                return 2 * timeTextHeight + 4.0;
            }
        }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region GRIDLINES PROPERTIES ****************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the pen for the horizontal gridlines.</summary>
        ///<value>The pen for the horizontal gridlines. The default is determined by the <see cref="DefaultHorizontalGridlinesBrush"/> and <see cref="DefaultHorizontalGridlinesThickness"/> values.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="HorizontalGridlinesPenProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Pen HorizontalGridlinesPen
        {
            get { return (Pen)GetValue(HorizontalGridlinesPenProperty); }
            set { SetValue(HorizontalGridlinesPenProperty, value); }
        }
        /// <summary>Identifies the <see cref="HorizontalGridlinesPen"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty HorizontalGridlinesPenProperty =
            DependencyProperty.Register("HorizontalGridlinesPen", typeof(Pen), typeof(CandleChart), new PropertyMetadata(new Pen(DefaultHorizontalGridlinesBrush, DefaultHorizontalGridlinesThickness)));

        ///<summary>Gets the default value for the Brush constituent of the HorizontalGridlinesPen property.</summary>
        ///<value>The default value for the <see cref="Brush"/> constituent of the <see cref="HorizontalGridlinesPen"/> property: <c>#1E000000</c>.</value>
        ///<seealso cref = "DefaultHorizontalGridlinesThickness">DefaultHorizontalGridlinesThickness</seealso>
        public static Brush DefaultHorizontalGridlinesBrush { get { return new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)); } } // #1E000000

        ///<summary>Gets the default value for Thickness constituent of the HorizontalGridlinesPen property.</summary>
        ///<value>The default value for the Thickness constituent of the <see cref="HorizontalGridlinesPen"/> property: <c>1.0</c>.</value>
        ///<seealso cref = "DefaultHorizontalGridlinesBrush">DefaultHorizontalGridlinesBrush</seealso>
        public static double DefaultHorizontalGridlinesThickness { get { return 1.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the pen for the vertical gridlines.</summary>
        ///<value>The pen for the vertical gridlines. The default is determined by the <see cref="DefaultVerticalGridlinesBrush"/> and <see cref="DefaultVerticalGridlinesThickness"/> values.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VerticalGridlinesPenProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Pen VerticalGridlinesPen
        {
            get { return (Pen)GetValue(VerticalGridlinesPenProperty); }
            set { SetValue(VerticalGridlinesPenProperty, value); }
        }
        /// <summary>Identifies the <see cref="VerticalGridlinesPen"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VerticalGridlinesPenProperty =
            DependencyProperty.Register("VerticalGridlinesPen", typeof(Pen), typeof(CandleChart),
                new PropertyMetadata(new Pen(DefaultVerticalGridlinesBrush, DefaultVerticalGridlinesThickness)));

        ///<summary>Gets the default value for the Brush constituent of the VerticalGridlinesPen property.</summary>
        ///<value>The default value for the <see cref="Brush"/> constituent of the <see cref="VerticalGridlinesPen"/> property: <c>#1E000000</c>.</value>
        ///<seealso cref = "DefaultVerticalGridlinesThickness">DefaultVerticalGridlinesThickness</seealso>
        public static Brush DefaultVerticalGridlinesBrush { get { return new SolidColorBrush(Color.FromArgb(50, 105, 42, 0)); } } // #32692A00

        ///<summary>Gets the default value for Thickness constituent of the VerticalGridlinesPen property.</summary>
        ///<value>The default value for the Thickness constituent of the <see cref="VerticalGridlinesPen"/> property: <c>1.0</c>.</value>
        ///<seealso cref = "DefaultVerticalGridlinesBrush">DefaultVerticalGridlinesBrush</seealso>
        public static double DefaultVerticalGridlinesThickness { get { return 1.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the visibility of the horizontal gridlines.</summary>
        ///<value>The visibility of the horizontal gridlines: Visible if <c>True</c>; Hidden if <c>False</c>. The default is determined by the <see cref="DefaultIsHorizontalGridlinesEnabled"/> values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsHorizontalGridlinesEnabledProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "IsVerticalGridlinesEnabled">IsHorizontalGridlinesEnabled</seealso>
        public bool IsHorizontalGridlinesEnabled
        {
            get { return (bool)GetValue(IsHorizontalGridlinesEnabledProperty); }
            set { SetValue(IsHorizontalGridlinesEnabledProperty, value); }
        }
        /// <summary>Identifies the <see cref="IsHorizontalGridlinesEnabled"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsHorizontalGridlinesEnabledProperty =
            DependencyProperty.Register("IsHorizontalGridlinesEnabled", typeof(bool), typeof(CandleChart), new PropertyMetadata(DefaultIsHorizontalGridlinesEnabled));

        ///<summary>Gets the default value for the IsHorizontalGridlinesEnabled property.</summary>
        ///<value>The default value for the <see cref="IsHorizontalGridlinesEnabled"/> property: <c>True</c>.</value>
        public static bool DefaultIsHorizontalGridlinesEnabled { get { return true; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility of the vertical gridlines.</summary>
        ///<value>The visibility of the vertical gridlines: Visible if <c>True</c>; Hidden if <c>False</c>. The default is determined by the <see cref="DefaultIsVerticalGridlinesEnabled"/> values.</value>
        ///<remarks>
        ///This property applies to all vertical gridlines, which are showed for all ticks of the time axis. But sometimes you don't need to show all of this gridlines and want to visualize lines only for the most round time and date values. 
        ///In that case you need to set both the <see cref="IsVerticalGridlinesEnabled"/> and the <see cref="HideMinorVerticalGridlines"/> properties to <c>True</c>.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsVerticalGridlinesEnabledProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "IsHorizontalGridlinesEnabled">IsHorizontalGridlinesEnabled</seealso>
        public bool IsVerticalGridlinesEnabled
        {
            get { return (bool)GetValue(IsVerticalGridlinesEnabledProperty); }
            set { SetValue(IsVerticalGridlinesEnabledProperty, value); }
        }
        /// <summary>Identifies the <see cref="IsVerticalGridlinesEnabled"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsVerticalGridlinesEnabledProperty =
            DependencyProperty.Register("IsVerticalGridlinesEnabled", typeof(bool), typeof(CandleChart), new PropertyMetadata(DefaultIsVerticalGridlinesEnabled));

        ///<summary>Gets the default value for the IsVerticalGridlinesEnabled property.</summary>
        ///<value>The default value for the <see cref="IsVerticalGridlinesEnabled"/> property: <c>True</c>.</value>
        public static bool DefaultIsVerticalGridlinesEnabled { get { return true; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility of the minor vertical gridlines.</summary>
        ///<value>The visibility of the vertical gridlines for minor (not "round") time ticks: Visible if <c>False</c>; Hidden if <c>True</c>. The default is determined by the <see cref="DefaultHideMinorVerticalGridlines"/>values.</value>
        ///<remarks>
        ///Sometimes you need to show gridlines only for the most round time or date values, and hide other minor gridlines.
        ///In that case you need to set both the <see cref="IsVerticalGridlinesEnabled"/> and the <see cref="HideMinorVerticalGridlines"/> properties to <c>True</c>.
        ///Whether the particular timetick value is Minor or not depends on the current timeframe. The common rule is: round time or date values are Major, others are Minor.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="HideMinorVerticalGridlinesProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "IsVerticalGridlinesEnabled">IsHorizontalGridlinesEnabled</seealso>
        public bool HideMinorVerticalGridlines
        {
            get { return (bool)GetValue(HideMinorVerticalGridlinesProperty); }
            set { SetValue(HideMinorVerticalGridlinesProperty, value); }
        }
        /// <summary>Identifies the <see cref="HideMinorVerticalGridlines"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty HideMinorVerticalGridlinesProperty =
            DependencyProperty.Register("HideMinorVerticalGridlines", typeof(bool), typeof(CandleChart), new PropertyMetadata(DefaultHideMinorVerticalGridlines));

        ///<summary>Gets the default value for the HideMinorVerticalGridlines property.</summary>
        ///<value>The default value for the <see cref="HideMinorVerticalGridlines"/> property: <c>False</c>.</value>
        public static bool DefaultHideMinorVerticalGridlines { get { return false; } }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region PROPERTIES OF THE CROSS *************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the brush for the cross lines.</summary>
        ///<value>The brush for the cross lines. The default is determined by the <see cref="DefaultCrossLinesBrush"/>values.</value>
        ///<remarks>
        ///The Cross lines always have a thickness of 1.0.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="CrossLinesBrushProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush CrossLinesBrush
        {
            get { return (Brush)GetValue(CrossLinesBrushProperty); }
            set { SetValue(CrossLinesBrushProperty, value); }
        }
        /// <summary>Identifies the <see cref="CrossLinesBrush"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CrossLinesBrushProperty =
            DependencyProperty.Register("CrossLinesBrush", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultCrossLinesBrush) );

        ///<summary>Gets the default value for the CrossLinesBrush property.</summary>
        ///<value>The default value for the <see cref="CrossLinesBrush"/> property: <c>#1E000A97</c>.</value>
        public static Brush DefaultCrossLinesBrush { get { return DefaultAxisTickColor; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the dash array for the cross lines.</summary>
        ///<value>The dash array for the cross lines. The default is determined by the <see cref="DefaultCrossLinesDashArray"/>values.</value>
        public DoubleCollection CrossLinesDashArray
        {
            get { return (DoubleCollection) GetValue(CrossLinesBrushProperty); }
            set { SetValue(CrossLinesBrushProperty, value); }
        }

        /// <summary>Identifies the <see cref="CrossLinesDashArray"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CrossLinesDashArrayProperty = DependencyProperty.Register("CrossLinesDashArray", typeof(DoubleCollection), typeof(CandleChart), new PropertyMetadata(DefaultCrossLinesDashArray));

        ///<summary>Gets the default value for the CrossLinesDashArray property.</summary>
        ///<value>The default value for the <see cref="CrossLinesDashArray"/> propert.</value>
        public static DoubleCollection DefaultCrossLinesDashArray { get { return new DoubleCollection(new List<double> { 6, 4 }); } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility of the cross lines.</summary>
        ///<value>The visibility of the crosslines: Visible if <c>True</c>; Hidden if <c>False</c>. The default is determined by the <see cref="DefaultIsCrossLinesVisible"/> values.</value>
        ///<remarks>
        ///The cross lines locates inside the price chart (or volume histogram) area and pass through the current mouse position. 
        ///You can separately set up the visibility for the cross lines and for the correspondent price (or volume) label by setting the 
        ///<see cref="IsCrossLinesVisible"/> and <see cref="IsCrossPriceVisible"/> properties respectively.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsCrossLinesVisibleProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public bool IsCrossLinesVisible
        {
            get { return (bool)GetValue(IsCrossLinesVisibleProperty); }
            set { SetValue(IsCrossLinesVisibleProperty, value); }
        }
        /// <summary>Identifies the <see cref="IsCrossLinesVisible"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsCrossLinesVisibleProperty =
            DependencyProperty.Register("IsCrossLinesVisible", typeof(bool), typeof(CandleChart), new PropertyMetadata(DefaultIsCrossLinesVisible));

        ///<summary>Gets the default value for the IsCrossLinesVisible property.</summary>
        ///<value>The default value for the <see cref="IsCrossLinesVisible"/> property: <c>true</c>.</value>
        public static bool DefaultIsCrossLinesVisible { get { return true; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility of the cross price (or volume) label.</summary>
        ///<value>The visibility of the cross price (or volume) label: Visible if <c>True</c>; Hidden if <c>False</c>. The default is determined by the <see cref="DefaultIsCrossPriceVisible"/> values.</value>
        ///<remarks>
        ///The cross price (or volume) label locates inside the price (or volume) axis area. 
        ///You can separately set up the visibility for the cross lines and for the correspondent price (or volume) label by setting the 
        ///<see cref="IsCrossLinesVisible"/> and <see cref="IsCrossPriceVisible"/> properties respectively.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="DefaultIsCrossPriceVisible"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public bool IsCrossPriceVisible
        {
            get { return (bool)GetValue(IsCrossPriceVisibleProperty); }
            set { SetValue(IsCrossPriceVisibleProperty, value); }
        }
        /// <summary>Identifies the <see cref="IsCrossPriceVisible"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsCrossPriceVisibleProperty =
            DependencyProperty.Register("IsCrossPriceVisible", typeof(bool), typeof(CandleChart), new PropertyMetadata(DefaultIsCrossPriceVisible));

        ///<summary>Gets the default value for the IsCrossLinesVisible property.</summary>
        ///<value>The default value for the <see cref="IsCrossLinesVisible"/> property: <c>true</c>.</value>
        public static bool DefaultIsCrossPriceVisible { get { return true; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the foreground for the price (or volume) label of the cross.</summary>
        ///<value>The foreground for the price or volume label of the cross. The default is determined by the <see cref="DefaultCrossPriceForeground"/>values.</value>
        ///<remarks>
        ///The price (or volume) value label locates on the price (or volume) axis area.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="CrossPriceForegroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush CrossPriceForeground
        {
            get { return (Brush)GetValue(CrossPriceForegroundProperty); }
            set { SetValue(CrossPriceForegroundProperty, value); }
        }
        /// <summary>Identifies the <see cref="CrossPriceForeground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CrossPriceForegroundProperty =
            DependencyProperty.Register("CrossPriceForeground", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultCrossPriceForeground));

        ///<summary>Gets the default value for the CrossPriceForeground property.</summary>
        ///<value>The default value for the <see cref="CrossPriceForeground"/> property: <c>Brushes.Red</c>.</value>
        public static Brush DefaultCrossPriceForeground { get { return new SolidColorBrush(Color.FromRgb(246, 246, 247)); } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the background for the price or volume label of the cross.</summary>
        ///<value>The background for the price or volume label of the cross. The default is determined by the <see cref="DefaultCrossPriceBackground"/>values.</value>
        ///<remarks>
        ///The price (or volume) value label locates on the price (or volume) axis area.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="CrossPriceBackgroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush CrossPriceBackground
        {
            get { return (Brush)GetValue(CrossPriceBackgroundProperty); }
            set { SetValue(CrossPriceBackgroundProperty, value); }
        }
        /// <summary>Identifies the <see cref="CrossPriceBackground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CrossPriceBackgroundProperty =
            DependencyProperty.Register("CrossPriceBackground", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultCrossPriceBackground));

        ///<summary>Gets the default value for the CrossPriceBackground property.</summary>
        ///<value>The default <see cref="Brush"/> value for the <see cref="CrossPriceBackground"/> property: <c>#FFE8EDFF</c>.</value>
        public static Brush DefaultCrossPriceBackground { get { return new SolidColorBrush(Color.FromRgb(77, 82, 94)); } }
        //----------------------------------------------------------------------------------------------------------------------------------
        int maxNumberOfDigitsAfterPointInPrice = 0;
        /// <summary>Gets the maximum number of fractional digits in a price and volume for the current candle collection.</summary>
        ///<value>The maximum number of fractional digits in a price and volume for the current candle collection.</value>
        ///<remarks>
        ///This property is recalculated every time the <see cref="CandlesSource"/> property is changed.
        ///</remarks>
        public int MaxNumberOfDigitsAfterPointInPrice
        {
            get { return maxNumberOfDigitsAfterPointInPrice; }
            private set
            {
                if (value == maxNumberOfDigitsAfterPointInPrice) return;
                maxNumberOfDigitsAfterPointInPrice = value;
                OnPropertyChanged();
            }
        }

        // Просматривает CandlesSource и пересчитывает maxNumberOfCharsInPrice
        void ReCalc_MaxNumberOfDigitsAfterPointInPrice()
        {
            if (CandlesSource == null) return;

            int max_n = 0;
            for (int i = 0; i < CandlesSource.Count; i++)
            {
                string str = CandlesSource[i].H.ToString();
                int point_i = str.LastIndexOf(',');
                if (point_i >= 0)
                {
                    int n = str.Length - point_i - 1;
                    if (n > max_n) max_n = n;
                }
            }
            MaxNumberOfDigitsAfterPointInPrice = max_n;
        }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region SCROLLBAR PROPERTIES ****************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the background for the scrollbar.</summary>
        ///<value>The brush for the scrollbar background. The default is determined by the <see cref="DefaultScrollBarBackground"/>values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="ScrollBarBackgroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush ScrollBarBackground
        {
            get { return (Brush)GetValue(ScrollBarBackgroundProperty); }
            set { SetValue(ScrollBarBackgroundProperty, value); }
        }
        /// <summary>Identifies the <see cref="ScrollBarBackground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty ScrollBarBackgroundProperty =
            DependencyProperty.Register("ScrollBarBackground", typeof(Brush), typeof(CandleChart), new PropertyMetadata(DefaultScrollBarBackground));

        ///<summary>Gets the default value for the ScrollBarBackground property.</summary>
        ///<value>The default value for the <see cref="ScrollBarBackground"/> property: <c>#FFF0F0F0</c>.</value>
        public static Brush DefaultScrollBarBackground { get { return new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)); } } // #FFF0F0F0
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the height of the scrollbar.</summary>
        ///<value>The height of the scrollbar background. The default is determined by the <see cref="DefaultScrollBarHeight"/>values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="ScrollBarHeightProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double ScrollBarHeight
        {
            get { return (double)GetValue(ScrollBarHeightProperty); }
            set { SetValue(ScrollBarHeightProperty, value); }
        }
        /// <summary>Identifies the <see cref="ScrollBarHeight"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty ScrollBarHeightProperty =
            DependencyProperty.Register("ScrollBarHeight", typeof(double), typeof(CandleChart), new PropertyMetadata(DefaultScrollBarHeight));

        ///<summary>Gets the default value for the ScrollBarHeight property.</summary>
        ///<value>The default value for the <see cref="ScrollBarHeight"/> property: <c>15.0</c>.</value>
        public static double DefaultScrollBarHeight { get { return 15.0; } }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        internal ObservableCollection<WholeContainerCandle> VisibleCandles
        {
            get { return (ObservableCollection<WholeContainerCandle>)GetValue(VisibleCandlesProperty); }
            set { SetValue(VisibleCandlesProperty, value); }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CandleChart.VisibleCandlesProperty'
        public static readonly DependencyProperty VisibleCandlesProperty =
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CandleChart.VisibleCandlesProperty'
            DependencyProperty.Register("VisibleCandles", typeof(ObservableCollection<WholeContainerCandle>), typeof(CandleChart), new PropertyMetadata(null));

        void ReCalc_VisibleCandles()
        {
            if (VisibleCandles == null)
                VisibleCandles = new ObservableCollection<WholeContainerCandle>();

            if (CandlesSource == null)
            {
                VisibleCandles.Clear();
                return;
            }

            double visibleCandlesRangeLH = CandlesLH.Y - CandlesLH.X;
            double visiblePortfolioCandlesRangeLH = PortfolioCandlesLH.Y - PortfolioCandlesLH.X;
            if (visiblePortfolioCandlesRangeLH == 0)
                visiblePortfolioCandlesRangeLH = 15;

            int common_N = Math.Min(VisibleCandles.Count, VisibleCandlesRange.Count);
            ICandle cndl, prev_cndl = VisibleCandlesRange.Start_i == 0 ? CandlesSource[0] : CandlesSource[VisibleCandlesRange.Start_i - 1];
            for (int i = 0; i < common_N; i++)
            {
                cndl = CandlesSource[VisibleCandlesRange.Start_i + i];
                VisibleCandles[i] = new WholeContainerCandle(cndl, i, visibleCandlesRangeLH, CandlesLH.X, visiblePortfolioCandlesRangeLH, PortfolioCandlesLH.X, CandleWidth, GapBetweenCandles, CandlesMaxVolume, DateTimeMilestonesHelper.GetMilestones(prev_cndl.t, cndl.t));
                prev_cndl = cndl;
            }

            int dN = VisibleCandlesRange.Count - VisibleCandles.Count;
            if (dN > 0)
            {
                int old_VisibleCandlesCount = VisibleCandles.Count;
                prev_cndl = VisibleCandlesRange.Start_i == (-common_N) ? CandlesSource[0] : CandlesSource[VisibleCandlesRange.Start_i + common_N - 1];
                for (int i = 0; i < dN; i++)
                {
                    cndl = CandlesSource[VisibleCandlesRange.Start_i + common_N + i];
                    VisibleCandles.Add(new WholeContainerCandle(cndl, old_VisibleCandlesCount + i, visibleCandlesRangeLH, CandlesLH.X, visiblePortfolioCandlesRangeLH, PortfolioCandlesLH.X, CandleWidth, GapBetweenCandles, CandlesMaxVolume, DateTimeMilestonesHelper.GetMilestones(prev_cndl.t, cndl.t)));
                    prev_cndl = cndl;
                }
            }
            else if (dN < 0)
            {
                for (int i = 0; i < -dN; i++)
                    VisibleCandles.RemoveAt(VisibleCandles.Count - 1);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the data source for the candles of this chart.</summary>
        ///<value>The data source for the candles of this chart. The default value is null.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="CandlesSourceProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public ObservableCollection<ICandle> CandlesSource
        {
            get { return (ObservableCollection<ICandle>) GetValue(CandlesSourceProperty); }
            set { SetValue(CandlesSourceProperty, value); }
        }

        /// <summary>Identifies the <see cref="CandlesSource"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CandlesSourceProperty =
            DependencyProperty.Register(nameof(CandlesSource), typeof(ObservableCollection<ICandle>), typeof(CandleChart), new UIPropertyMetadata(null, OnCandlesSourceChanged, CoerceCandlesSource));

        DateTime lastCenterCandleDateTime;
        private static object CoerceCandlesSource(DependencyObject objWithOldDP, object newDPValue)
        {
            CandleChart thisCandleChart = (CandleChart)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            ObservableCollection<ICandle> newValue = (ObservableCollection<ICandle>)newDPValue;

            IntRange vcRange = thisCandleChart.VisibleCandlesRange;
            if (IntRange.IsUndefined(vcRange))
                thisCandleChart.lastCenterCandleDateTime = DateTime.MinValue;
            else
            {
                if (thisCandleChart.CandlesSource != null && (vcRange.Start_i + vcRange.Count) < thisCandleChart.CandlesSource.Count)
                {
                    int centralCandle_i = (2 * vcRange.Start_i + vcRange.Count) / 2;
                    thisCandleChart.lastCenterCandleDateTime = thisCandleChart.CandlesSource[centralCandle_i].t;
                }
                else
                    thisCandleChart.lastCenterCandleDateTime = DateTime.MaxValue;
            }

            return newValue;
        }

        // Заменили коллекцию CandlesSource на новую:
        static void OnCandlesSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CandleChart thisCandleChart = obj as CandleChart;
            if (thisCandleChart == null) return;

            if (e.OldValue != null)
            {
                ObservableCollection<ICandle> old_obsCollection = (ObservableCollection<ICandle>)e.OldValue;
                old_obsCollection.CollectionChanged -= thisCandleChart.OnCandlesSourceCollectionChanged;
            }

            if (e.NewValue != null)
            {
                ObservableCollection<ICandle> new_obsCollection = (ObservableCollection<ICandle>)e.NewValue;
                new_obsCollection.CollectionChanged += thisCandleChart.OnCandlesSourceCollectionChanged;
            }

            if (thisCandleChart.IsLoaded)
            {
                thisCandleChart.ReCalc_TimeFrame();
                thisCandleChart.ReCalc_MaxNumberOfCharsInPrice();
                thisCandleChart.ReCalc_MaxNumberOfDigitsAfterPointInPrice();

                if (thisCandleChart.lastCenterCandleDateTime != DateTime.MinValue)
                    thisCandleChart.SetVisibleCandlesRangeCenter(thisCandleChart.lastCenterCandleDateTime);
                else
                    thisCandleChart.ReCalc_VisibleCandlesRange();
                thisCandleChart.ReCalc_CandlesLH();
                thisCandleChart.ReCalc_PortfolioCandlesLH();
                thisCandleChart.ReCalc_CandlesMaxVolume();
                thisCandleChart.ReCalc_VisibleCandles();
            }
        }

        // Произошли изменения содержимого коллекции CandlesSource:
        private void OnCandlesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //different kind of changes that may have occurred in collection
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex == (CandlesSource.Count - 1) && (VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) == e.NewStartingIndex)
                {
                    VisibleCandlesRange = new IntRange(VisibleCandlesRange.Start_i + 1, VisibleCandlesRange.Count);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                int vc_i = e.NewStartingIndex - VisibleCandlesRange.Start_i; // VisibleCandles index.
                if (vc_i >= 0 && vc_i < VisibleCandlesRange.Count)
                {
                    //WholeContainerCandle old_wcc = VisibleCandles[vc_i];
                    ReCalc_CandlesLH_AfterOneCandleChanged(e.NewStartingIndex);
                    ReCalc_PortfolioCandlesLH_AfterOneCandleChanged(e.NewStartingIndex);
                    ReCalc_CandlesMaxVolume_AfterOneCandleChanged(e.NewStartingIndex);
                    ReCalc_VisibleCandles();
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove) { /* your code */ }
            if (e.Action == NotifyCollectionChangedAction.Move) { /* your code */ }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        int timeFrame;
        /// <summary>Gets the automatically determined timeframe of this chart in minutes.</summary>
        ///<value>The automatically determined timeframe of this chart in minutes.</value>
        ///<remarks>
        ///This value is recalculated every time the <see cref="CandlesSource"/> property is changed.
        ///</remarks>
        public int TimeFrame
        {
            get { return timeFrame; }
            private set
            {
                if (value == timeFrame) return;
                timeFrame = value;
                OnPropertyChanged();
            }
        }

        // Просматривает CandlesSource и возвращает предполагаемый таймфрейм в минутах
        void ReCalc_TimeFrame()
        {
            if (CandlesSource == null) return;

            Dictionary<TimeSpan, int> hist = new Dictionary<TimeSpan, int>();

            for (int i = 1; i < CandlesSource.Count; i++)
            {
                DateTime t0 = CandlesSource[i - 1].t; // MyDateAndTime.YYMMDDHHMM_to_Datetime(CandlesSource[i - 1].YYMMDD, CandlesSource[i - 1].HHMM);
                DateTime t1 = CandlesSource[i].t; // MyDateAndTime.YYMMDDHHMM_to_Datetime(CandlesSource[i].YYMMDD, CandlesSource[i].HHMM);
                TimeSpan ts = t1 - t0;
                if (hist.ContainsKey(ts))
                    hist[ts]++;
                else
                    hist.Add(ts, 1);
            }

            int max_freq = int.MinValue;
            TimeSpan max_freq_ts = TimeSpan.MinValue;
            foreach (KeyValuePair<TimeSpan, int> keyVal in hist)
            {
                if (keyVal.Value > max_freq)
                {
                    max_freq = keyVal.Value;
                    max_freq_ts = keyVal.Key;
                }
            }

            TimeFrame = (int)(max_freq_ts.TotalMinutes);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        long candlesMaxVolume;
        /// <summary>Gets the maximum volume of the visible candles.</summary>
        ///<value>The maximum volume of the visible candles.</value>
        ///<remarks>
        ///The visible candles are those that fall inside the visible candles range, which is determined by the <see cref="VisibleCandlesRange"/> property.
        ///</remarks>
        public long CandlesMaxVolume
        {
            get { return candlesMaxVolume; }
            private set
            {
                if (value == candlesMaxVolume)
                    return;

                candlesMaxVolume = value;
                OnPropertyChanged();
            }
        }

        void ReCalc_CandlesMaxVolume()
        {
            int end_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - 1;
            long maxVolume = 0;
            for (int i = VisibleCandlesRange.Start_i; i <= end_i; i++)
            {
                ICandle cndl = CandlesSource[i];
                if (cndl.V > maxVolume) maxVolume = cndl.V;
            }

            if (CandlesMaxVolume != maxVolume)
                CandlesMaxVolume = maxVolume;
        }

        void ReCalc_CandlesMaxVolume_AfterOneCandleChanged(int changedCandle_i)
        {
            ICandle cndl = CandlesSource[changedCandle_i];
            if (CandlesMaxVolume < cndl.V)
            {
                CandlesMaxVolume = cndl.V;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        Vector candlesLH;
        ///<summary>Gets the Low and High of the visible candles in vector format (Low,High).</summary>
        ///<value>The Low and High of the visible candles in vector format (Low,High).</value>
        ///<remarks>
        ///<para>The visible candles are those that fall inside the visible candles range, which is determined by the <see cref="VisibleCandlesRange"/> property.</para>
        ///The Low of a set of candles is a minimum Low value of this candles. The High of a set of candles is a maximum High value of this candles.
        ///</remarks>
        public Vector CandlesLH
        {
            get => candlesLH;
            private set
            {
                if (value == candlesLH)
                    return;

                candlesLH = value;
                OnPropertyChanged();
            }
        }

        void ReCalc_CandlesLH()
        {
            //Debug.WriteLine($"ReCalc_CandlesLH {VisibleCandlesRange.Start_i}");
            int end_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - 1;
            double candlesTotalH = double.MinValue;
            double candlesTotalL = double.MaxValue;
            for (int i = VisibleCandlesRange.Start_i; i <= end_i; i++)
            {
                ICandle cndl = CandlesSource[i];
                if (cndl.H > candlesTotalH) candlesTotalH = cndl.H;
                if (cndl.L < candlesTotalL) candlesTotalL = cndl.L;
            }

            if (CandlesLH.X != candlesTotalL || CandlesLH.Y != candlesTotalH)
            {
                CandlesLH = new Vector(candlesTotalL, candlesTotalH);
            }
        }

        void ReCalc_CandlesLH_AfterOneCandleChanged(int changedCandle_i)
        {
            ICandle cndl = CandlesSource[changedCandle_i];
            double newL = Math.Min(cndl.L, CandlesLH.X);
            double newH = Math.Max(cndl.H, CandlesLH.Y);
            if (newL < CandlesLH.X || newH > CandlesLH.Y)
            {
                CandlesLH = new Vector(newL, newH);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        Vector portfolioCandlesLH;
        public Vector PortfolioCandlesLH
        {
            get => portfolioCandlesLH;
            private set
            {
                if (value == portfolioCandlesLH)
                    return;

                portfolioCandlesLH = value;
                OnPropertyChanged();
            }
        }

        bool HasPortfolioData() => CandlesSource.Where(x => x.Portfolio != null).Count() == CandlesSource.Count();

        void ReCalc_PortfolioCandlesLH()
        {
            if (HasPortfolioData() == false)
                return;

            //Debug.WriteLine($"ReCalc_CandlesLH {VisibleCandlesRange.Start_i}");
            int end_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - 1;
            double candlesTotalH = double.MinValue;
            double candlesTotalL = double.MaxValue;
            for (int i = VisibleCandlesRange.Start_i; i <= end_i; i++)
            {
                ICandle cndl = CandlesSource[i];
                if (cndl.Portfolio.H > candlesTotalH) candlesTotalH = cndl.Portfolio.H;
                if (cndl.Portfolio.L < candlesTotalL) candlesTotalL = cndl.Portfolio.L;
            }

            if (PortfolioCandlesLH.X != candlesTotalL || PortfolioCandlesLH.Y != candlesTotalH)
                PortfolioCandlesLH = new Vector(candlesTotalL, candlesTotalH);
        }

        void ReCalc_PortfolioCandlesLH_AfterOneCandleChanged(int changedCandle_i)
        {
            if (HasPortfolioData() == false)
                return;

            ICandle cndl = CandlesSource[changedCandle_i];
            double newL = Math.Min(cndl.Portfolio.L, PortfolioCandlesLH.X);
            double newH = Math.Max(cndl.Portfolio.H, PortfolioCandlesLH.Y);
            if (newL < PortfolioCandlesLH.X || newH > PortfolioCandlesLH.Y)
                PortfolioCandlesLH = new Vector(newL, newH);
        }
        //----------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Gets the range of indexes of candles, currently visible in this chart window.</summary>
        ///<value>The range of indexes of candles, currently visible in this chart window. The default value is <see cref="IntRange.Undefined"/>.</value>
        ///<remarks>
        ///This property defines the part of collection of candles <see cref="CandlesSource"/> that currently visible in the chart window.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VisibleCandlesRangeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public IntRange VisibleCandlesRange
        {
            get { return (IntRange)GetValue(VisibleCandlesRangeProperty); }
            set { SetValue(VisibleCandlesRangeProperty, value); }
        }
        /// <summary>Identifies the <see cref="VisibleCandlesRange"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VisibleCandlesRangeProperty =
            DependencyProperty.Register("VisibleCandlesRange", typeof(IntRange), typeof(CandleChart), new PropertyMetadata(IntRange.Undefined, OnVisibleCanlesRangeChanged, CoerceVisibleCandlesRange));

        static void OnVisibleCanlesRangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CandleChart thisCandleChart = (CandleChart)obj;
            if (thisCandleChart.IsLoaded)
            {
                thisCandleChart.ReCalc_CandlesLH();
                thisCandleChart.ReCalc_PortfolioCandlesLH();
                thisCandleChart.ReCalc_CandlesMaxVolume();
                thisCandleChart.ReCalc_VisibleCandles();
            }
        }

        private static object CoerceVisibleCandlesRange(DependencyObject objWithOldDP, object baseValue)
        {
            // Если произошли изменения CandleWidth или GapBetweenCandles, то возвращает true. Иначе - false.
            CandleChart thisCandleChart = (CandleChart)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            IntRange newValue = (IntRange)baseValue;

            if (IntRange.IsUndefined(newValue))
                return newValue;
            // Это хак для привязки к скроллеру, когда передается только компонента IntRange.Start_i, а компонента IntRange.Count берется из старого значения свойства:
            else if (IntRange.IsContainsOnlyStart_i(newValue))
                return new IntRange(newValue.Start_i, thisCandleChart.VisibleCandlesRange.Count);
            // А это обычная ситуация:
            else
            {
                int newVisibleCandlesStart_i = Math.Max(0, newValue.Start_i);
                int newVisibleCandlesEnd_i = Math.Min(thisCandleChart.CandlesSource.Count - 1, newValue.Start_i + Math.Max(1, newValue.Count) - 1);
                int maxVisibleCandlesCount = thisCandleChart.MaxVisibleCandlesCount;
                int newVisibleCandlesCount = newVisibleCandlesEnd_i - newVisibleCandlesStart_i + 1;
                if (newVisibleCandlesCount > maxVisibleCandlesCount)
                {
                    newVisibleCandlesStart_i = newVisibleCandlesEnd_i - maxVisibleCandlesCount + 1;
                    newVisibleCandlesCount = maxVisibleCandlesCount;
                }

                return new IntRange(newVisibleCandlesStart_i, newVisibleCandlesCount);
            }
        }

        // Пересчитывает VisibleCandlesRange.Count таким образом, чтобы по возможности сохранить индекс последней видимой свечи 
        // и соответствовать текущим значениям CandleWidth и GapBetweenCandles.
        void ReCalc_VisibleCandlesRange()
        {
            if (priceChartContainer.ActualWidth == 0 || CandlesSource == null)
            {
                VisibleCandlesRange = IntRange.Undefined;
                return;
            }

            int newCount = (int)(priceChartContainer.ActualWidth / (CandleWidth + GapBetweenCandles));
            if (newCount > CandlesSource.Count) newCount = CandlesSource.Count;
            int new_start_i = IntRange.IsUndefined(VisibleCandlesRange) ? (CandlesSource.Count - newCount) : VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - newCount;
            if (new_start_i < 0) new_start_i = 0;
            if (new_start_i + newCount > CandlesSource.Count)
                new_start_i = CandlesSource.Count - newCount;

            VisibleCandlesRange = new IntRange(new_start_i, newCount);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        int MaxVisibleCandlesCount
        { get { return (int)(priceChartContainer.ActualWidth / 2); } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Shifts the range of visible candles to the position where the <c>t</c> property of the central visible candle is equal (or closest) to specified value.</summary>
        ///<param name="visibleCandlesRangeCenter">Central visible candle should have its <c>t</c> property equal to this parameter (or close to it as much as possible).</param>
        public void SetVisibleCandlesRangeCenter(DateTime visibleCandlesRangeCenter)
        {
            ICandle cndl = CandlesSource[VisibleCandlesRange.Count / 2];
            if (visibleCandlesRangeCenter < cndl.t) //MyDateAndTime.YYMMDDHHMM_to_Datetime(cndl.YYMMDD, cndl.HHMM))
            {
                VisibleCandlesRange = new IntRange(0, VisibleCandlesRange.Count);
                return;
            }

            cndl = CandlesSource[CandlesSource.Count - 1 - VisibleCandlesRange.Count / 2];
            if (visibleCandlesRangeCenter > cndl.t) // MyDateAndTime.YYMMDDHHMM_to_Datetime(cndl.YYMMDD, cndl.HHMM))
            {
                VisibleCandlesRange = new IntRange(CandlesSource.Count - VisibleCandlesRange.Count, VisibleCandlesRange.Count);
                return;
            }

            VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(CandlesSource.FindCandleByDatetime(visibleCandlesRangeCenter) - VisibleCandlesRange.Count / 2);
        }

        ///<summary>Sets the range of visible candles, that starts and ends at specified moments in time.</summary>
        ///<param name="lowerBound">The datetime value at which the range of visible candles must start.</param>
        ///<param name="upperBound">The datetime value at which the range of visible candles must end.</param>
        ///<remarks>
        ///This function finds in the <see cref="CandlesSource"/> collection of candles two of them that has its <c>t</c> property equal or closest to <c>datetime0</c> and <c>datetime1</c>. 
        ///Then it sets the <see cref="VisibleCandlesRange"/> to the <see cref="IntRange"/> value that starts at the index of the first aforementioned candle, and ends at the index of the second one.
        ///</remarks>
        public void SetVisibleCandlesRangeBounds(DateTime lowerBound, DateTime upperBound)
        {
            if (CandlesSource == null || CandlesSource.Count == 0) return;

            if (lowerBound > upperBound)
            {
                DateTime t_ = lowerBound;
                lowerBound = upperBound;
                upperBound = t_;
            }

            int i0, i1;
            int N = CandlesSource.Count;
            if (CandlesSource[0].t > upperBound)
            {
                VisibleCandlesRange = new IntRange(0, 1);
                return;
            }

            if (CandlesSource[N - 1].t < lowerBound)
            {
                VisibleCandlesRange = new IntRange(N - 1, 1);
                return;
            }

            if (CandlesSource[0].t > lowerBound)
                i0 = 0;
            else
                i0 = CandlesSource.FindCandleByDatetime(lowerBound);

            if (CandlesSource[N - 1].t < upperBound)
                i1 = N - 1;
            else
                i1 = CandlesSource.FindCandleByDatetime(upperBound);

            int newVisibleCandlesCount = i1 - i0 + 1;
            ReCalc_CandleWidthAndGapBetweenCandles(newVisibleCandlesCount);
            VisibleCandlesRange = new IntRange(i0, newVisibleCandlesCount);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        bool ReCalc_CandleWidthAndGapBetweenCandles(int new_VisibleCandlesCount)
        {
            if (new_VisibleCandlesCount <= 0) return false;

            double new_ActualWidth = priceChartContainer.ActualWidth;
            if (new_ActualWidth == 0)
            {
                CandleWidth = 0;
                GapBetweenCandles = 0;
                return false;
            }

            while (new_VisibleCandlesCount * (CandleWidth + 1.0) + 1.0 > new_ActualWidth)
            {
                if (Math.Round(CandleWidth) < 3.0)
                    return false;
                else
                    CandleWidth -= 2.0;
            }

            while (new_VisibleCandlesCount * (CandleWidth + 3.0) + 1.0 < new_ActualWidth)
                CandleWidth += 2.0;

            GapBetweenCandles = (new_ActualWidth - new_VisibleCandlesCount * CandleWidth - 1.0) / new_VisibleCandlesCount;
            return true;
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the modifier key that in conjunction with mouse wheel rolling will cause a change of the visible candles range width.</summary>
        ///<value>The the modifier key that in conjunction with mouse wheel rolling will cause a change of the visible candles range width. The default value is <see cref="ModifierKeys.None"/>.</value>
        ///<remarks>
        ///Depending on the keyboard modifier keys the mouse wheel can serve for two functions: scrolling through the candle collection and changing the width of visible candles range. 
        ///You can set up modifier keys for the aforementioned functions by setting the <see cref="MouseWheelModifierKeyForScrollingThroughCandles"/> and 
        ///<see cref="MouseWheelModifierKeyForCandleWidthChanging"/> properties respectively.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VisibleCandlesRangeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public ModifierKeys MouseWheelModifierKeyForCandleWidthChanging
        {
            get { return (ModifierKeys)GetValue(MouseWheelModifierKeyForCandleWidthChangingProperty); }
            set { SetValue(MouseWheelModifierKeyForCandleWidthChangingProperty, value); }
        }
        /// <summary>Identifies the <see cref="MouseWheelModifierKeyForCandleWidthChanging"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty MouseWheelModifierKeyForCandleWidthChangingProperty =
            DependencyProperty.Register("MouseWheelModifierKeyForCandleWidthChanging", typeof(ModifierKeys), typeof(CandleChart), new PropertyMetadata(ModifierKeys.None));
        //--------
        /// <summary>Gets or sets a modifier key that in conjunction with mouse wheel rolling will cause a scrolling through the candles.</summary>
        ///<value>The the modifier key that in conjunction with mouse wheel rolling will cause a scrolling through the candles. The default value is <see cref="ModifierKeys.Control"/>.</value>
        ///<remarks>
        ///Depending on the keyboard modifier keys the mouse wheel can serve for two functions: scrolling through the candle collection and changing the width of visible candles range. 
        ///You can set up modifier keys for the aforementioned functions by setting the <see cref="MouseWheelModifierKeyForScrollingThroughCandles"/> and 
        ///<see cref="MouseWheelModifierKeyForCandleWidthChanging"/> properties respectively.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VisibleCandlesRangeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public ModifierKeys MouseWheelModifierKeyForScrollingThroughCandles
        {
            get { return (ModifierKeys)GetValue(MouseWheelModifierKeyForScrollingThroughCandlesProperty); }
            set { SetValue(MouseWheelModifierKeyForScrollingThroughCandlesProperty, value); }
        }
        /// <summary>Identifies the <see cref="MouseWheelModifierKeyForScrollingThroughCandles"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty MouseWheelModifierKeyForScrollingThroughCandlesProperty =
            DependencyProperty.Register("MouseWheelModifierKeyForScrollingThroughCandles", typeof(ModifierKeys), typeof(CandleChart), new PropertyMetadata(ModifierKeys.Control));
        
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            void SetVisibleCandlesRangeCount(int newCount)
            {
                if (newCount > CandlesSource.Count) newCount = CandlesSource.Count;
                if (newCount == VisibleCandlesRange.Count) return;
                if (!ReCalc_CandleWidthAndGapBetweenCandles(newCount)) return; // Если график уже нельзя больше сжимать.

                int new_start_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - newCount;
                if (new_start_i < 0) new_start_i = 0;
                VisibleCandlesRange = new IntRange(new_start_i, newCount);
            }

            if (Keyboard.Modifiers == MouseWheelModifierKeyForCandleWidthChanging)
            {
                var change = (int) (Math.Max(1, (double) VisibleCandlesRange.Count * 0.25d));
                if (e.Delta > 0)
                    SetVisibleCandlesRangeCount(VisibleCandlesRange.Count - change);
                else if (e.Delta < 0)
                    SetVisibleCandlesRangeCount(VisibleCandlesRange.Count + change);
            }
            else if (Keyboard.Modifiers == MouseWheelModifierKeyForScrollingThroughCandles)
            {
                var change = 1;
                if (e.Delta > 0)
                {
                    if ((VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) < CandlesSource.Count)
                        VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(VisibleCandlesRange.Start_i + change);
                }
                else if (e.Delta < 0)
                {
                    if (VisibleCandlesRange.Start_i > 0) 
                        VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(VisibleCandlesRange.Start_i - change);
                }
            }
        }

        private void OnMouseMoveInsidePriceChartContainer(object sender, MouseEventArgs e)
        {
            CurrentMousePosition = Mouse.GetPosition(priceChartContainer);
        }

        private void OnMouseMoveInsideVolumeHistogramContainer(object sender, MouseEventArgs e)
        {
            CurrentMousePosition = Mouse.GetPosition(volumeHistogramContainer);
        }

        private void OnMouseMoveInsidPortfolioChartContainer(object sender, MouseEventArgs e)
        {
            CurrentMousePosition = Mouse.GetPosition(portfolioChartContainer);
        }

        Point currentMousePosition;
        /// <summary>This is a property for internal use only. You should not use it.</summary>
        public Point CurrentMousePosition
        {
            get => currentMousePosition;
            private set
            {
                if (currentMousePosition == value)
                    return;
                currentMousePosition = value;
                OnPropertyChanged();
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        private void OnPanelCandlesContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded || e.NewSize.Width == 0 || CandlesSource.Count() == 0)
                return;

            if (e.NewSize.Width != e.PreviousSize.Width)
                ReCalc_VisibleCandlesRange();
        }
        //---------------- INotifyPropertyChanged ----------------------------------------------------------
        /// <summary>INotifyPropertyChanged interface realization.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>INotifyPropertyChanged interface realization.</summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
    }
}
using System;
using System.Windows;
using System.Windows.Media;

using MaterialDesignThemes.Wpf;

using Caliburn.Micro;

namespace AlgoTrader.Dashboard.Model.Common
{
    public class MenuNavigationItem<T> : MenuNavigationItem where T : Screen
    {
        public MenuNavigationItem(PackIconKind icon, string title) : base(icon, title, typeof(T)) { }

        public override Screen GetScreen() => IoC.Get<T>();
    }

    public abstract class MenuNavigationItem : PropertyChangedBase
    {
        private PackIconKind _icon;
        public PackIconKind Icon { get => _icon; set => Set(ref _icon, value); }

        private string _title;
        public string Title { get => _title; set => Set(ref _title, value); }

        private Brush _foreground = ForegroundUnfocused;
        public Brush Foreground { get => _foreground; set => Set(ref _foreground, value); }

        private Brush _background = BackgroundUnfocused;
        public Brush Background { get => _background; set => Set(ref _background, value); }

        private FontWeight _fontWeight = FontWeights.Bold;
        public FontWeight FontWeight { get => _fontWeight; set => Set(ref _fontWeight, value); }

        private Type _pageType;
        public Type PageType { get => _pageType; set => Set(ref _pageType, value); }

        #region Defaults

        private static ITheme Theme => new PaletteHelper().GetTheme();

        private static Brush ForegroundUnfocused => new SolidColorBrush(Theme.PrimaryDark.ForegroundColor.Value);
        private static Brush ForegroundFocused => new SolidColorBrush(Theme.PrimaryLight.Color);

        private static Brush BackgroundUnfocused => new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        private static Brush BackgroundFocused => BackgroundUnfocused;
        //private static Brush BackgroundFocused { get { var c = ThemeColorPair.Color; return new SolidColorBrush(Color.FromArgb(100, c.R, c.G, c.B)); } }

        #endregion

        public abstract Screen GetScreen();

        public MenuNavigationItem(PackIconKind icon, string title, Type pageType)
        {
            Icon = icon;
            Title = title;
            _pageType = pageType;
        }

        public void Focus()
        {
            Foreground = ForegroundFocused;
            Background = BackgroundFocused;
            FontWeight = FontWeights.ExtraBold;
        }

        public void Unfocus()
        {
            Foreground = ForegroundUnfocused;
            Background = BackgroundUnfocused;
            FontWeight = FontWeights.DemiBold;
        }
    }
}

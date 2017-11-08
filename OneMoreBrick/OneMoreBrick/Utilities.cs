using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace OneMoreBrick {
    public static class Utilities {
        public static Typeface DefaultTypeface = new Typeface(
            new FontFamily("Arial"),
            new FontStyle(),
            new FontWeight(),
            new FontStretch()
        );

        public static FormattedText GetFormattedText(string text) {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                DefaultTypeface,
                12,
                new SolidColorBrush(Colors.White)
            );
        }
    }
}

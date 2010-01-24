namespace SumControls.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converts an object to a System.Windows.Visibility value based on whether or not the object is null
    /// </summary>
    /// <example>If object is not null, the converter will return Visibility.Visible, otherwise it will return
    /// Visibility.Hidden</example>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class ObjectVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a given object to a Visibility
        /// </summary>
        /// <param name="value">The object value to convert</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>The Visibility.Visible value if the object is not null, or Visibility.Hidden otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// This method is not implemented and will throw an InvalidOperationException exception
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>An Exception is thrown</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
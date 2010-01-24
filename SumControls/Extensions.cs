namespace SumControls
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    using EnumAttributes;

    /// <summary>
    /// Contains a collection of extension methods
    /// </summary>
    public static class Extensions
    {
        #region IList extensions

        /// <summary>
        /// Swaps the values of the two given indexes
        /// </summary>
        /// <param name="list">The list to swap the values in</param>
        /// <param name="i">The first index of the list to swap</param>
        /// <param name="j">The second index of the list to swap</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i",
            Justification = "It is an index value")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "j",
            Justification = "It is an index value")]
        public static void Swap(this IList list, int i, int j)
        {
            var element = list[i];
            list[i] = list[j];
            list[j] = element;
        }

        /// <summary>
        /// Swaps the given index value with the next value
        /// @pre: i larger than -1 and smaller than Count - 2
        /// </summary>
        /// <param name="list">The list to swap the values in</param>
        /// <param name="i">The index to swap</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i",
            Justification = "It is an index value")]
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "i+1",
            Justification = "The Exception should never occur")]
        public static void SwapNext(this IList list, int i)
        {
            list.Swap(i, i + 1);
        }

        /// <summary>
        /// Swaps the given index value with the previous value
        /// @pre: i larger than 0 and smaller than Count - 1
        /// </summary>
        /// <param name="list">The list to swap the values in</param>
        /// <param name="i">The index to swap</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i",
            Justification = "This is an index value")]
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "i-1",
            Justification = "The Exception should never occur")]
        public static void SwapPrevious(this IList list, int i)
        {
            list.Swap(i, i - 1);
        }

        #endregion IList extensions

        #region Enum extensions

        /// <summary>
        /// Get the ID attribute of the Enum instance
        /// </summary>
        /// <param name="e">The Enum instance to retrieve the ID from</param>
        /// <returns>The ID of the Enum</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e",
            Justification = "This is allowed in an extension method")]
        public static string GetId(this Enum e)
        {
            var ids = (EnumIdAttribute[])e.GetType().GetField(e.ToString())
                .GetCustomAttributes(typeof(EnumIdAttribute), false);

            return ids.Length == 1 ? ids[0].Id : null;
        }

        /// <summary>
        /// Get the string representation of the Enum instance
        /// </summary>
        /// <param name="e">The Enum instance to retrieve the string representation of</param>
        /// <returns>The string representation of the Enum</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e",
            Justification = "This is allowed in an extension method")]
        public static string GetValue(this Enum e)
        {
            var values = (EnumValueAttribute[])e.GetType().GetField(e.ToString())
                .GetCustomAttributes(typeof(EnumValueAttribute), false);

            return values.Length == 1 ? values[0].Value : e.ToString();
        }

        #endregion Enum extensions

        #region string extensions

        /// <summary>
        /// Converts the string to an empty string if it is null
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>An string.Empty if the string was null, or the original unchanged value of the string</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s",
            Justification = "This is allowed in an extension method")]
        public static string ConvertNullToEmpty(this string s)
        {
            return s ?? string.Empty;
        }

        #endregion string extensions

        #region double extensions

        /// <summary>
        /// Determines whether the double is a valid value (not NaN or Infinity)
        /// </summary>
        /// <param name="d">The double to test</param>
        /// <returns>true if the double is valid</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d",
            Justification = "This is allowed in an extension method")]
        public static bool IsValid(this double d)
        {
            return !(double.IsNaN(d) || double.IsInfinity(d));
        }

        #endregion double extensions

        #region Point extensions

        /// <summary>
        /// Rounds the values of the Point to the their closet values
        /// </summary>
        /// <param name="p">The Point to round</param>
        /// <returns>The new Point with rounded values</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "p",
            Justification = "This is allowed in an extension method")]
        public static Point Round(this Point p)
        {
            return new Point(Math.Round(p.X), Math.Round(p.Y));
        }

        #endregion Point extensions

        #region Size extensions

        /// <summary>
        /// Rounds the values of the Size to their closet values
        /// </summary>
        /// <param name="s">The Size to round</param>
        /// <returns>The new Size with rounded values</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s",
            Justification = "This is allowed in an extension method")]
        public static Size Round(this Size s)
        {
            return new Size(Math.Round(s.Width), Math.Round(s.Height));
        }

        #endregion Size extensions
    }
}
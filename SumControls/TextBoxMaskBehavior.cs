// This is not my code. See comments for original author
namespace SumControls
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// WPF Maskable TextBox class. Just specify the TextBoxMaskBehavior.Mask attached property to a TextBox. 
    /// It protect your TextBox from unwanted non numeric symbols and make it easy to modify your numbers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Class Information:
    /// <list type="bullet">
    /// <item name="authors">Authors: Ruben Hakopian</item>
    /// <item name="date">February 2009</item>
    /// <item name="originalURL">http://www.rubenhak.com/?p=8</item>
    /// </list>
    /// </para>
    /// </remarks>
    internal static class TextBoxMaskBehavior
    {
        #region MinimumValue Property

        /// <summary>
        /// Gets the minimum value
        /// </summary>
        /// <param name="obj">The object to retrieve the value from</param>
        /// <returns>The minimum value</returns>
        public static double GetMinimumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinimumValueProperty);
        }

        /// <summary>
        /// Sets the minimum value
        /// </summary>
        /// <param name="obj">The object to set the value for</param>
        /// <param name="value">The new value</param>
        public static void SetMinimumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinimumValueProperty, value);
        }

        /// <summary>
        /// Identifies the MinimumValue dependency property
        /// </summary>
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(double),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback));

        private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidateTextBox(d as TextBox);
        }

        #endregion

        #region MaximumValue Property

        /// <summary>
        /// Gets the maximum value
        /// </summary>
        /// <param name="obj">The object to get the value for</param>
        /// <returns>The maximum value</returns>
        public static double GetMaximumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaximumValueProperty);
        }

        /// <summary>
        /// Sets the maxmum value
        /// </summary>
        /// <param name="obj">The object to set the value for</param>
        /// <param name="value">The new value</param>
        public static void SetMaximumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaximumValueProperty, value);
        }

        /// <summary>
        /// Identifies the MaximumValue attached property
        /// </summary>
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(double),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback));

        private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidateTextBox(d as TextBox);
        }

        #endregion

        #region Mask Property

        /// <summary>
        /// Gets the mask
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>The value</returns>
        public static MaskType GetMask(DependencyObject obj)
        {
            return (MaskType)obj.GetValue(MaskProperty);
        }

        /// <summary>
        /// Sets the mask
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="value">The value</param>
        public static void SetMask(DependencyObject obj, MaskType value)
        {
            obj.SetValue(MaskProperty, value);
        }

        /// <summary>
        /// Identifies the mask attached property
        /// </summary>
        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.RegisterAttached(
                "Mask",
                typeof(MaskType),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(MaskChangedCallback));

        private static void MaskChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TextBox)
            {
                (e.OldValue as TextBox).PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler((e.OldValue as TextBox), TextBoxPastingEventHandler);
            }

            var textBox = d as TextBox;
            if (textBox == null)
            {
                return;
            }

            if ((MaskType)e.NewValue != MaskType.Any)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(textBox, TextBoxPastingEventHandler);
            }

            ValidateTextBox(textBox);
        }

        #endregion

        #region Private Static Methods

        private static void ValidateTextBox(TextBox textBox)
        {
            if (GetMask(textBox) != MaskType.Any)
            {
                textBox.Text = ValidateValue(GetMask(textBox), textBox.Text);
            }
        }

        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = (TextBox)sender;
            var clipboard = e.DataObject.GetData(typeof(string)) as string;
            clipboard = ValidateValue(GetMask(textBox), clipboard);
            if (!string.IsNullOrEmpty(clipboard))
            {
                textBox.Text = clipboard;
            }

            e.CancelCommand();
            e.Handled = true;
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var isValid = IsSymbolValid(GetMask(textBox), e.Text);
            e.Handled = !isValid;
            if (!isValid || textBox == null)
            {
                return;
            }

            var caret = textBox.CaretIndex;
            var text = textBox.Text;
            var textInserted = false;
            var selectionLength = 0;

            if (textBox.SelectionLength > 0)
            {
                text = text.Substring(0, textBox.SelectionStart) +
                    text.Substring(textBox.SelectionStart + textBox.SelectionLength);
                caret = textBox.SelectionStart;
            }

            if (e.Text == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
            {
                while (true)
                {
                    var ind = text.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator,
                        StringComparison.CurrentCulture);
                    if (ind == -1)
                    {
                        break;
                    }

                    text = text.Substring(0, ind) + text.Substring(ind + 1);
                    if (caret > ind)
                    {
                        caret--;
                    }
                }

                if (caret == 0)
                {
                    text = "0" + text;
                    caret++;
                }
                else
                {
                    if (caret == 1 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign)
                    {
                        text = NumberFormatInfo.CurrentInfo.NegativeSign + "0" + text.Substring(1);
                        caret++;
                    }
                }

                if (caret == text.Length)
                {
                    selectionLength = 1;
                    textInserted = true;
                    text = text + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "0";
                    caret++;
                }
            }
            else if (e.Text == NumberFormatInfo.CurrentInfo.NegativeSign)
            {
                textInserted = true;
                if (textBox.Text.Contains(NumberFormatInfo.CurrentInfo.NegativeSign))
                {
                    text = text.Replace(NumberFormatInfo.CurrentInfo.NegativeSign, string.Empty);
                    if (caret != 0)
                    {
                        caret--;
                    }
                }
                else
                {
                    text = NumberFormatInfo.CurrentInfo.NegativeSign + textBox.Text;
                    caret++;
                }
            }

            if (!textInserted)
            {
                text = text.Substring(0, caret) + e.Text +
                    ((caret < textBox.Text.Length) ? text.Substring(caret) : string.Empty);

                caret++;
            }

            try
            {
                var val = Convert.ToDouble(text, CultureInfo.CurrentCulture);
                var newVal = ValidateLimits(GetMinimumValue(textBox), GetMaximumValue(textBox), val);
                if (val != newVal)
                {
                    text = newVal.ToString(CultureInfo.CurrentCulture);
                }
                else if (val == 0)
                {
                    if (!text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                    {
                        text = "0";
                    }
                }
            }
            catch (FormatException)
            {
                text = "0";
            }
            catch (OverflowException)
            {
                text = "0";
            }

            while (text.Length > 1 && text[0] == '0' && string.Empty + text[1] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
            {
                text = text.Substring(1);
                if (caret > 0)
                {
                    caret--;
                }
            }

            while (text.Length > 2 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign && text[1] == '0' && string.Empty + text[2] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
            {
                text = NumberFormatInfo.CurrentInfo.NegativeSign + text.Substring(2);
                if (caret > 1)
                {
                    caret--;
                }
            }

            if (caret > text.Length)
            {
                caret = text.Length;
            }

            textBox.Text = text;
            textBox.CaretIndex = caret;
            textBox.SelectionStart = caret;
            textBox.SelectionLength = selectionLength;
            e.Handled = true;
        }

        private static string ValidateValue(MaskType mask, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            value = value.Trim();
            switch (mask)
            {
                case MaskType.Integer:
                    try
                    {
                        Convert.ToInt64(value, CultureInfo.CurrentCulture);
                        return value;
                    }
                    catch (FormatException)
                    {
                        return string.Empty;
                    }
                    catch (OverflowException)
                    {
                        return string.Empty;
                    }

                case MaskType.Decimal:
                    try
                    {
                        Convert.ToDouble(value, CultureInfo.CurrentCulture);

                        return value;
                    }
                    catch (FormatException)
                    {
                        return string.Empty;
                    }
                    catch (OverflowException)
                    {
                        return string.Empty;
                    }
            }

            return value;
        }

        private static double ValidateLimits(double min, double max, double value)
        {
            if (!min.Equals(double.NaN))
            {
                if (value < min)
                {
                    return min;
                }
            }

            if (!max.Equals(double.NaN))
            {
                if (value > max)
                {
                    return max;
                }
            }

            return value;
        }

        private static bool IsSymbolValid(MaskType mask, string str)
        {
            switch (mask)
            {
                case MaskType.Any:
                    return true;

                case MaskType.Integer:
                    if (str == NumberFormatInfo.CurrentInfo.NegativeSign)
                    {
                        return true;
                    }

                    break;

                case MaskType.Decimal:
                    if (str == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator ||
                        str == NumberFormatInfo.CurrentInfo.NegativeSign)
                    {
                        return true;
                    }

                    break;
            }

            if (mask.Equals(MaskType.Integer) || mask.Equals(MaskType.Decimal))
            {
                return str.All(Char.IsDigit);
            }

            return false;
        }

        #endregion
    }

    internal enum MaskType
    {
        /// <summary>
        /// Allow any value type
        /// </summary>
        Any,

        /// <summary>
        /// Allow any integer value
        /// </summary>
        Integer,

        /// <summary>
        /// Allow any decimal value
        /// </summary>
        Decimal
    }
}
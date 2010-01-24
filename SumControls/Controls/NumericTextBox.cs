namespace SumControls.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Represents a TextBox control that only accepts numeric input
    /// </summary>
    public class NumericTextBox : TextBox
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the Minimum dependency property
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(NumericTextBox),
                new PropertyMetadata(0D, Minimum_Changed));

        /// <summary>
        /// Identifies the Maximum dependency property
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(NumericTextBox),
                new PropertyMetadata(100D, Maximum_Changed));

        #endregion Dependency properties

        /// <summary>
        /// Initializes a new instance of the NumericTextBox class
        /// </summary>
        public NumericTextBox()
        {
            SetValue(TextBoxMaskBehavior.MaskProperty, MaskType.Integer);
            SetMinimum(Minimum);
            SetMaximum(Maximum);
        }

        #region Public properties

        /// <summary>
        /// Gets or sets the minimum value which can be entered into the NumericTextBox. This is a dependency property
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum value which can be entered into the NumericTextBox. This is a dependency property
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        #endregion Public properties

        #region Dependency property related data

        /// <summary>
        /// Updates the minimum value that can be entered into the NumericTextBox when the property is changed
        /// </summary>
        /// <param name="dp">The NumericTextBox whose property was changed</param>
        /// <param name="e">The property change event arguments</param>
        private static void Minimum_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((NumericTextBox)dp).SetMinimum((double)e.NewValue);
        }

        /// <summary>
        /// Updates the maximum value that can be entered into the NumericTextBox when the property is changed
        /// </summary>
        /// <param name="dp">The NumericTextBox whose property was changed</param>
        /// <param name="e">The property change event arguments</param>
        private static void Maximum_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((NumericTextBox)dp).SetMaximum((double)e.NewValue);
        }

        #endregion Dependency property related data

        #region Private methods

        private void SetMinimum(double value)
        {
            SetValue(TextBoxMaskBehavior.MinimumValueProperty, value);
        }

        private void SetMaximum(double value)
        {
            SetValue(TextBoxMaskBehavior.MaximumValueProperty, value);
        }

        #endregion Private methods
    }
}
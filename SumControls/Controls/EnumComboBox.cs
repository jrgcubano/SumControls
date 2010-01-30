namespace SumControls.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Represents a ComboBox which displays Enum values within its drop-down list
    /// </summary>
    public class EnumComboBox : ComboBox
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the EnumType dependency property
        /// </summary>
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumComboBox),
                new PropertyMetadata(null, EnumType_Changed), IsEnumTypeValid);

        /// <summary>
        /// Identifies the EnumValues dependency property
        /// </summary>
        private static readonly DependencyProperty EnumValuesProperty =
            DependencyProperty.Register("EnumValues", typeof(Array), typeof(EnumComboBox));

        #endregion Dependency properties

        /// <summary>
        /// Initializes a new instance of the EnumComboBox class
        /// </summary>
        public EnumComboBox()
        {
            SelectedValuePath = "Key";
            DisplayMemberPath = "Value";

            var binding = new Binding("EnumValues") { Source = this };
            SetBinding(ItemsSourceProperty, binding);
        }

        /// <summary>
        /// Gets or sets the enum this EnumComboBox represents. This is a dependency property
        /// </summary>
        public Type EnumType
        {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the values this EnumComboBox holds in the ItemsSource property. This is a dependency property
        /// </summary>
        private Array EnumValues
        {
            set { SetValue(EnumValuesProperty, value); }
        }

        #region Dependency property methods

        /// <summary>
        /// Updates the items when the EnumType property is changed
        /// </summary>
        /// <param name="dp">The EnumComboBox instance which the property belongs to</param>
        /// <param name="e">The parameter is not used.</param>
        private static void EnumType_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((EnumComboBox)dp).UpdateItems();
        }

        /// <summary>
        /// Returns whether the given EnumType property value is valid
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>true if the given value is null or of Enum type</returns>
        private static bool IsEnumTypeValid(object value)
        {
            var type = (Type)value;
            return type == null || type.IsEnum;
        }

        #endregion Dependency property methods

        /// <summary>
        /// Updates the items the EnumComboBox displays
        /// </summary>
        private void UpdateItems()
        {
            if (EnumType != null)
            {
                var enumValues = new List<KeyValuePair<string, string>>();
                enumValues.AddRange(from Enum value in Enum.GetValues(EnumType)
                let id = value.GetId()
                let name = value.GetValue()
                select new KeyValuePair<string, string>(string.IsNullOrEmpty(id) ? name : id, name));

                EnumValues = enumValues.ToArray();
            }
            else
            {
                EnumValues = null;
            }
        }
    }
}
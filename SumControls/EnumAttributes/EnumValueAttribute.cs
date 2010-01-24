namespace SumControls.EnumAttributes
{
    using System;

    /// <summary>
    /// Specifies a string representation of an Enum field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumValueAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the EnumValueAttribute class
        /// </summary>
        /// <param name="value">The string representation of the Enum</param>
        public EnumValueAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the string representation of the Enum Attribute
        /// </summary>
        public string Value { get; private set; }
    }
}
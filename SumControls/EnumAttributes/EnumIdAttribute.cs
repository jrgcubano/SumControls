namespace SumControls.EnumAttributes
{
    using System;

    /// <summary>
    /// Specifies an ID for an Enum field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumIdAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the EnumIdAttribute class
        /// </summary>
        /// <param name="id">The ID of the Enum</param>
        public EnumIdAttribute(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the ID of the Enum Attribute
        /// </summary>
        public string Id { get; private set; }
    }
}
namespace SumControls.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    /// <summary>
    /// Represents a Windows Button control, which is intelligent enough to change its Content based on its Command
    /// </summary>
    public class CommandButton : Button
    {
        /// <summary>
        /// Initializes a new instance of the CommandButton class
        /// </summary>
        public CommandButton()
        {
            var property = DependencyPropertyDescriptor.FromProperty(CommandProperty, typeof(ButtonBase));
            property.AddValueChanged(this, CommandChanged);
        }

        /// <summary>
        /// Updates the Content binding when the Command property is changed
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="args">The parameter is not used.</param>
        private void CommandChanged(object sender, EventArgs args)
        {
            if (Command != null)
            {
                var binding = new Binding("Text") { Source = Command };
                SetBinding(ContentProperty, binding);
            }
        }
    }
}
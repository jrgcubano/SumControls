namespace SumControls.Controls
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;

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
            DependencyPropertyDescriptor.FromProperty(CommandProperty, typeof(ButtonBase))
                .AddValueChanged(this, (s, e) => CommandChanged());
        }

        /// <summary>
        /// Updates the Content binding when the Command property is changed
        /// </summary>
        private void CommandChanged()
        {
            if (Command is RoutedUICommand)
            {
                SetBinding(ContentProperty, new Binding("Text") { Source = Command });
            }
        }
    }
}
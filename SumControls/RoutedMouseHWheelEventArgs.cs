namespace SumControls
{
    using System.Windows;

    /// <summary>
    /// Contains state information and event data associated with a horizontal tilt event
    /// </summary>
    public class RoutedMouseHWheelEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the RoutedMouseHWheelEventArgs class
        /// </summary>
        public RoutedMouseHWheelEventArgs() {}

        /// <summary>
        /// Initializes a new instance of the RoutedMouseHWheelEventArgs class
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance</param>
        public RoutedMouseHWheelEventArgs(RoutedEvent routedEvent) : base(routedEvent) {}

        /// <summary>
        /// Initializes a new instance of the RoutedMouseHWheelEventArgs class
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This
        /// pre-populates the System.Windows.RoutedEventArgs.Source property.</param>
        public RoutedMouseHWheelEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) {}

        /// <summary>
        /// Gets or sets the delta of the mouse horizontal tilt
        /// </summary>
        public int Delta { get; set; }
    }
}
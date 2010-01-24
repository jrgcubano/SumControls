namespace SumControls.Gestures
{
    using System.Windows.Input;

    #region MouseWheelAction enum

    /// <summary>
    /// Specifies constants that define actions performed by the mouse wheel
    /// </summary>
    public enum MouseWheelAction
    {
        /// <summary>
        /// The mouse wheel was scrolled in either direction
        /// </summary>
        AllMovement,

        /// <summary>
        /// The mouse wheel was scrolled vertically
        /// </summary>
        WheelUp,

        /// <summary>
        /// The mouse wheel was scrolled horizontally
        /// </summary>
        WheelDown
    }

    #endregion MouseWheelAction enum

    /// <summary>
    /// Defines a mouse wheel input gesture that can be used to invoke a command
    /// </summary>
    public class MouseWheelGesture : MouseGesture
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MouseWheelGesture class
        /// </summary>
        public MouseWheelGesture()
            : base(MouseAction.WheelClick)
        {
            MouseWheelAction = MouseWheelAction.AllMovement;
        }

        /// <summary>
        /// Initializes a new instance of the MouseWheelGesture class
        /// </summary>
        /// <param name="mouseWheelAction">The action associated with this gesture</param>
        public MouseWheelGesture(MouseWheelAction mouseWheelAction)
            : base(MouseAction.WheelClick)
        {
            MouseWheelAction = mouseWheelAction;
        }

        /// <summary>
        /// Initializes a new instance of the MouseWheelGesture class
        /// </summary>
        /// <param name="mouseWheelAction">The action associated with this gesture</param>
        /// <param name="modifiers">The modifiers associated with this gesture</param>
        public MouseWheelGesture(MouseWheelAction mouseWheelAction, ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers)
        {
            MouseWheelAction = mouseWheelAction;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the MouseWheelAction associated with this gesture
        /// </summary>
        public MouseWheelAction MouseWheelAction { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether MouseWheelGesture matches the input associated with the specified InputEventArgs object
        /// </summary>
        /// <param name="targetElement">The target</param>
        /// <param name="inputEventArgs">The input event data to compare with this gesture</param>
        /// <returns>true if the event data matches this MouseWheelGesture</returns>
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (base.Matches(targetElement, inputEventArgs))
            {
                var wheelArgs = inputEventArgs as MouseWheelEventArgs;
                if (wheelArgs != null)
                {
                    if (MouseWheelAction == MouseWheelAction.AllMovement ||
                        (MouseWheelAction == MouseWheelAction.WheelDown && wheelArgs.Delta < 0) ||
                            (MouseWheelAction == MouseWheelAction.WheelUp && wheelArgs.Delta > 0))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Methods
    }
}
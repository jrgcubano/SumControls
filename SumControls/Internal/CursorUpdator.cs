namespace SumControls.Internal
{
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;

    using Properties;

    /// <summary>
    /// Helper class to update the Cursor of a FrameworkElement
    /// </summary>
    internal class CursorUpdator
    {
        #region Fields

        private static readonly Cursor PanCursor = new Cursor(new MemoryStream(Resources.Pan));
        private static readonly Cursor PanClosedCursor = new Cursor(new MemoryStream(Resources.PanClosed));

        private readonly FrameworkElement _control;
        private Cursor _backup;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the CursorUpdator class
        /// @pre control is not null
        /// </summary>
        /// <param name="control">The FrameworkElement to change the Cursor of</param>
        public CursorUpdator(FrameworkElement control)
        {
            Debug.Assert(control != null, "control must not be null");
            _control = control;
        }

        #region Public methods

        /// <summary>
        /// Creates a backup of the current cursor
        /// </summary>
        public void Backup()
        {
            _backup = _control.Cursor;
        }

        /// <summary>
        /// Restores the Cursor back to its original
        /// @pre Backup() has been called previously
        /// </summary>
        public void Restore()
        {
            _control.Cursor = _backup;
        }

        /// <summary>
        /// Updates the Cursor to ScrollAll
        /// </summary>
        public void ScrollAll()
        {
            _control.Cursor = Cursors.ScrollAll;
        }

        /// <summary>
        /// Updates the Cursor to ScrollW
        /// </summary>
        public void ScrollLeft()
        {
            _control.Cursor = Cursors.ScrollW;
        }

        /// <summary>
        /// Updates the Cursor to ScrollE
        /// </summary>
        public void ScrollRight()
        {
            _control.Cursor = Cursors.ScrollE;
        }

        /// <summary>
        /// Updates the Cursor to ScrollN
        /// </summary>
        public void ScrollTop()
        {
            _control.Cursor = Cursors.ScrollN;
        }

        /// <summary>
        /// Updates the Cursor to ScrollS
        /// </summary>
        public void ScrollBottom()
        {
            _control.Cursor = Cursors.ScrollS;
        }

        /// <summary>
        /// Updates the Cursor to ScrollNE
        /// </summary>
        public void ScrollTopRight()
        {
            _control.Cursor = Cursors.ScrollNE;
        }

        /// <summary>
        /// Updates the Cursor to ScrollSE
        /// </summary>
        public void ScrollBottomRight()
        {
            _control.Cursor = Cursors.ScrollSE;
        }

        /// <summary>
        /// Updates the Cursor to ScrollSW
        /// </summary>
        public void ScrollBottomLeft()
        {
            _control.Cursor = Cursors.ScrollSW;
        }

        /// <summary>
        /// Updates the Cursor to ScrollNW
        /// </summary>
        public void ScrollTopLeft()
        {
            _control.Cursor = Cursors.ScrollNW;
        }

        /// <summary>
        /// Updates the Cursor to Pan
        /// </summary>
        public void Pan()
        {
            _control.Cursor = PanCursor;
        }

        /// <summary>
        /// Updates the Cursor to PanClosed
        /// </summary>
        public void PanClosed()
        {
            _control.Cursor = PanClosedCursor;
        }

        #endregion Public methods
    }
}
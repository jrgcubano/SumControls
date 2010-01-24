namespace SumControls.Controls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    using Internal;

    using Timer = System.Timers.Timer;

    /// <summary>
    /// Represents a scrollable area that can contain other visible elements, and handles scrolling by the cursor
    /// </summary>
    public class ExtendedScrollViewer : ScrollViewer, IDisposable
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the PreviewMouseHWheel routed event
        /// </summary>
        public static readonly RoutedEvent PreviewMouseHWheelEvent =
            EventManager.RegisterRoutedEvent("PreviewMouseHWheel", RoutingStrategy.Tunnel,
                typeof(EventHandler<RoutedMouseHWheelEventArgs>), typeof(ExtendedScrollViewer));

        /// <summary>
        /// Identifies the MouseHWheelEvent event
        /// </summary>
        public static readonly RoutedEvent MouseHWheelEvent =
            EventManager.RegisterRoutedEvent("MouseHWheel", RoutingStrategy.Bubble,
                typeof(EventHandler<RoutedMouseHWheelEventArgs>), typeof(ExtendedScrollViewer));

        /// <summary>
        /// Identifies the AllowCursorScrolling dependency property
        /// </summary>
        public static readonly DependencyProperty AllowCursorScrollingProperty =
            DependencyProperty.Register("AllowCursorScrolling", typeof(bool), typeof(ExtendedScrollViewer),
                new PropertyMetadata(true));

        /// <summary>
        /// Identifies the AllowPanning dependency property
        /// </summary>
        public static readonly DependencyProperty AllowPanningProperty =
            DependencyProperty.Register("AllowPanning", typeof(bool), typeof(ExtendedScrollViewer),
                new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PanKey dependency property
        /// </summary>
        public static readonly DependencyProperty PanKeyProperty =
            DependencyProperty.Register("PanKey", typeof(Key), typeof(ExtendedScrollViewer),
                new PropertyMetadata(Key.Space));

        /// <summary>
        /// Identifies the DrawStartIndicator dependency property
        /// </summary>
        public static readonly DependencyProperty DrawStartIndicatorProperty =
            DependencyProperty.Register("DrawStartIndicator", typeof(bool), typeof(ExtendedScrollViewer),
                new PropertyMetadata(true));

        #endregion Dependency properties

        #region Constants

        // ReSharper disable InconsistentNaming
        private const int WM_MOUSEHWHEEL = 0x020E;
        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        private const double INDICATOR_SIZE = 4D;
        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        private const double INDICATOR_OFFSET = 1D;
        // ReSharper restore InconsistentNaming

        #endregion Constants

        #region Instance variables

        private readonly Scroller _scroller;
        private readonly Panner _panner;
        private readonly CursorUpdator _cursor;
        private readonly Shape _indicator = new Ellipse();

        #endregion Instance variables

        /// <summary>
        /// Initializes a new instance of the ExtendedScrollViewer class
        /// </summary>
        public ExtendedScrollViewer()
        {
            _cursor = new CursorUpdator(this);
            _scroller = new Scroller(this);
            _panner = new Panner(this);

            _indicator.Stroke = Brushes.Black;
            _indicator.Fill = Brushes.Black;
            _indicator.Width = INDICATOR_SIZE;
            _indicator.Height = INDICATOR_SIZE;
            _indicator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            Loaded += ExtendedScrollViewer_Loaded;
        }

        #region Routed events

        /// <summary>
        /// Occurs when the user rotates the horizontal mouse wheel while the mouse pointer is over this element
        /// </summary>
        public event EventHandler<RoutedMouseHWheelEventArgs> PreviewMouseHWheel
        {
            add { AddHandler(PreviewMouseHWheelEvent, value); }
            remove { RemoveHandler(PreviewMouseHWheelEvent, value); }
        }

        /// <summary>
        /// Occurs when the user rotates the horizontal mouse wheel while the mouse pointer is over this element
        /// </summary>
        public event EventHandler<RoutedMouseHWheelEventArgs> MouseHWheel
        {
            add { AddHandler(MouseHWheelEvent, value); }
            remove { RemoveHandler(MouseHWheelEvent, value); }
        }

        #endregion Routed events

        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether a value indicating whether to allow scrolling using the cursor when
        /// the middle button is clicked. This is a dependency property
        /// </summary>
        [DefaultValue(true)]
        [Category("Behaviour")]
        public bool AllowCursorScrolling
        {
            get { return (bool)GetValue(AllowCursorScrollingProperty); }
            set { SetValue(AllowCursorScrollingProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow panning of the content. This is a dependency property
        /// </summary>
        [DefaultValue(true)]
        [Category("Behaviour")]
        public bool AllowPanning
        {
            get { return (bool)GetValue(AllowPanningProperty); }
            set { SetValue(AllowPanningProperty, value); }
        }

        /// <summary>
        /// Gets or sets the key to start the panning process when the left mouse button is pressed and AllowPanning is
        /// true. Default is Key.Space. This is a dependency property
        /// </summary>
        [Category("Behaviour")]
        public Key PanKey
        {
            get { return (Key)GetValue(PanKeyProperty); }
            set { SetValue(PanKeyProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to draw an indicator where the scrolling began
        /// </summary>
        [DefaultValue(true)]
        [Category("Appearance")]
        public bool DrawStartIndicator
        {
            get { return (bool)GetValue(DrawStartIndicatorProperty); }
            set { SetValue(DrawStartIndicatorProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the control is able to scroll further up
        /// </summary>
        public bool CanScrollUp
        {
            get { return VerticalOffset > 0D; }
        }

        /// <summary>
        /// Gets a value indicating whether the control is able to scroll further down
        /// </summary>
        public bool CanScrollDown
        {
            get { return VerticalOffset < ExtentHeight - ViewportHeight; }
        }

        /// <summary>
        /// Gets a value indicating whether the control is able to scroll further right
        /// </summary>
        public bool CanScrollRight
        {
            get { return HorizontalOffset < ExtentWidth - ViewportWidth; }
        }

        /// <summary>
        /// Gets a value indicating whether the control is able to scroll further left
        /// </summary>
        public bool CanScrollLeft
        {
            get { return HorizontalOffset > 0D; }
        }

        #endregion Public properties

        /// <summary>
        /// Gets the number of visual child elements within this element
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                var visualChildrenCount = base.VisualChildrenCount;
                if (visualChildrenCount == 1 && DrawStartIndicator)
                {
                    return visualChildrenCount + 1;
                }

                return visualChildrenCount;
            }
        }

        #region Protected methods

        /// <summary>
        /// Processes Windows messages
        /// </summary>
        /// <param name="hwnd">The window handle of the message</param>
        /// <param name="msg">The Windows Message to process</param>
        /// <param name="wParam">The WParam field of the message</param>
        /// <param name="lParam">The LParam field of the message</param>
        /// <param name="handled">The parameter is not used.</param>
        /// <returns>Always returns IntPtr.Zero</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#",
            Justification = "This is a requirement")]
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEHWHEEL)
            {
                FireMouseHWheel(wParam);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Responds to the horizontal tilt of a mouse
        /// </summary>
        /// <param name="e">Required arguments that describe this event</param>
        protected virtual void OnPreviewMouseHWheel(RoutedMouseHWheelEventArgs e) {}

        /// <summary>
        /// Responds to the horizontal tilt of a mouse
        /// </summary>
        /// <param name="e">Required arguments that describe this event</param>
        protected virtual void OnMouseHWheel(RoutedMouseHWheelEventArgs e)
        {
            ScrollToHorizontalOffset(HorizontalOffset + e.Delta);
        }

        #endregion Protected methods

        #region Overriden methods

        /// <summary>
        /// Raises the System.Windows.FrameworkElement.Initialized event. This method is invoked whenever
        /// System.Windows.FrameworkElement.IsInitialized is set to true internally.
        /// </summary>
        /// <param name="e">The System.Windows.RoutedEventArgs that contains the event data</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _cursor.Backup();
        }

        /// <summary>
        /// Responds to the click of a mouse button
        /// </summary>
        /// <param name="e">Required arguments that describe this event</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Handled)
            {
                return;
            }

            if (AllowCursorScrolling)
            {
                if (_scroller.IsScrolling)
                {
                    Focus();
                    FinishScrolling();
                    e.Handled = true;
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    StartScrolling(e.GetPosition(this));
                    e.Handled = true;
                }
            }

            if (AllowPanning && e.LeftButton == MouseButtonState.Pressed)
            {
                _panner.MouseDown(e.GetPosition(this));
            }
        }

        /// <summary>
        /// Responds to the release of a mouse button
        /// </summary>
        /// <param name="e">Required arguments that describe this event</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (AllowPanning && !e.Handled && e.LeftButton == MouseButtonState.Released)
            {
                _panner.MouseUp();
            }
        }

        /// <summary>
        /// Responds to specific keyboard input and invokes associated scrolling behavior
        /// </summary>
        /// <param name="e">Required arguments for this event</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            if (AllowCursorScrolling && _scroller.IsScrolling)
            {
                FinishScrolling();
            }

            if (AllowPanning && e.Key == PanKey)
            {
                _panner.KeyDown();
            }
        }

        /// <summary>
        /// Responds to specific keyboard input and invokes associated scrolling behavior
        /// </summary>
        /// <param name="e">Required arguments for this event</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (AllowPanning && !e.Handled && e.Key == PanKey)
            {
                _panner.KeyUp();
            }
        }

        /// <summary>
        /// Responds to the movement of the mouse
        /// </summary>
        /// <param name="e">Required arguments that describe this event</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Handled)
            {
                return;
            }

            if (AllowCursorScrolling && _scroller.IsScrolling)
            {
                _scroller.Continue(e.GetPosition(this));
                e.Handled = true;
            }

            if (AllowPanning && _panner.IsPanning)
            {
                _panner.Continue(e.GetPosition(this));
            }
        }

        /// <summary>
        /// Responds to a click of the mouse wheel
        /// </summary>
        /// <param name="e">Required arguments that describe this event</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (AllowCursorScrolling && _scroller.IsScrolling)
            {
                FinishScrolling();
            }

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Called to arrange and size the content the instance
        /// </summary>
        /// <param name="arrangeBounds">The computed size that is used to arrange the content</param>
        /// <returns>The size of the control</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(arrangeBounds);

            if (_scroller.IsScrolling && DrawStartIndicator)
            {
                var location = new Point(_scroller.StartPosition.X - INDICATOR_OFFSET,
                    _scroller.StartPosition.Y - INDICATOR_OFFSET);
                _indicator.Arrange(new Rect(location, _indicator.DesiredSize));
            }

            return size;
        }

        /// <summary>
        /// Returns a child at the specified index from a collection of child elements
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection</param>
        /// <returns>The requested child element</returns>
        protected override Visual GetVisualChild(int index)
        {
            return index == 1 ? _indicator : base.GetVisualChild(index);
        }

        #endregion Overriden methods

        #region Private methods

        /// <summary>
        /// Start the process of scrolling by the cursor
        /// </summary>
        /// <param name="start">The current mouse position</param>
        private void StartScrolling(Point start)
        {
            Focus();
            _scroller.Start(start);
            CaptureMouse();
        }

        /// <summary>
        /// Completes the process of scrolling by the cursor
        /// </summary>
        private void FinishScrolling()
        {
            _scroller.Finish();
            ReleaseMouseCapture();
        }

        /// <summary>
        /// Translates the given parameters to a MouseEventArgs instance and raises the related MouseHWheel events
        /// </summary>
        /// <param name="wParam">The WParam field of the message</param>
        private void FireMouseHWheel(IntPtr wParam)
        {
            RaiseMouseHWheelEvent((short)Functions.HiWord(wParam));
        }

        /// <summary>
        /// Raises the PreviewMouseHWheelEvent and MouseHWheelEvent routed events
        /// </summary>
        /// <param name="delta">The delta of the horizontal tilt</param>
        private void RaiseMouseHWheelEvent(int delta)
        {
            var tunnel = new RoutedMouseHWheelEventArgs(PreviewMouseHWheelEvent) { Delta = delta };
            OnPreviewMouseHWheel(tunnel);
            RaiseEvent(tunnel);

            if (!tunnel.Handled)
            {
                var bubble = new RoutedMouseHWheelEventArgs(MouseHWheelEvent) { Delta = delta };
                OnMouseHWheel(bubble);
                RaiseEvent(bubble);
            }
        }

        /// <summary>
        /// Adds the WndProc hook
        /// </summary>
        private void AddWndProcHook()
        {
            var source = PresentationSource.FromDependencyObject(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        /// <summary>
        /// Creates the WndProc hook when the control is loaded
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ExtendedScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            AddWndProcHook();
        }

        #endregion Private methods

        #region Private classes

        #region Private Panner class

        /// <summary>
        /// Helper class to pan within the control
        /// </summary>
        private class Panner
        {
            #region Instance variables

            private readonly ExtendedScrollViewer _control;
            private Point _start;
            private bool _keyDown;
            private bool _mouseDown;

            #endregion Instance variables

            /// <summary>
            /// Initializes a new instance of the Panner class
            /// @pre control is not null
            /// </summary>
            /// <param name="control">The ExtendedScrollViewer to scroll</param>
            public Panner(ExtendedScrollViewer control)
            {
                Debug.Assert(control != null, "control must not be null");
                _control = control;
            }

            /// <summary>
            /// Gets a value indicating whether the Panner is currently in the process of panning
            /// </summary>
            public bool IsPanning { get; private set; }

            #region Public methods

            /// <summary>
            /// When called signals that the panning key has been pressed
            /// </summary>
            public void KeyDown()
            {
                _keyDown = true;
                CheckPan();
                UpdateCursor();
            }

            /// <summary>
            /// When called signals that the panning key has been released
            /// </summary>
            public void KeyUp()
            {
                _keyDown = false;
                CheckPan();
                UpdateCursor();
            }

            /// <summary>
            /// When called signals that the the panning mouse button has beeen pressed
            /// </summary>
            /// <param name="position">The current position of the mouse</param>
            public void MouseDown(Point position)
            {
                _mouseDown = true;
                _start = position;
                CheckPan();
            }

            /// <summary>
            /// When called signals that the the panning mouse button has beeen released
            /// </summary>
            public void MouseUp()
            {
                _mouseDown = false;
                CheckPan();
            }

            /// <summary>
            /// Continues the process of panning
            /// @pre IsPanning is true
            /// </summary>
            /// <param name="position">The current position of the mouse</param>
            public void Continue(Point position)
            {
                Debug.Assert(IsPanning, "IsPanning must be true");

                var x = position.X - _start.X;
                var y = position.Y - _start.Y;

                if (x != 0D)
                {
                    _control.ScrollToHorizontalOffset(_control.HorizontalOffset - x);
                }

                if (y != 0D)
                {
                    _control.ScrollToVerticalOffset(_control.VerticalOffset - y);
                }

                _start = position;
            }

            #endregion Public methods

            #region Private methods

            /// <summary>
            /// Starts the process of panning
            /// @pre IsPanning is false
            /// @post IsPanning is true
            /// </summary>
            private void Start()
            {
                Debug.Assert(!IsPanning, "IsPanning must be false");

                IsPanning = true;
                _control.CaptureMouse();

                UpdateCursor();
            }

            /// <summary>
            /// Completes the process of panning
            /// @pre IsPanning is true
            /// @post IsPanning is false
            /// </summary>
            private void Finish()
            {
                Debug.Assert(IsPanning, "IsPanning must be true");

                _keyDown = false;
                _mouseDown = false;
                IsPanning = false;
                _control.ReleaseMouseCapture();
            }

            /// <summary>
            /// Determines whether the panning process should be started or finished
            /// </summary>
            private void CheckPan()
            {
                if (!IsPanning)
                {
                    if (_keyDown && _mouseDown)
                    {
                        Start();
                    }
                }
                else
                {
                    if (!_keyDown || !_mouseDown)
                    {
                        Finish();
                    }
                }
            }

            /// <summary>
            /// Updates the cursor display
            /// </summary>
            private void UpdateCursor()
            {
                if (IsPanning)
                {
                    _control._cursor.PanClosed();
                }
                else if (_keyDown)
                {
                    _control._cursor.Pan();
                }
                else
                {
                    _control._cursor.Restore();
                }
            }

            #endregion Private methods
        }

        #endregion Private Panner class

        #region Private Scroller class

        /// <summary>
        /// Helper class to scroll the control by the cursor
        /// </summary>
        private sealed class Scroller : IDisposable
        {
            #region Constants

            private const double Offset = 10D;
            private const double DistanceOffset = Offset - 1D;
            private const double DistanceConstant = 0.00001D;

            [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional",
                MessageId = "Member", Justification = "This defines a matrix")] private static readonly ScrollMode[,] Matrix = new[,]
                {
                    { ScrollMode.TopLeft, ScrollMode.Top, ScrollMode.TopRight },
                    { ScrollMode.Left, ScrollMode.None, ScrollMode.Right },
                    { ScrollMode.BottomLeft, ScrollMode.Bottom, ScrollMode.BottomRight }
                };

            #endregion Constants

            #region Fields

            private readonly Timer _timer = new Timer(1D);
            private readonly ExtendedScrollViewer _control;
            private Point _position;
            private double _left, _right, _top, _bottom;
            private bool _disposed;

            #endregion Fields

            /// <summary>
            /// Initializes a new instance of the Scroller class
            /// @pre control is not null
            /// </summary>
            /// <param name="control">The ExtendedScrollViewer to scroll</param>
            public Scroller(ExtendedScrollViewer control)
            {
                Debug.Assert(control != null, "control must not be null");
                _control = control;

                _timer.Elapsed += Timer_Elapsed;
            }

            #region Private ScrollMode enum

            /// <summary>
            /// Represents the different scrolling modes
            /// </summary>
            private enum ScrollMode
            {
                /// <summary>
                /// Scroll to the top left
                /// </summary>
                TopLeft,

                /// <summary>
                /// Scroll to the top
                /// </summary>
                Top,

                /// <summary>
                /// Scroll to the top right
                /// </summary>
                TopRight,

                /// <summary>
                /// Scroll to theleft
                /// </summary>
                Left,

                /// <summary>
                /// No scrolling mode
                /// </summary>
                None,

                /// <summary>
                /// Scroll to the right
                /// </summary>
                Right,

                /// <summary>
                /// Scroll to the bottom left
                /// </summary>
                BottomLeft,

                /// <summary>
                /// Scroll to the bottom
                /// </summary>
                Bottom,

                /// <summary>
                /// Scroll to the bottom right
                /// </summary>
                BottomRight
            }

            #endregion Private ScrollMode enum

            /// <summary>
            /// Gets the start position where the scrolling began
            /// @pre IsScrolling is true
            /// </summary>
            public Point StartPosition { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the Scroller is currently in the process of scrolling
            /// </summary>
            public bool IsScrolling { get; private set; }

            /// <summary>
            /// Gets the current scrolling mode
            /// @pre IsScrolling is true
            /// </summary>
            private ScrollMode Mode
            {
                get
                {
                    var row = _position.Y < _top ? 0 // TopLeft || Top || TopRight
                        : _position.Y > _bottom ? 2 // BottomLeft || Bottom || BottomRight
                            : 1; // Left || None || Right

                    var column = _position.X < _left ? 0 // TopLeft || Left || BottomLeft
                        : _position.X > _right ? 2 // TopRight || Right || BottomRight
                            : 1; // Top || None || Bottom

                    return Matrix[row, column];
                }
            }

            #region Public methods

            /// <summary>
            /// Starts the process of scrolling by the cursor
            /// @pre IsScrolling is false
            /// @post IsScrolling is true
            /// </summary>
            /// <param name="start">The current position of the mouse</param>
            public void Start(Point start)
            {
                Debug.Assert(!IsScrolling, "IsScrolling must be false");

                StartPosition = start;
                _control._cursor.ScrollAll();
                IsScrolling = true;

                _left = StartPosition.X - Offset;
                _right = StartPosition.X + Offset;
                _top = StartPosition.Y - Offset;
                _bottom = StartPosition.Y + Offset;

                _timer.Start();

                _control._indicator.Visibility = Visibility.Visible;
            }

            /// <summary>
            /// Continues the process of scrolling by the cursor
            /// @pre IsScrolling is true
            /// </summary>
            /// <param name="position">The current position of the mouse</param>
            public void Continue(Point position)
            {
                Debug.Assert(IsScrolling, "IsScrolling must be true");

                // Update the mouse position. The Continue() method will then use the updated value when the timer
                // has elapsed
                _position = position;
            }

            /// <summary>
            /// Completes the process of scrolling by the cursor
            /// @pre IsScrolling is true
            /// @post IsScrolling is false
            /// </summary>
            public void Finish()
            {
                Debug.Assert(IsScrolling, "IsScrolling must be true");

                IsScrolling = false;
                _timer.Stop();
                _control._cursor.Restore();

                _control._indicator.Visibility = Visibility.Hidden;
                _control.InvalidateMeasure();
            }

            #endregion Public methods

            #region Private data

            #region Private methods

            /// <summary>
            /// Calls the Continue() after every elapse of the timer
            /// </summary>
            /// <param name="sender">The parameter is not used.</param>
            /// <param name="e">The parameter is not used.</param>
            private void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                // TODO: Consider locking
                _control.Dispatcher.BeginInvoke((ThreadStart)Continue, DispatcherPriority.Normal);
            }

            #region Continue scrolling methods

            /// <summary>
            /// Continues the scrolling behaviour in an exponential fashion
            /// </summary>
            private void Continue()
            {
                if (!IsScrolling)
                {
                    return;
                }

                switch (Mode)
                {
                    case ScrollMode.TopLeft:
                        TopLeft();
                        break;

                    case ScrollMode.Top:
                        Top();
                        break;

                    case ScrollMode.TopRight:
                        TopRight();
                        break;

                    case ScrollMode.Left:
                        Left();
                        break;

                    case ScrollMode.Right:
                        Right();
                        break;

                    case ScrollMode.BottomLeft:
                        BottomLeft();
                        break;

                    case ScrollMode.Bottom:
                        Bottom();
                        break;

                    case ScrollMode.BottomRight:
                        BottomRight();
                        break;

                    case ScrollMode.None:
                        _control._cursor.ScrollAll();
                        break;
                }
            }

            /// <summary>
            /// Handles scrolling in top left direction
            /// </summary>
            private void TopLeft()
            {
                if (_control.CanScrollUp && _control.CanScrollLeft)
                {
                    _control._cursor.ScrollTopLeft();
                    ScrollUp();
                    ScrollLeft();
                }
                else if (_control.CanScrollUp)
                {
                    _control._cursor.ScrollTop();
                    ScrollUp();
                }
                else if (_control.CanScrollLeft)
                {
                    _control._cursor.ScrollLeft();
                    ScrollLeft();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in top direction
            /// </summary>
            private void Top()
            {
                if (_control.CanScrollUp)
                {
                    _control._cursor.ScrollTop();
                    ScrollUp();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in down direction
            /// </summary>
            private void Bottom()
            {
                if (_control.CanScrollDown)
                {
                    _control._cursor.ScrollBottom();
                    ScrollDown();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in left direction
            /// </summary>
            private void Left()
            {
                if (_control.CanScrollLeft)
                {
                    _control._cursor.ScrollLeft();
                    ScrollLeft();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in right direction
            /// </summary>
            private void Right()
            {
                if (_control.CanScrollRight)
                {
                    _control._cursor.ScrollRight();
                    ScrollRight();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in top right direction
            /// </summary>
            private void TopRight()
            {
                if (_control.CanScrollUp && _control.CanScrollRight)
                {
                    _control._cursor.ScrollTopRight();
                    ScrollUp();
                    ScrollRight();
                }
                else if (_control.CanScrollUp)
                {
                    _control._cursor.ScrollTop();
                    ScrollUp();
                }
                else if (_control.CanScrollRight)
                {
                    _control._cursor.ScrollRight();
                    ScrollLeft();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in bottom left direction
            /// </summary>
            private void BottomLeft()
            {
                if (_control.CanScrollDown && _control.CanScrollLeft)
                {
                    _control._cursor.ScrollBottomLeft();
                    ScrollDown();
                    ScrollLeft();
                }
                else if (_control.CanScrollDown)
                {
                    _control._cursor.ScrollBottom();
                    ScrollDown();
                }
                else if (_control.CanScrollLeft)
                {
                    _control._cursor.ScrollLeft();
                    ScrollLeft();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            /// <summary>
            /// Handles scrolling in bottom right direction
            /// </summary>
            private void BottomRight()
            {
                if (_control.CanScrollDown && _control.CanScrollRight)
                {
                    _control._cursor.ScrollBottomRight();
                    ScrollDown();
                    ScrollRight();
                }
                else if (_control.CanScrollDown)
                {
                    _control._cursor.ScrollBottom();
                    ScrollDown();
                }
                else if (_control.CanScrollRight)
                {
                    _control._cursor.ScrollRight();
                    ScrollRight();
                }
                else
                {
                    _control._cursor.ScrollAll();
                }
            }

            #endregion Continue scrolling methods

            #region Scrolling methods

            /// <summary>
            /// Scrolls upwards exponentially
            /// </summary>
            private void ScrollUp()
            {
                _control.ScrollToVerticalOffset(_control.VerticalOffset - GetDistance(StartPosition.Y - _position.Y));
            }

            /// <summary>
            /// Scrolls downwards exponentially
            /// </summary>
            private void ScrollDown()
            {
                _control.ScrollToVerticalOffset(_control.VerticalOffset + GetDistance(_position.Y - StartPosition.Y));
            }

            /// <summary>
            /// Scrolls left exponentially
            /// </summary>
            private void ScrollLeft()
            {
                _control.ScrollToHorizontalOffset(_control.HorizontalOffset - GetDistance(StartPosition.X - _position.X));
            }

            /// <summary>
            /// Scrolls right exponentially
            /// </summary>
            private void ScrollRight()
            {
                _control.ScrollToHorizontalOffset(_control.HorizontalOffset + GetDistance(_position.X - StartPosition.X));
            }

            /// <summary>
            /// Gets the distance to scroll based on the given delta. The distance increases exponentially based on the
            /// given delta
            /// </summary>
            /// <param name="delta">The delta to use to determine the distance to scroll</param>
            /// <returns>The distance to scroll</returns>
            private static double GetDistance(double delta)
            {
                delta = delta + 1D - DistanceOffset;
                return DistanceConstant * (delta * delta * delta);
            }

            #endregion Scrolling methods

            #endregion Private methods

            #endregion Private data

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
            /// </summary>
            public void Dispose()
            {
                if (!_disposed)
                {
                    _timer.Dispose();
                    _disposed = true;
                }
            }
        }

        #endregion Private Scroller class

        #endregion Private classes

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether or not to dispose the managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _scroller.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the ExtendedScrollViewer class
        /// </summary>
        ~ExtendedScrollViewer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
namespace SumControls.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Arranges and positions Sortable children elements in a horizontal fashion, and provides the ability for the
    /// user to arrange elements in a custom order
    /// </summary>
    public class SortablePanel : CustomPanel<Sortable>
    {
        #region Fields

        /// <summary>
        /// Identifies the Left attached property
        /// </summary>
        private static readonly DependencyProperty LeftProperty =
            DependencyProperty.RegisterAttached("Left", typeof(double), typeof(SortablePanel),
                new FrameworkPropertyMetadata(0D, FrameworkPropertyMetadataOptions.AffectsParentArrange, null,
                    CoerceLeft));

        private readonly List<Sortable> _ordered = new List<Sortable>();
        private readonly Moving _moving = new Moving();
        private Size _size;
        private bool _isSorted = true;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the SortablePanel class
        /// </summary>
        public SortablePanel()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
        }

        #region Public properties

        /// <summary>
        /// Gets the ordered list of Sortable elements this SortablePanel holds
        /// </summary>
        public ReadOnlyCollection<Sortable> OrderedSortables
        {
            get { return _ordered.AsReadOnly(); }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Determines whether the given element is able to move one position left
        /// </summary>
        /// <param name="sortable">The element to test</param>
        /// <returns>true if the element can move one position left</returns>
        public bool CanMoveLeft(Sortable sortable)
        {
            return _ordered.Count > 1 && _ordered.IndexOf(sortable) > 0;
        }

        /// <summary>
        /// Determines whether the given element is able to move one position right
        /// </summary>
        /// <param name="sortable">The element to test</param>
        /// <returns>true if the element can move one position right</returns>
        public bool CanMoveRight(Sortable sortable)
        {
            return _ordered.Count > 1 && _ordered.IndexOf(sortable) < _ordered.Count - 1;
        }

        /// <summary>
        /// Moves the given element one position left
        /// @pre: CanMoveLeft(sortable) returned true
        /// </summary>
        /// <param name="sortable">The element to move</param>
        public void MoveLeft(Sortable sortable)
        {
            _ordered.SwapPrevious(_ordered.IndexOf(sortable));
            _isSorted = false;
            InvalidateArrange();
        }

        /// <summary>
        /// Moves the given element one position right
        /// @pre: CanMoveRight(sortable) returned true
        /// </summary>
        /// <param name="sortable">The element to move</param>
        public void MoveRight(Sortable sortable)
        {
            _ordered.SwapNext(_ordered.IndexOf(sortable));
            _isSorted = false;
            InvalidateArrange();
        }

        #endregion Public methods

        #region Protected methods

        #region Overriden methods

        /// <summary>
        /// Handles the PreviewMouseMove event
        /// </summary>
        /// <param name="e">The mouse event arguments</param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            ContinueMoving(e);
        }

        /// <summary>
        /// Handles the PreviewMouseUp event
        /// </summary>
        /// <param name="e">The mouse button event arguments</param>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (e.LeftButton == MouseButtonState.Released)
            {
                FinishMoving();
            }
        }

        /// <summary>
        /// Measures the size and layout required for child elements and determines a size for the panel
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements</param>
        /// <returns>The size that this element determines it needs during layout, based on its child
        /// elements</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            double desiredHeight = 0;
            double desiredWidth = 0;

            foreach (Sortable element in Items)
            {
                element.Measure(availableSize);
                var height = element.DesiredSize.Height;
                var width = element.DesiredSize.Width;
                desiredHeight = Math.Max(height, desiredHeight);
                desiredWidth += width;
            }

            _size = new Size(desiredWidth, desiredHeight);
            return _size;
        }

        /// <summary>
        /// Positions the child elements and determines the size of the instance
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and
        /// its children</param>
        /// <returns>The actual size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Display(); // The Arrage method needs to be called on all elements before sorting

            if (!_isSorted)
            {
                Sort();
                Display();
            }

            return _size;
        }

        /// <summary>
        /// Called when an item is added to the panel. Override to handle this event
        /// </summary>
        /// <param name="item">The new item which was added</param>
        protected override void OnItemAdded(Sortable item)
        {
            IntializeSortable(item);
        }

        /// <summary>
        /// Called when an iten is removed from the panel. Override to handle this event
        /// </summary>
        /// <param name="item">The item which was removed</param>
        protected override void OnItemRemoved(Sortable item)
        {
            DeinitializeSortable(item);
        }

        #endregion Overriden methods

        #endregion Protected methods

        #region Dependency properties

        /// <summary>
        /// Coerces the given double value so that it is above 0 and not NaN or Infinity
        /// </summary>
        /// <param name="d">The parameter is not used.</param>
        /// <param name="newValue">The double value to coerce</param>
        /// <returns>The coerced double value</returns>
        private static object CoerceLeft(DependencyObject d, object newValue)
        {
            var value = (double)newValue;
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                value = 0;
            }
            else if (value < 0)
            {
                value = 0;
            }

            return value;
        }

        /// <summary>
        /// Sets the value of the Left property for the given element
        /// </summary>
        /// <exception cref="ArgumentNullException">If sortable is null</exception>
        /// <param name="sortable">The element on which to apply the property value</param>
        /// <param name="value">The Left position which the element should have</param>
        private static void SetLeft(Sortable sortable, double value)
        {
            if (sortable == null)
            {
                throw new ArgumentNullException("sortable");
            }

            sortable.SetValue(LeftProperty, value);
        }

        /// <summary>
        /// Gets the value of the Left property for the given element
        /// </summary>
        /// <exception cref="ArgumentNullException">If sortable is null</exception>
        /// <param name="sortable">The element for which to retrieve the Left value</param>
        /// <returns>The Left position of the element</returns>
        private static double GetLeft(Sortable sortable)
        {
            if (sortable == null)
            {
                throw new ArgumentNullException("sortable");
            }

            return (double)sortable.GetValue(LeftProperty);
        }

        #endregion Dependency properties

        #region Private methods

        /// <summary>
        /// Determines the middle point in the X-axis for the given element
        /// </summary>
        /// <param name="sortable">The element to retrieve the middle point from</param>
        /// <returns>The middle point along the X-axis for the given element</returns>
        private static double MidPoint(Sortable sortable)
        {
            return GetLeft(sortable) + (sortable.ActualWidth / 2);
        }

        /// <summary>
        /// Event handler for when the mouse button has been pressed on a Sortable element
        /// </summary>
        /// <param name="sender">The Sortable element which raised the event</param>
        /// <param name="e">The mouse event arguments</param>
        private void Sortable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Only handle event if the left mouse button was pressed and all the items are currently in the correct
            // order
            if (e.LeftButton == MouseButtonState.Pressed && _isSorted)
            {
                StartMoving((Sortable)sender, e);
            }
        }

        /// <summary>
        /// Adds the given Sortable instance to the ordered list and initializes the events
        /// </summary>
        /// <param name="sortable">The element to initialize</param>
        private void IntializeSortable(Sortable sortable)
        {
            _isSorted = false;
            _ordered.Add(sortable);
            sortable.MouseDown += Sortable_MouseDown;
        }

        /// <summary>
        /// Removes the given Sortable instance from the ordered list and deintializes the events
        /// </summary>
        /// <param name="sortable">The element to deinitialize</param>
        private void DeinitializeSortable(Sortable sortable)
        {
            _isSorted = false;
            _ordered.Remove(sortable);
            sortable.MouseDown -= Sortable_MouseDown;
        }

        /// <summary>
        /// Starts the process of moving the given Sortable element
        /// </summary>
        /// <param name="sortable">The element to begin the moving process on</param>
        /// <param name="e">The mouse event arguments</param>
        private void StartMoving(Sortable sortable, MouseButtonEventArgs e)
        {
            _moving.Start(this, sortable, _ordered.IndexOf(sortable), e.GetPosition(this));
            BringToFront(sortable);
            CaptureMouse();
        }

        /// <summary>
        /// Updates the user arranged order
        /// </summary>
        private void CheckOrder()
        {
            var p1 = GetLeft(_moving.Sortable);
            var p2 = p1 + _moving.Sortable.ActualWidth;

            var next = Next(_moving.Index);
            var previous = Previous(_moving.Index);

            if (next != null && p2 > MidPoint(next))
            {
                var nextLeft = GetLeft(next);
                SetLeft(next, _moving.Left);
                _ordered.SwapNext(_moving.Index);
                _moving.Left = nextLeft;
                ++_moving.Index;
            }
            else if (previous != null && p1 < MidPoint(previous))
            {
                var previousLeft = GetLeft(previous);
                SetLeft(previous, _moving.Left);
                _ordered.SwapPrevious(_moving.Index);
                _moving.Left = previousLeft;
                --_moving.Index;
            }
        }

        /// <summary>
        /// Gets the item after the given index from the ordered sortables array
        /// @pre: The ordered array is not empty and index larger than or equal to 0
        /// </summary>
        /// <param name="index">The index before the index to retrieve</param>
        /// <returns>The element after the given index, or null if the given index points to the last element in the
        /// array</returns>
        private Sortable Next(int index)
        {
            return index++ >= _ordered.Count - 1 ? null : _ordered[index];
        }

        /// <summary>
        /// Gets the item before the given index from the ordered sortables array
        /// @pre: The ordered array is not empty and index larger than or equal to 0, and is smaller than or equals to
        /// the size of the ordered array
        /// </summary>
        /// <param name="index">The index after the index to retrieve</param>
        /// <returns>The element before the given index, or null if the given index points to the first element in the
        /// array</returns>
        private Sortable Previous(int index)
        {
            return index-- <= 0 ? null : _ordered[index];
        }

        /// <summary>
        /// Updates the Left attached property of each element in the ordered list based on it's index
        /// @pre: _isSorted is false
        /// </summary>
        private void Sort()
        {
            Debug.Assert(!_isSorted, "_isSorted is not false");

            double lastLeft = 0, lastWidth = 0;
            Sortable element;
            var count = _ordered.Count;
            for (var i = 0; i < count; ++i)
            {
                element = _ordered[i];
                var currentLeft = GetLeft(element);
                if (i > 0)
                {
                    var newLeft = lastLeft + lastWidth;
                    if (currentLeft != newLeft)
                    {
                        SetLeft(element, newLeft);
                        element.Arrange(new Rect(new Point(newLeft, 0), element.DesiredSize));
                    }

                    lastLeft = newLeft;
                }
                else
                {
                    SetLeft(element, 0);
                }

                lastWidth = element.DesiredSize.Width;
            }

            _isSorted = true;
        }

        /// <summary>
        /// Displays all the elements
        /// </summary>
        private void Display()
        {
            foreach (Sortable element in Items)
            {
                element.Arrange(new Rect(new Point(GetLeft(element), 0), element.DesiredSize));
            }
        }

        /// <summary>
        /// Continues the process of moving if needed
        /// </summary>
        /// <param name="e">The mouse event arguments</param>
        private void ContinueMoving(MouseEventArgs e)
        {
            if (_moving.IsMoving)
            {
                _moving.Continue(e.GetPosition(this));
                CheckOrder();
            }
        }

        /// <summary>
        /// Finishes the process of moving if needed
        /// </summary>
        private void FinishMoving()
        {
            if (_moving.IsMoving)
            {
                _moving.Finish();
                ReleaseMouseCapture();
                _isSorted = false;
                InvalidateArrange();
            }
        }

        #endregion Private methods

        #region Private Moving class

        /// <summary>
        /// Provides an interface to move a given Sortable element
        /// </summary>
        private class Moving
        {
            #region Instance variables

            private SortablePanel _control;
            private double _maxLeft;
// ReSharper disable RedundantDefaultFieldInitializer
            private Mover _mover = new Mover();
// ReSharper restore RedundantDefaultFieldInitializer

            #endregion Instance variables

            #region Public properties

            /// <summary>
            /// Gets or sets what position of Sortable's Left attached property should be
            /// </summary>
            public double Left { get; set; }

            /// <summary>
            /// Gets a value indicating whether the instance is currently in the process of moving
            /// </summary>
            public bool IsMoving { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the Sortable instance which is currently being moved
            /// </summary>
            public Sortable Sortable { get; private set; }

            /// <summary>
            /// Gets or sets the index of Sortable
            /// </summary>
            public int Index { get; set; }

            #endregion Public properties

            #region Public methods

            /// <summary>
            /// Starts the moving process for the given element
            /// @pre: sortable is not null, index is the correct index of sortable
            /// @post: Left set to the Left attached property of sortable, IsMoving set to true, Sortable set to
            /// sortable, Index set to index
            /// </summary>
            /// <param name="control">The SortablePanel instance to move within</param>
            /// <param name="sortable">The element to move</param>
            /// <param name="index">The index position of the given Sortable element</param>
            /// <param name="point">The current position of the mouse</param>
            public void Start(SortablePanel control, Sortable sortable, int index, Point point)
            {
                _control = control;
                IsMoving = true;
                Sortable = sortable;
                Index = index;
                Left = GetLeft(sortable);
                _maxLeft = _control.ActualWidth - Sortable.DesiredSize.Width;
                _mover.Start(Left, point.X);
            }

            /// <summary>
            /// Continues the moving process for Sortable
            /// @pre: IsMoving set to true
            /// </summary>
            /// <param name="point">The new position of the mouse</param>
            public void Continue(Point point)
            {
                Debug.Assert(IsMoving, "IsMoving is not true");

                var newLeft = _mover.Continue(point.X);
                if (newLeft > _maxLeft)
                {
                    newLeft = _maxLeft;
                }

                SetLeft(Sortable, newLeft);
            }

            /// <summary>
            /// Finishes the moving process for Sortable
            /// @pre: IsMoving is true
            /// @post: IsMoving set to false, Sortable set to null
            /// </summary>
            public void Finish()
            {
                Debug.Assert(IsMoving, "IsMoving is not true");
                IsMoving = false;
                Sortable = null;
            }

            #endregion Public methods

            #region Private Mover class

            /// <summary>
            /// Calculates a new appropriate position based on differences between two points
            /// </summary>
            private struct Mover
            {
                /// <summary>
                /// The moving process will not start until the reference point has moved more than OFFSET
                /// </summary>
                private const double Offset = 4;

                private bool _started;
                private double _startingPoint;
                private double _startingMousePoint;

                /// <summary>
                /// Starts the moving process
                /// </summary>
                /// <param name="startingPoint">The current position of the point to move</param>
                /// <param name="mousePoint">The reference position to calculate the new positions with</param>
                public void Start(double startingPoint, double mousePoint)
                {
                    _startingPoint = startingPoint;
                    _startingMousePoint = mousePoint;
                    _started = false;
                }

                /// <summary>
                /// Calculates the new position based on the updated reference point
                /// @pre: Start method has been previously called
                /// </summary>
                /// <param name="point">The new updated reference point</param>
                /// <returns>The new position based on the reference point</returns>
                public double Continue(double point)
                {
                    CheckStarted(point);
                    if (_started)
                    {
                        return _startingPoint + (point - _startingMousePoint);
                    }

                    return _startingPoint;
                }

                /// <summary>
                /// Updates whether or not the moving process has started
                /// </summary>
                /// <param name="point">The new updated reference point</param>
                private void CheckStarted(double point)
                {
                    if (!_started)
                    {
                        point = Math.Abs(point - _startingMousePoint);
                        if (point > Offset)
                        {
                            _started = true;
                        }
                    }
                }
            }

            #endregion Private Mover class
        }

        #endregion Private Moving class
    }
}
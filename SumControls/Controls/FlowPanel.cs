namespace SumControls.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// A work in progress
    /// </summary>
    public class FlowPanel : CustomPanel<UIElement>
    {
        #region Constants

        private static readonly Size DefaultElementSize = new Size(500, 500);
        private const double DefaultItemGap = 100D;
        private const double DefaultFrontItemGap = 20D;

        #endregion Constants

        #region Dependency properties

        /// <summary>
        /// Identifies the ItemSize dependency property
        /// </summary>
        public static readonly DependencyProperty ItemSizeProperty =
            DependencyProperty.Register("ItemSize", typeof(Size), typeof(FlowPanel),
                new FrameworkPropertyMetadata(DefaultElementSize,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Journal),
                ValidateSize);

        /// <summary>
        /// Identifies the SelectedIndex dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(FlowPanel),
                new PropertyMetadata(0), ValidateInt);

        /// <summary>
        /// Identifies the ItemGap dependency property
        /// </summary>
        public static readonly DependencyProperty ItemGapProperty =
            DependencyProperty.Register("ItemGap", typeof(double), typeof(FlowPanel),
                new FrameworkPropertyMetadata(DefaultItemGap,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Journal),
                ValidateDouble);

        /// <summary>
        /// Identifies the FrontItemGap dependency property
        /// </summary>
        public static readonly DependencyProperty FrontItemGapProperty =
            DependencyProperty.Register("FrontItemGap", typeof(double), typeof(FlowPanel),
                new FrameworkPropertyMetadata(DefaultFrontItemGap,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Journal),
                ValidateDouble);

        #endregion Dependency properties

        /// <summary>
        /// Initializes a new instance of the FlowPanel class
        /// </summary>
        public FlowPanel()
        {
            ClipToBounds = true;
        }

        #region Public properties

        /// <summary>
        /// Gets or sets the size of each item within the FlowPanel. This is a dependency property
        /// </summary>
        [DefaultValue(typeof(Size), "500, 500")]
        public Size ItemSize
        {
            get { return (Size)GetValue(ItemSizeProperty); }
            set { SetValue(ItemSizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the currently selected index of the item within the FlowPanel. This is a dependency property
        /// </summary>
        [DefaultValue(0)]
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Gets or sets the distance between each item within the FlowPanel. This is a dependency property
        /// </summary>
        [DefaultValue(100D)]
        public double ItemGap
        {
            get { return (double)GetValue(ItemGapProperty); }
            set { SetValue(ItemGapProperty, value); }
        }

        /// <summary>
        /// Gets or sets the distance between the selected item and the remaining items. This is a dependency property
        /// </summary>
        [DefaultValue(20D)]
        public double FrontItemGap
        {
            get { return (double)GetValue(FrontItemGapProperty); }
            set { SetValue(FrontItemGapProperty, value); }
        }

        #endregion Public properties

        /// <summary>
        /// Gets the SelectedIndex value within that is within range of the number of items in the FlowPanel
        /// @pre Items.Count is larger than zero
        /// </summary>
        private int CoercedSelectedIndex
        {
            get
            {
                if (SelectedIndex >= Items.Count)
                {
                    return Items.Count - 1;
                }

                return SelectedIndex;
            }
        }

        /// <summary>
        /// Gets a visual child of this instance at the specified index position
        /// </summary>
        /// <param name="index">The index position of the child</param>
        /// <returns>The visual child of the parent element</returns>
        protected override Visual GetVisualChild(int index)
        {
            var count = Items.Count;
            var selectedIndex = SelectedIndex;
            if (selectedIndex >= count)
            {
                selectedIndex = count - 1;
            }

            return index < selectedIndex ? Items[index] : Items[(count - 1) - index + selectedIndex];
        }

        /// <summary>
        /// Measures the size in layout required for child elements and determines a size for the instance
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child
        /// element sizes</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement element in Items)
            {
                element.Measure(ItemSize);
            }

            var desiredWidth = ItemSize.Height;
            var desiredHeight = ItemSize.Width;

            if (availableSize.Width.IsValid() && desiredWidth > availableSize.Width)
            {
                desiredWidth = availableSize.Width;
            }

            if (availableSize.Height.IsValid() && desiredHeight > availableSize.Height)
            {
                desiredHeight = availableSize.Height;
            }

            return new Size(desiredWidth, desiredHeight);
        }

        /// <summary>
        /// Positions child elements and determines a size for the instance
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and
        /// its children</param>
        /// <returns>The actual size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var count = Items.Count;
            if (count > 0)
            {
                var selectedIndex = CoercedSelectedIndex;

                var x = GetCenterWidth(finalSize);
                Items[selectedIndex].Arrange(new Rect(new Point(x, 0D), ItemSize));

                var leftSideX = x - ItemSize.Width - FrontItemGap;
                var rightSideX = x + ItemSize.Width + FrontItemGap;

                int i;

                for (i = selectedIndex - 1; i >= 0; --i)
                {
                    Items[i].Arrange(new Rect(new Point(leftSideX, 0D), ItemSize));
                    leftSideX -= ItemGap;
                }

                for (i = selectedIndex + 1; i < count; ++i)
                {
                    Items[i].Arrange(new Rect(new Point(rightSideX, 0D), ItemSize));
                    rightSideX += ItemGap;
                }
            }

            return finalSize;
        }

        /// <summary>
        /// Returns whether the given Size contains valid double values above zero
        /// </summary>
        /// <param name="value">The Size value to test</param>
        /// <returns>true if the given Size is valid</returns>
        private static bool ValidateSize(object value)
        {
            var s = (Size)value;
            return s.Width.IsValid() && s.Width > 0D && s.Height.IsValid() && s.Height > 0D;
        }

        /// <summary>
        /// Returns whether the given int is larger than zero
        /// </summary>
        /// <param name="value">The int value to test</param>
        /// <returns>true if the given int is valid</returns>
        private static bool ValidateInt(object value)
        {
            return ((int)value) >= 0;
        }

        /// <summary>
        /// Returns whether the double is a valid value (not NaN or Infinity)
        /// </summary>
        /// <param name="value">The double to test</param>
        /// <returns>true if the double is valid</returns>
        private static bool ValidateDouble(object value)
        {
            return ((double)value).IsValid();
        }

        /// <summary>
        /// Returns the center width for an element
        /// </summary>
        /// <param name="finalSize">The final rendering size of this element</param>
        /// <returns>The center point of where the element should be positioned</returns>
        private double GetCenterWidth(Size finalSize)
        {
            return (finalSize.Width - ItemSize.Width) / 2D;
        }
    }
}
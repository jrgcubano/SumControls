namespace SumControls.Controls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using Gestures;

    /// <summary>
    /// Scales another element
    /// </summary>
    public class Zoombox : Border
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the MinZoom dependency property
        /// </summary>
        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register("MinZoom", typeof(double), typeof(Zoombox),
                new FrameworkPropertyMetadata(DefaultMinZoom, FrameworkPropertyMetadataOptions.Journal,
                    ZoomProperties_Changed, CoerceMinZoom), ValidateDouble);

        /// <summary>
        /// Identifies the MaxZoom dependency property
        /// </summary>
        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register("MaxZoom", typeof(double), typeof(Zoombox),
                new FrameworkPropertyMetadata(DefaultMaxZoom, FrameworkPropertyMetadataOptions.Journal,
                    ZoomProperties_Changed, CoerceMaxZoom), ValidateDouble);

        /// <summary>
        /// Identifies the Zoom dependency property
        /// </summary>
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(Zoombox),
                new FrameworkPropertyMetadata(DefaultZoom,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure |
                        FrameworkPropertyMetadataOptions.Journal, Zoom_Changed, CoerceZoom), ValidateDouble);

        /// <summary>
        /// Identifies the ZoomIncrement dependency property
        /// </summary>
        public static readonly DependencyProperty ZoomIncrementProperty =
            DependencyProperty.Register("ZoomIncrement", typeof(double), typeof(Zoombox),
                new PropertyMetadata(DefaultZoomIncrement), ValidateDouble);

        private static readonly DependencyPropertyKey CanZoomInPropertyKey =
            DependencyProperty.RegisterReadOnly("CanZoomIn", typeof(bool), typeof(Zoombox),
                new PropertyMetadata(true));

        /// <summary>
        /// Identifies the readonly CanZoomIn dependency property
        /// </summary>
        public static readonly DependencyProperty CanZoomInProperty = CanZoomInPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CanZoomOutPropertyKey =
            DependencyProperty.RegisterReadOnly("CanZoomOut", typeof(bool), typeof(Zoombox),
                new PropertyMetadata(true));

        /// <summary>
        /// Identifies the readonly CanZoomOut dependency property
        /// </summary>
        public static readonly DependencyProperty CanZoomOutProperty = CanZoomOutPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the readonly AnimateZoom dependency property
        /// </summary>
        public static readonly DependencyProperty AnimateZoomProperty =
            DependencyProperty.Register("AnimateZoom", typeof(bool), typeof(Zoombox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the readonly AnimationDuration dependency property
        /// </summary>
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(TimeSpan), typeof(Zoombox),
                new PropertyMetadata(TimeSpan.FromMilliseconds(200D)));

        #endregion Dependency properties

        #region Constants

        private const double DefaultMinZoom = 10D;
        private const double DefaultMaxZoom = 1000D;
        private const double DefaultZoom = 100D;
        private const double DefaultZoomIncrement = 10D;

        #endregion Constants

        #region Instance variables

        private readonly ScaleTransform _scaleTransform = new ScaleTransform();
        private readonly TransformGroup _transformGroup = new TransformGroup();
        private bool _initialized;
        private double _zoomFactor;

        #endregion Instance variables

        #region Constructors

        /// <summary>
        /// Initializes static members of the Zoombox class
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "These must be initialized in the static constructor")]
        static Zoombox()
        {
            NavigationCommands.IncreaseZoom.InputGestures.Add(
                new KeyGesture(Key.Add, ModifierKeys.Control, "Ctrl+ +"));
            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.OemPlus, ModifierKeys.Control));
            NavigationCommands.IncreaseZoom.InputGestures.Add(
                new MouseWheelGesture(MouseWheelAction.WheelUp, ModifierKeys.Control));

            NavigationCommands.DecreaseZoom.InputGestures.Add(
                new KeyGesture(Key.Subtract, ModifierKeys.Control, "Ctrl+-"));
            NavigationCommands.DecreaseZoom.InputGestures.Add(
                new KeyGesture(Key.OemMinus, ModifierKeys.Control));
            NavigationCommands.DecreaseZoom.InputGestures.Add(
                new MouseWheelGesture(MouseWheelAction.WheelDown, ModifierKeys.Control));

            CommandManager.RegisterClassCommandBinding(typeof(Zoombox),
                new CommandBinding(NavigationCommands.IncreaseZoom, IncreaseZoom_Executed, IncreaseZoom_CanExecute));

            CommandManager.RegisterClassCommandBinding(typeof(Zoombox),
                new CommandBinding(NavigationCommands.DecreaseZoom, DecreaseZoom_Executed, DecreaseZoom_CanExecute));
        }

        /// <summary>
        /// Initializes a new instance of the Zoombox class
        /// </summary>
        public Zoombox()
        {
            Initialized += Zoombox_Initialized;

            _transformGroup.Children.Add(_scaleTransform);
            LayoutTransform = _transformGroup;

            Focusable = true;

            ApplyZoom();
        }

        #endregion Constructors

        #region Public properties

        /// <summary>
        /// Gets or sets the minimum Zoom value. This is a dependency property
        /// </summary>
        [DefaultValue(10D)]
        [Category("Zoom")]
        public double MinZoom
        {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum Zoom value. This is a dependency property
        /// </summary>
        [DefaultValue(1000D)]
        [Category("Zoom")]
        public double MaxZoom
        {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }

        /// <summary>
        /// Gets or sets the scale of the control as a percentage. This is a dependency property
        /// </summary>
        [DefaultValue(100D)]
        [Category("Zoom")]
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        /// <summary>
        /// Gets or sets the amount to zoom in or out with each command call. This is a dependency property
        /// </summary>
        [DefaultValue(10D)]
        [Category("Zoom")]
        public double ZoomIncrement
        {
            get { return (double)GetValue(ZoomIncrementProperty); }
            set { SetValue(ZoomIncrementProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the Zoombox is able to zoom in further. This is a dependency property
        /// </summary>
        public bool CanZoomIn
        {
            get { return (bool)GetValue(CanZoomInProperty); }
            private set { SetValue(CanZoomInPropertyKey, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the Zoombox is able to zoom out further. This is a dependency property
        /// </summary>
        public bool CanZoomOut
        {
            get { return (bool)GetValue(CanZoomOutProperty); }
            private set { SetValue(CanZoomOutPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to animate the zoom behaviour. This is a dependency property
        /// </summary>
        [DefaultValue(true)]
        [Category("Zoom")]
        public bool AnimateZoom
        {
            get { return (bool)GetValue(AnimateZoomProperty); }
            set { SetValue(AnimateZoomProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating the total duration for the zoom animation to run for when AnimateZoom is
        /// true. Default value is 300 milliseconds. This is a dependency property
        /// </summary>
        [Category("Zoom")]
        public TimeSpan AnimationDuration
        {
            get { return (TimeSpan)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Scales the ZoomBox up by one increment if possible
        /// </summary>
        public void ZoomIn()
        {
            var zoomLevel = Zoom + ZoomIncrement;
            if (zoomLevel > MaxZoom)
            {
                zoomLevel = MaxZoom;
            }

            Zoom = zoomLevel;
        }

        /// <summary>
        /// Scales down the ZoomBox by one increment if possible
        /// </summary>
        public void ZoomOut()
        {
            var zoomLevel = Zoom - ZoomIncrement;
            if (zoomLevel < MinZoom)
            {
                zoomLevel = MinZoom;
            }

            Zoom = zoomLevel;
        }

        #endregion Public methods

        #region Dependency properties related data

        /// <summary>
        /// Ensures that the Zoom property is between MinZoom and MaxZoom
        /// @pre ValidateDouble(value) returned true
        /// </summary>
        /// <param name="dp">The Zoombox whose property was changed</param>
        /// <param name="value">The new double Zoom value</param>
        /// <returns>The coerced double Zoom value</returns>
        private static object CoerceZoom(DependencyObject dp, object value)
        {
            var zoom = (double)value;
            var box = (Zoombox)dp;

            // Ensure that the new zoom value is between the MinZoom and MaxZoom properties
            if (zoom > box.MaxZoom)
            {
                zoom = box.MaxZoom;
            }
            else if (zoom < box.MinZoom)
            {
                zoom = box.MinZoom;
            }

            return zoom;
        }

        /// <summary>
        /// Ensures that the Zoom property is at least set to MinZoom and that MinZoom does not exceed MazZoom
        /// </summary>
        /// <param name="dp">The Zoombox whose property was changed</param>
        /// <param name="value">The new double MinZoom value</param>
        /// <returns>The coerced double MinZoom value</returns>
        private static object CoerceMinZoom(DependencyObject dp, object value)
        {
            var zoom = (double)value;
            var box = (Zoombox)dp;

            // Change the MazZoom if the new MinZoom is larger
            if (zoom > box.MaxZoom)
            {
                box.MaxZoom = zoom;
            }

            // Change the current zoom if the new MinZoom is smaller
            if (box.Zoom < zoom)
            {
                box.Zoom = zoom;
            }

            return zoom;
        }

        /// <summary>
        /// Ensures that the Zoom property is no larger than MaxZoom and that MaxZoom is not smaller than MinZoom
        /// </summary>
        /// <param name="dp">The Zoombox whose property was changed</param>
        /// <param name="value">The new double MaxZoom value</param>
        /// <returns>The coerced double MaxZoom value</returns>
        private static object CoerceMaxZoom(DependencyObject dp, object value)
        {
            var zoom = (double)value;
            var box = (Zoombox)dp;

            // Change the MinZoom if the new MaxZoom is smaller
            if (zoom < box.MinZoom)
            {
                box.MinZoom = zoom;
            }

            // Change the current zoom if the new MaxZoom is larger
            if (box.Zoom > zoom)
            {
                box.Zoom = zoom;
            }

            return zoom;
        }

        /// <summary>
        /// Determines whether the given double value is valid (not NaN and not Infinity) and is larger than 0
        /// </summary>
        /// <param name="value">The double value to validate</param>
        /// <returns>true if the given double is valid</returns>
        private static bool ValidateDouble(object value)
        {
            var d = (double)value;
            return d.IsValid() && d > 0D;
        }

        /// <summary>
        /// Updates the CanZoom properties and applies the new zoom when the Zoom property is changed
        /// </summary>
        /// <param name="dp">The Zoombox</param>
        /// <param name="e">The parameter is not used.</param>
        private static void Zoom_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var box = (Zoombox)dp;
            box.UpdateCanZoom();
            box.ApplyZoom();
        }

        /// <summary>
        /// Updates the CanZoom properties when any Zoom related properties are changed
        /// </summary>
        /// <param name="dp">The Zoombox</param>
        /// <param name="e">The parameter is not used.</param>
        private static void ZoomProperties_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((Zoombox)dp).UpdateCanZoom();
        }

        #endregion Dependency properties related data

        #region Private methods

        /// <summary>
        /// Applies the new zoom
        /// </summary>
        private void ApplyZoom()
        {
            _zoomFactor = Zoom / 100;

            if (AnimateZoom && _initialized)
            {
                var xScale = MakeAnimation(_zoomFactor);
                var yScale = MakeAnimation(_zoomFactor);

                var storyboard = new Storyboard();
                storyboard.Children.Add(xScale);
                storyboard.Children.Add(yScale);

                Storyboard.SetTargetProperty(xScale, new PropertyPath("LayoutTransform.Children[0].ScaleX"));
                Storyboard.SetTargetProperty(yScale, new PropertyPath("LayoutTransform.Children[0].ScaleY"));

                storyboard.Begin(this);
            }
            else
            {
                // Cancel any currently running animations
                _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);

                _scaleTransform.ScaleX = _zoomFactor;
                _scaleTransform.ScaleY = _zoomFactor;
            }
        }

        /// <summary>
        /// Creates a new DoubleAnimation instance for animating the zoom
        /// </summary>
        /// <param name="toValue">The new Zoom value to animate to</param>
        /// <returns>The new DoubleAnimation instance</returns>
        private DoubleAnimation MakeAnimation(double toValue)
        {
            return new DoubleAnimation(toValue, new Duration(AnimationDuration))
            {
                FillBehavior = FillBehavior.HoldEnd
            };
        }

        /// <summary>
        /// Updates the CanZoomIn and CanZoomOut properties to their correct values if required
        /// </summary>
        private void UpdateCanZoom()
        {
            var canZoomIn = Zoom != MaxZoom;
            var canZoomOut = Zoom != MinZoom;

            if (canZoomIn != CanZoomIn)
            {
                CanZoomIn = canZoomIn;
            }

            if (canZoomOut != CanZoomOut)
            {
                CanZoomOut = canZoomOut;
            }
        }

        /// <summary>
        /// Updates the initialized status
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Zoombox_Initialized(object sender, EventArgs e)
        {
            _initialized = true;
        }

        #endregion Private methods

        #region Public static functions

        /// <summary>
        /// Changes the modifier key needed to be pressed to allow zooming by the mouse wheel
        /// </summary>
        /// <param name="key">The new modifier key to use instead</param>
        public static void ChangeModifierKeyForZooming(ModifierKeys key)
        {
            var increaseZoomGestures = NavigationCommands.IncreaseZoom.InputGestures;
            var decreaseZoomGestures = NavigationCommands.DecreaseZoom.InputGestures;

            foreach (InputGesture gesture in increaseZoomGestures)
            {
                var mouseWheelGesture = gesture as MouseWheelGesture;
                if (mouseWheelGesture != null && mouseWheelGesture.MouseWheelAction == MouseWheelAction.WheelUp)
                {
                    mouseWheelGesture.Modifiers = key;
                    break;
                }
            }

            foreach (InputGesture gesture in decreaseZoomGestures)
            {
                var mouseWheelGesture = gesture as MouseWheelGesture;
                if (mouseWheelGesture != null && mouseWheelGesture.MouseWheelAction == MouseWheelAction.WheelUp)
                {
                    mouseWheelGesture.Modifiers = key;
                    break;
                }
            }
        }

        #endregion Public static functions

        #region Commands related data

        /// <summary>
        /// Determines whether the IncreaseZoom command can be executed
        /// </summary>
        /// <param name="sender">The Zoombox instance</param>
        /// <param name="e">The event arguments</param>
        private static void IncreaseZoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var box = sender as Zoombox;
            if (box != null)
            {
                e.CanExecute = box.CanZoomIn;
            }
        }

        /// <summary>
        /// Zooms in the Zoombox
        /// </summary>
        /// <param name="sender">The Zoombox instance</param>
        /// <param name="e">The event arguments</param>
        private static void IncreaseZoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var box = sender as Zoombox;
            if (box != null)
            {
                box.ZoomIn();
            }
        }

        /// <summary>
        /// Determines whether the DecreaseZoom command can be executed
        /// </summary>
        /// <param name="sender">The Zoombox instance</param>
        /// <param name="e">The event arguments</param>
        private static void DecreaseZoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var box = sender as Zoombox;
            if (box != null)
            {
                e.CanExecute = box.CanZoomOut;
            }
        }

        /// <summary>
        /// Zooms out the Zoombox
        /// </summary>
        /// <param name="sender">The Zoombox instance</param>
        /// <param name="e">The event arguments</param>
        private static void DecreaseZoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var box = sender as Zoombox;
            if (box != null)
            {
                box.ZoomOut();
            }
        }

        #endregion Commands related data
    }
}
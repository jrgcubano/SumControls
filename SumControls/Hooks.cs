namespace SumControls
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Event handler for the horizontal mouse wheel tilt notification
    /// </summary>
    /// <param name="delta">The delta which represents the distance tilted</param>
    public delegate void HorizontalTiltEventHandler(int delta);

    /// <summary>
    /// Provides an interface to attach Win32 events to an element
    /// </summary>
    public static class Hooks
    {
        /// <summary>
        /// Identifies the WndProcHookHandler attached property
        /// </summary>
        public static readonly DependencyProperty WndProcHookHandlerProperty =
            DependencyProperty.RegisterAttached("WndProcHookHandler", typeof(HwndSourceHook), typeof(Hooks),
                new PropertyMetadata(null, WndProcHookHandler_Changed));

        /// <summary>
        /// Identifies the HorizontalTiltHandler attached property
        /// </summary>
        public static readonly DependencyProperty HorizontalTiltHandlerProperty =
            DependencyProperty.RegisterAttached("HorizontalTiltHandler", typeof(HorizontalTiltEventHandler),
                typeof(Hooks), new PropertyMetadata(null, HorizontalTiltHandler_Changed));

        // ReSharper disable InconsistentNaming
        private const int WM_MOUSEHWHEEL = 0x020E;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Sets the WndProc event handler for the given FrameworkElement
        /// </summary>
        /// <param name="element">The FrameworkElement to attach the event to</param>
        /// <param name="handler">The WndProc event handler</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Only objects that derive from FrameworkElement can use a hook")]
        public static void SetWindProcHookHandler(FrameworkElement element, HwndSourceHook handler)
        {
            element.SetValue(WndProcHookHandlerProperty, handler);
        }

        /// <summary>
        /// Sets the event handler for the given FrameworkElement instance's horizontal mouse wheel tilt notification
        /// </summary>
        /// <param name="element">The FrameworkElement to attach the event to</param>
        /// <param name="handler">The horizontal mouse wheel tilt event handler</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Only objects that derive from FrameworkElement can use a hook")]
        public static void SetHorizontalTiltHandler(FrameworkElement element, HorizontalTiltEventHandler handler)
        {
            element.SetValue(HorizontalTiltHandlerProperty, handler);
        }

        #region Dependency properties related data

        /// <summary>
        /// Hooks the the new event handler to the WndProc function of the element
        /// </summary>
        /// <param name="dp">The FrameworkElement element to attach the event to</param>
        /// <param name="e">The arguments that describe this event</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId = "SumControls.Hooks+WndProcHooker", Justification = "The object must be instantiated")]
        private static void WndProcHookHandler_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var element = dp as FrameworkElement;
            var handler = e.NewValue as HwndSourceHook;
            if (element != null && handler != null)
            {
                new WndProcHooker(element, handler);
            }
        }

        /// <summary>
        /// Hooks the new event handler to the WM_MOUSEHWHEEL notification of the element
        /// </summary>
        /// <param name="dp">The FrameworkElement element to attach the event to</param>
        /// <param name="e">The arguments that describe this event</param>
        private static void HorizontalTiltHandler_Changed(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var element = dp as FrameworkElement;
            var handler = e.NewValue as HorizontalTiltEventHandler;
            if (element != null && handler != null)
            {
                SetWindProcHookHandler(element,
                    delegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
                    {
                        if (msg == WM_MOUSEHWHEEL)
                        {
                            handler((short)Functions.HiWord(wParam));
                        }

                        return IntPtr.Zero;
                    });
            }
        }

        #endregion Dependency properties related data

        /// <summary>
        /// Hooks a given FrameworkElement to a WndProc event handler
        /// </summary>
        private class WndProcHooker
        {
            private readonly FrameworkElement _element;
            private readonly HwndSourceHook _handler;

            /// <summary>
            /// Initializes a new instance of the WndProcHooker class
            /// </summary>
            /// <param name="element">The FrameworkElement to hook the event to</param>
            /// <param name="handler">The event handler to be called in place of the WndProc function</param>
            public WndProcHooker(FrameworkElement element, HwndSourceHook handler)
            {
                Debug.Assert(element != null, "element is null");
                Debug.Assert(handler != null, "handler is null");

                _element = element;
                _handler = handler;

                AddWndProcHook();
            }

            /// <summary>
            /// Adds the WndProc hook to the item
            /// </summary>
            private void AddWndProcHook()
            {
                var source = PresentationSource.FromDependencyObject(_element) as HwndSource;
                if (source != null)
                {
                    source.AddHook(_handler);
                }
                else
                {
                    _element.Loaded += Element_Loaded;
                }
            }

            /// <summary>
            /// Attempts to add the WndProc hook to the item when it is completely initialized in case the
            /// first attempt was a failure
            /// </summary>
            /// <param name="sender">The parameter is not used.</param>
            /// <param name="e">The parameter is not used.</param>
            private void Element_Loaded(object sender, RoutedEventArgs e)
            {
                AddWndProcHook();
            }
        }
    }
}
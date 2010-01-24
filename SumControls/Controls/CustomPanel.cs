namespace SumControls.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;

    using ElementCollection;

    /// <summary>
    /// A Panel which holds a custom collection of controls derived from UIElement
    /// </summary>
    /// <typeparam name="T">The type of objects to hold. Must derive from UIElement</typeparam>
    [ContentProperty("Items")]
    public abstract class CustomPanel<T> : Panel where T : UIElement
    {
        private bool _handleEvents = true;

        /// <summary>
        /// Initialzies a new instance of the CustomPanel&lt;T&gt; class
        /// </summary>
        protected CustomPanel()
        {
            Items = new ElementCollection<T>(this, this);

            Items.ElementAdded += Items_ElementAdded;
            Items.ElementRemoved += Items_ElementRemoved;
        }

        /// <summary>
        /// Gets the collection of child elements for this control
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ElementCollection<T> Items { get; private set; }

        /// <summary>
        /// Gets the number of child visual objects in this instance
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return Items.Count; }
        }

        #region Protected methods

        /// <summary>
        /// Moves the given element to the front of the list. Calling this method will raise the Items.ElementAdded and
        /// Items.ElementRemoved events, but will not call the OnItemAdded and OnItemRemoved methods
        /// </summary>
        /// <param name="item">The element to move to the front</param>
        protected void BringToFront(T item)
        {
            if (ReferenceEquals(Items[Items.Count - 1], item))
            {
                return;
            }

            lock (this)
            {
                _handleEvents = false;
                Items.Remove(item);
                Items.Add(item);
                _handleEvents = true;
            }
        }

        /// <summary>
        /// Gets a visual child of this instance at the specified index position
        /// </summary>
        /// <param name="index">The index position of the child</param>
        /// <returns>The visual child of the parent element</returns>
        protected override Visual GetVisualChild(int index)
        {
            return Items[index];
        }

        /// <summary>
        /// Called when an item is added to the panel. Override to handle this event
        /// </summary>
        /// <param name="item">The new item which was added</param>
        protected virtual void OnItemAdded(T item) {}

        /// <summary>
        /// Called when an iten is removed from the panel. Override to handle this event
        /// </summary>
        /// <param name="item">The item which was removed</param>
        protected virtual void OnItemRemoved(T item) {}

        #endregion Protected methods

        #region Private methods

        /// <summary>
        /// Event handler for when an item is added
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The arguments that describe this event</param>
        private void Items_ElementAdded(object sender, ElementCollectionEventArgs<T> e)
        {
            if (_handleEvents)
            {
                OnItemAdded(e.Item);
            }
        }

        /// <summary>
        /// Event handler for when an item is removed
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The arguments that describe this event</param>
        private void Items_ElementRemoved(object sender, ElementCollectionEventArgs<T> e)
        {
            if (_handleEvents)
            {
                OnItemRemoved(e.Item);
            }
        }

        #endregion Private methods
    }
}
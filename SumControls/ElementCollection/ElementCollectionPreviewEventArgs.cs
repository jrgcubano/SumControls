namespace SumControls.ElementCollection
{
    /// <summary>
    /// Event arguments for the ElementCollection&lt;T&gt;.PreviewElementAdded and
    /// ElementCollection&lt;T&gt;.PreviewElementRemoved events
    /// </summary>
    /// <typeparam name="T">The type of object to hold</typeparam>
    public class ElementCollectionPreviewEventArgs<T> : ElementCollectionEventArgs<T>
    {
        /// <summary>
        /// Initializes a new instance of the ElementCollectionPreviewEventArgs&lt;T&gt; class
        /// </summary>
        /// <param name="item">The item to hold</param>
        public ElementCollectionPreviewEventArgs(T item)
            : base(item)
        {
            Cancel = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the action should be canceled
        /// </summary>
        public bool Cancel { get; set; }
    }
}
namespace SumControls.ElementCollection
{
    using System;

    /// <summary>
    /// Event arguments for the ElementCollection&lt;T&gt;.ElementAdded and ElementCollection&lt;T&gt;.ElementRemoved
    /// events
    /// </summary>
    /// <typeparam name="T">The type of object to hold</typeparam>
    public class ElementCollectionEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ElementCollectionEventArgs&lt;T&gt; class
        /// </summary>
        /// <param name="item">The item to hold</param>
        public ElementCollectionEventArgs(T item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was added or removed
        /// </summary>
        public T Item { get; private set; }
    }
}
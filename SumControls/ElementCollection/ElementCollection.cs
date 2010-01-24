namespace SumControls.ElementCollection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Holds a generic set of objects that derive from UIElement
    /// </summary>
    /// <typeparam name="T">The type of objects to hold. Must derive from UIElement</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface",
        Justification = "Type must derive from UIElement")]
    public class ElementCollection<T> : IList where T : UIElement
    {
        #region Instance variables

        private readonly UIElementCollection _collection;
        private ElementCollectionPreviewEventArgs<T> _previewAdded;
        private ElementCollectionPreviewEventArgs<T> _previewRemoved;

        #endregion Instance variables

        /// <summary>
        /// Initializes a new instance of the ElementCollection&lt;T&gt; class
        /// </summary>
        /// <param name="visualParent">The System.Windows.UIElement parent of the collection</param>
        /// <param name="logicalParent">The logical parent of the elements in the collection</param>
        public ElementCollection(UIElement visualParent, FrameworkElement logicalParent)
        {
            _collection = new UIElementCollection(visualParent, logicalParent);
        }

        #region Events

        /// <summary>
        /// Occurs when an element is about to be added to the collection. Provides the ability to stop the object from
        /// being added
        /// </summary>
        public event EventHandler<ElementCollectionPreviewEventArgs<T>> PreviewElementAdded;

        /// <summary>
        /// Occurs when an element is about to be removed from the collection. Provides the ability to stop the object
        /// form being removed
        /// </summary>
        public event EventHandler<ElementCollectionPreviewEventArgs<T>> PreviewElementRemoved;

        /// <summary>
        /// Occurs after an element has been added to the collection
        /// </summary>
        public event EventHandler<ElementCollectionEventArgs<T>> ElementAdded;

        /// <summary>
        /// Occurs after an element has been removed from the collection
        /// </summary>
        public event EventHandler<ElementCollectionEventArgs<T>> ElementRemoved;

        #endregion Events

        #region Public properties

        /// <summary>
        /// Gets or sets the number of elements that the collection can contain
        /// </summary>
        public virtual int Capacity
        {
            get { return _collection.Capacity; }
            set { _collection.Capacity = value; }
        }

        /// <summary>
        /// Gets the actual number of elements in the collection
        /// </summary>
        public virtual int Count
        {
            get { return _collection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread-safe)
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return _collection.IsSynchronized; }
        }

        /// <summary>
        /// Gets an object that you can use to synchronize access to the collection
        /// </summary>
        public virtual object SyncRoot
        {
            get { return _collection.SyncRoot; }
        }

        #endregion Public properties

        #region IList Members

        int IList.Add(object value)
        {
            return Add((T)value);
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "Used only to implement IList interface")]
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "Used only to implement IList interface")]
        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the System.Windows.UIElement stored at the zero-based index position of the collection
        /// </summary>
        /// <param name="index">The index position of the element</param>
        /// <returns>The element at the specified index position</returns>
        public virtual T this[int index]
        {
            get { return (T)_collection[index]; }
            set { _collection[index] = value; }
        }

        #region Public methods

        /// <summary>
        /// Adds the specified element to the collection
        /// </summary>
        /// <param name="element">The element to add</param>
        /// <returns>The index position of the added element</returns>
        public virtual int Add(T element)
        {
            var result = -1;

            OnPreviewElementAdded(element);
            if (_previewAdded == null || !_previewAdded.Cancel)
            {
                result = _collection.Add(element);
                OnElementAdded(element);
            }

            return result;
        }

        /// <summary>
        /// Removes all elements from the collection. Does not raise the PreviewElementRemoved event
        /// </summary>
        public virtual void Clear()
        {
            if (_collection.Count == 0)
            {
                return;
            }

            var elements = new List<T>(_collection.Count);
            elements.AddRange(_collection.Cast<T>());

            _collection.Clear();

            foreach (var element in elements)
            {
                OnElementRemoved(element);
            }
        }

        /// <summary>
        /// Determines whether a specified element is in the collection
        /// </summary>
        /// <param name="element">The element to find</param>
        /// <returns>true if the given element exists within the collection</returns>
        public virtual bool Contains(T element)
        {
            return _collection.Contains(element);
        }

        /// <summary>
        /// Copies the elements in the collection to an array, starting at a specified index position
        /// </summary>
        /// <param name="array">The array to copy the element into</param>
        /// <param name="index">The index position of the element where copying begins</param>
        public virtual void CopyTo(Array array, int index)
        {
            _collection.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the elements in the collection to an array, starting at a specified index position
        /// </summary>
        /// <param name="array">The array to copy the element into</param>
        /// <param name="index">The index position of the element where copying begins</param>
        public virtual void CopyTo(T[] array, int index)
        {
            _collection.CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that can iterate the collection
        /// </summary>
        /// <returns>An IEnumerator that can list the members of this collection</returns>
        public virtual IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        /// <summary>
        /// Returns the index position of a specified element in the collection
        /// </summary>
        /// <param name="element">The element whose index position is required</param>
        /// <returns>The index position of the element</returns>
        public virtual int IndexOf(T element)
        {
            return _collection.IndexOf(element);
        }

        /// <summary>
        /// Inserts an element into the collection at the specified index position
        /// </summary>
        /// <param name="index">The index position of where to insert the element</param>
        /// <param name="element">The element to insert into the collection</param>
        public virtual void Insert(int index, T element)
        {
            OnPreviewElementAdded(element);
            if (_previewAdded == null || !_previewAdded.Cancel)
            {
                _collection.Insert(index, element);
                OnElementAdded(element);
            }
        }

        /// <summary>
        /// Removes the specified element from the collection
        /// </summary>
        /// <param name="element">The element to remove</param>
        public virtual void Remove(T element)
        {
            OnPreviewElementRemoved(element);
            if (_previewRemoved == null || !_previewRemoved.Cancel)
            {
                _collection.Remove(element);
                OnElementRemoved(element);
            }
        }

        /// <summary>
        /// Removes the element at the specified index
        /// </summary>
        /// <param name="index">The index of the element to remove</param>
        public virtual void RemoveAt(int index)
        {
            var element = this[index];
            OnPreviewElementRemoved(element);
            if (_previewRemoved == null || !_previewRemoved.Cancel)
            {
                _collection.RemoveAt(index);
                OnElementRemoved(element);
            }
        }

        /// <summary>
        /// Removes a range of elements from the collection. Does not raise the PreviewElementRemoved event
        /// </summary>
        /// <param name="index">The index position of the element where removal begins</param>
        /// <param name="count">The number of elements to remove</param>
        public virtual void RemoveRange(int index, int count)
        {
            var elements = new List<T>(count);
            for (var i = index; i < count; ++i)
            {
                elements.Add(this[i]);
            }

            _collection.RemoveRange(index, count);

            foreach (var element in elements)
            {
                OnElementRemoved(element);
            }
        }

        #endregion Public methods

        #region Event raisers

        /// <summary>
        /// Raises the PreviewElementAdded event
        /// </summary>
        /// <param name="element">The element to be added</param>
        protected virtual void OnPreviewElementAdded(T element)
        {
            if (PreviewElementAdded != null)
            {
                _previewAdded = new ElementCollectionPreviewEventArgs<T>(element);
                PreviewElementAdded(this, _previewAdded);
            }
        }

        /// <summary>
        /// Raises the PreviewElementRemoved event
        /// </summary>
        /// <param name="element">The element to be removed</param>
        protected virtual void OnPreviewElementRemoved(T element)
        {
            if (PreviewElementRemoved != null)
            {
                _previewRemoved = new ElementCollectionPreviewEventArgs<T>(element);
                PreviewElementRemoved(this, _previewRemoved);
            }
        }

        /// <summary>
        /// Raises the ElementAdded event
        /// </summary>
        /// <param name="element">The element which was added</param>
        protected virtual void OnElementAdded(T element)
        {
            if (ElementAdded != null)
            {
                ElementAdded(this, new ElementCollectionEventArgs<T>(element));
            }
        }

        /// <summary>
        /// Raises the ElementRemoved event
        /// </summary>
        /// <param name="element">The element which was removed</param>
        protected virtual void OnElementRemoved(T element)
        {
            if (ElementRemoved != null)
            {
                ElementRemoved(this, new ElementCollectionEventArgs<T>(element));
            }
        }

        #endregion Event raisers
    }
}
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace KIPC.Util
{
    /// <summary>
    /// Handles an Indexing Enumerator, which wraps a regular enumerator as if it was a DictionaryIterator using an autoincrementing count as the index.
    /// </summary>
    public struct CountingIterator : IDictionaryEnumerator
    {
        /// <summary>
        /// Stores the actual enumerator we have wrapped.
        /// </summary>
        public IEnumerator wrappedEnumerator;

        /// <summary>
        /// The start value for this CountingIterator.
        /// </summary>
        public int start;

        /// <summary>
        /// Whether we've started the count yet.  MoveNext() checks this every call; if false it sets it to true and resets index to start.
        /// </summary>
        private bool started;

        /// <summary>
        /// Current position.
        /// </summary>
        private int index;

        /// <summary>
        /// Creates a CountingIterator that wraps the specified enumerator with the specified start value.
        /// </summary>
        /// <param name="wrappedEnumerator">Enumerator to wrap</param>
        /// <param name="start">Start index</param>
        public CountingIterator(IEnumerator wrappedEnumerator, int start = 0)
        {
            this.wrappedEnumerator = wrappedEnumerator;
            this.start = 0;
            this.started = false;
            this.index = -1;
        }

        /// <summary>
        /// Returns the current entry.
        /// </summary>
        public object Current { get { return Entry; } }

        /// <summary>
        /// Returns the current entry.
        /// </summary>
        public DictionaryEntry Entry
        {
            get {
                var value = Value;  // Rely on this triggering an exception if one needs to be triggered.
                return new DictionaryEntry(index, value);
            }
        }

        /// <summary>
        /// Returns the current key (index value).
        /// </summary>
        public object Key
        {
            get
            {
                var value = Value;  // Rely on this triggering an exception if one needs to be triggered.
                return index;
            }
        }

        /// <summary>
        /// Returns the wrapped enumerator's current value
        /// </summary>
        public object Value {
            get
            {
                return wrappedEnumerator.Current;
            }
        }

        /// <summary>
        /// Advances to the next entry.
        /// </summary>
        public bool MoveNext()
        {
            bool result = wrappedEnumerator.MoveNext();
            if (!started)
            {
                started = true;
                index = start;
            } else
            {
                ++index;
            }
            return result;
        }

        /// <summary>
        /// Reset the iterator.
        /// </summary>
        public void Reset()
        {
            wrappedEnumerator.Reset();
            started = false;
        }

        /// <summary>
        /// Wraps the specified IEnumerable's enumerator.
        /// </summary>
        /// <param name="enumerable">Enumerable to wrap.</param>
        /// <returns>IEnumerable.GetEnumerator() if it is a IDictionaryEnumerator, otherwise the wrapped iterator.</returns>
        public static IEnumerator GetKeyedEnumerator(IEnumerable enumerable) {
            var e = enumerable.GetEnumerator();
            if (e is IDictionaryEnumerator) return e;
            return new CountingIterator(e);
        }

        private struct CountingProxy : IEnumerable
        {
            private IEnumerable e;
            internal CountingProxy(IEnumerable e) { this.e = e; }
            public IEnumerator GetEnumerator() { return GetKeyedEnumerator(e); }
        }

        /// <summary>
        /// Wraps the specified IEnumerable.  Use this method if you want to foreach.
        /// </summary>
        /// <param name="enumerable">Enumerable to wrap.</param>
        /// <returns>An IEnumerable that returns an appropriate iterator for the wrapped item.</returns>
        public static IEnumerable GetKeyedEnumerable(IEnumerable enumerable)
        {
            if (enumerable is IDictionary) return enumerable;
            return new CountingProxy(enumerable);
        }
    }
}

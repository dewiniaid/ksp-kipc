using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace KIPC.Util
{
    /// <summary>
    /// Implements a caching dictionary.
    /// 
    /// A caching dictionary has an optional maximum number of elements.  If additional elements are added beyond that maximum, the least recently used members
    /// are removed from the dictionary.
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class CacheDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        #region Node inner class
        /// <summary>
        /// Internal representation of our linked list.  We don't use the actual LinkedList since we don't care about the actual list structure, nor the value of the nodes
        /// We only care about their order and the ability to quickly change that.
        /// </summary>
        protected class Node
        {
            public Node prev { get; set; } = null;
            public Node next { get; set; } = null;
            public TKey value;

            public Node(TKey value) {
                this.value = value;
            }

            /// <summary>
            /// Inserts us after the target node, removing us from our current location if we have one.
            /// </summary>
            /// <param name="node">Node to be inserted after</param>
            public void InsertAfter(Node node)
            {
                if (node == null) throw new ArgumentNullException();
                Remove();
                next = node.next;
                if (next != null) next.prev = this;
                node.next = this;
                this.prev = node;
            }

            /// <summary>
            /// Inserts us before the target node, removing us from our current location if we have one.
            /// </summary>
            /// <param name="node">Node to be inserted after</param>
            public void InsertBefore(Node node)
            {
                if (node == null) throw new ArgumentNullException();
                Remove();
                prev = node.prev;
                if (prev != null) prev.next = this;
                node.prev = this;
                this.next = node;
            }

            public void Remove()
            {
                if (prev != null) prev.next = next;
                if (next != null) next.prev = prev;
                next = prev = null;
            }

            static public Node CreateBefore(Node node, TKey value)
            {
                Node result = new Node(value);
                result.InsertBefore(node);
                return result;
            }

            static public Node CreateAfter(Node node, TKey value)
            {
                Node result = new Node(value);
                result.InsertAfter(node);
                return result;
            }
        }
        #endregion

        #region Node properties
        private Node firstNode, lastNode;
        private void InitializeNodes()
        {
            firstNode = new Node(default(TKey));
            lastNode = Node.CreateAfter(FirstNode, default(TKey));

        }
        protected Node FirstNode { get { if (firstNode == null) InitializeNodes(); return firstNode; } }
        protected Node LastNode { get { if (firstNode == null) InitializeNodes(); return lastNode; } }
        #endregion

        protected int maxSize = 0;

        public int MaxSize
        {
            get { return maxSize; }
            set { maxSize = value; if (maxSize > 0) Prune(Count - maxSize); }
        }

        private Dictionary<TKey, Node> recentKeys = new Dictionary<TKey, Node>();

        /// <summary>
        /// Bump a key to the front of the line.
        /// </summary>
        /// <param name="key"></param>
        protected void MoveToFront(TKey key)
        {
            Node node;
            try
            {
                node = recentKeys[key];
            }
            catch (KeyNotFoundException)
            {
                node = new Node(key);
                if (MaxSize > 0 && MaxSize < Count)
                {
                    Prune(1);
                }
            }
            node.InsertAfter(FirstNode);
            recentKeys[key] = node;
        }

        /// <summary>
        /// Remove up to the specified number of items.  Will not remove FirstNode or LastNode.
        /// </summary>
        /// <param name="count"># of items to remove.  If zero or negative, this is a noop.</param>
        protected void Prune(int count)
        {
            if (count <= 0) return; // noop
            var node = LastNode.prev;
            while(count-- > 0 && node.prev != null)
            {
                recentKeys.Remove(node.value);
                base.Remove(node.value);
            }
            // Now on the last node we want to keep, which might be the first node.
            node.next = LastNode;
            LastNode.prev = node;
            // cut off elements will be gc'd.
        }

        public CacheDictionary() : base() { }

        public CacheDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) {
            foreach(var key in Keys) { this.MoveToFront(key);  }
        }

        public CacheDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        public CacheDictionary(int capacity) : base(capacity) { maxSize = capacity; }

        public CacheDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) {
            foreach (var key in Keys) { this.MoveToFront(key); }
        }

        public CacheDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { maxSize = capacity; }

        new public TValue this[TKey key]
        {
            get
            {
                // Done in this order to avoid mucking up the cache and throw an exception instead if the key doesn't exist.
                var result = base[key];
                MoveToFront(key);
                return result;
            }
            set
            {
                base[key] = value;
                MoveToFront(key);
            }
        }

        new public void Add(TKey key, TValue value)
        {
            this.MoveToFront(key);
            base.Add(key, value);
        }

        new public bool Remove(TKey key)
        {
            if (base.Remove(key)) {
                recentKeys[key].Remove();
                recentKeys.Remove(key);
                return true;
            }
            return false;
        }

        new public void Clear()
        {
            base.Clear();
            recentKeys.Clear();
            FirstNode.next = LastNode;
            LastNode.prev = FirstNode;
        }

        new public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = base.TryGetValue(key, out value);
            if (result) MoveToFront(key);
            return result;
        }

        /// <summary>
        /// Retrieves the value of a key from the cache without moving it to the front.
        /// </summary>
        /// <param name="key">Key to retrieve</param>
        /// <returns></returns>
        public TValue Peek(TKey key)
        {
            return base[key];
        }
    }
}

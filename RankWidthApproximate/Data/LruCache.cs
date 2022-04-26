using System.Collections.Generic;

namespace RankWidthApproximate.Data
{
    /// <summary>
    /// Cache with a least-recently-used replacement strategy, based on a Dictionary and a LinkedList.
    /// This class is not thread-safe.
    /// </summary>
    /// <typeparam name="K">The type of the keys</typeparam>
    /// <typeparam name="V">The type of the values</typeparam>
    public class LruCache<K, V>
    {
        private class ListNode
        {
            public readonly K Key;
            public          V Value;

            public ListNode(K key, V value)
            {
                Key   = key;
                Value = value;
            }
        }

        private readonly Dictionary<K, LinkedListNode<ListNode>> _data;

        private readonly LinkedList<ListNode> _list = new();

        /// <summary>
        /// The maximum amount of elements in this cache
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Constructs a new least-recently-used cache with the given capacity
        /// </summary>
        /// <param name="capacity">The cache capacity</param>
        public LruCache(int capacity)
        {
            Capacity = capacity;
            _data    = new(Capacity);
        }

        /// <summary>
        /// Adds an item to the cache, or changes the value if the key already exists
        /// </summary>
        /// <param name="key">The key of the item</param>
        /// <param name="value">The value of the item</param>
        public void Add(K key, V value)
        {
            if (_data.TryGetValue(key, out var keyNode))
            {
                // Update value if item is already in the cache
                keyNode.Value.Value = value;
                _list.Remove(keyNode);
                _list.AddLast(keyNode);
                return;
            }

            if (_data.Count == Capacity)
            {
                var head = _list.First;
                _data.Remove(head.Value.Key);
                _list.Remove(head);
            }

            var node = new ListNode(key, value);
            _data.Add(key, _list.AddLast(node));
        }

        /// <summary>
        /// Gets the value that belongs to the given key if available
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="value">The value belonging to the key, if available</param>
        /// <returns></returns>
        public bool TryGet(K key, out V value)
        {
            if (!_data.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            _list.Remove(node);
            _list.AddLast(node);

            value = node.Value.Value;
            return true;
        }
    }
}
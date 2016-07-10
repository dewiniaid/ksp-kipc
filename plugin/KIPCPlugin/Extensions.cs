using System.Collections.Generic;
namespace KIPC.Extensions { 
    public static class DictionaryExtensions {
        /// <summary>
        /// Get a the value for a key. If the key does not exist, return null;
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dic">The dictionary to call this method on.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The key value. null if this key is not in the dictionary.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default(TValue))
        {
            TValue result;
            return dic.TryGetValue(key, out result) ? result : defaultValue;
        }
    }
}

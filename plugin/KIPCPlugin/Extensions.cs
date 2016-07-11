using System;
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

    public static class TypeExtensions
    {
        /// <summary>
        /// Counterpart to IsSubclassOf.  Also returns true if the two types are the same type.   This differs from IsAssignableFrom in that it compares actual types and ignores cases
        /// where a type supports assignment from another type that's not one of its subclasses.
        /// </summary>
        /// <param name="parent">Parent type</param>
        /// <param name="child">Possible child type</param>
        /// <returns>True if the child and parent represent the same type or the child is a subclass of the parent.</returns>
        public static bool IsParentClassOf(this Type parent, Type child)
        {
            return child == parent || child.IsSubclassOf(parent);
        }
    }
}

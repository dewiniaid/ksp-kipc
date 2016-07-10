using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace KIPC.Util
{
    /// <summary>
    /// Provides a handful of static methods for argument validation.
    /// </summary>
    public class Validation
    {
        // [Serializable]
        public class ElementException : ArgumentException
        {
            readonly object element;
            public ElementException(object element) {
                this.element = element;
            }
            public ElementException(object element, string message) : base(message)
            {
                this.element = element;
            }
            public ElementException(object element, string message, Exception inner) : base(message, inner) {
                this.element = element;
            }
            // protected ElementException(
            //   System.Runtime.Serialization.SerializationInfo info,
            // System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

            public static ElementException Wrap(object element, ArgumentException ex)
            {
                if (ex is ArgumentNullException)
                {
                    return new ElementNullException(element, ex.Message, ex.InnerException);
                } else {
                    return new ElementException(element, ex.Message, ex.InnerException);
                }
            }
        }

        // [Serializable]
        public class ElementNullException : ElementException
        {
            public ElementNullException(object element) : base(element) { }
            public ElementNullException(object element, string message, Exception inner) : base(element, message, inner) { }
            public ElementNullException(object element, string message) : base(element, message) { }
            //protected ElementNullException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        /// <summary>
        /// Raises an exception if the value is not of the specified type, or if it is null and allowNull is false.
        /// </summary>
        /// <typeparam name="TType">Required type</typeparam>
        /// <param name="value">Value to test.</param>
        /// <param name="allowNull">True if the value is allowed to be null.</param>
        /// <returns>value if no exception is raised.</returns>
        public static void AssertType<TType>(object value, bool allowNull = false)
        {
            if (value is TType) return;
            if (value == null)
            {
                if (allowNull) return;
                throw new ArgumentNullException();
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Raises an exception if the value is not of the specified type, or if it is null and allowNull is false.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <param name="type">Required type</param>
        /// <param name="allowNull">True if the value is allowed to be null.</param>
        public static void AssertType(object value, Type type, bool allowNull)
        {
            if (value == null)
            {
                if (allowNull) return;
                throw new ArgumentNullException();
            }
            var valueType = value.GetType();
            // We could use type.IsAssignableFrom here, but we want to ensure it actually is that type, not merely castable.
            if (valueType == type || valueType.IsSubclassOf(type)) return;
            throw new ArgumentException();
        }

        /// <summary>
        /// Raises an exception if the value is not of one of the specified types, or if it is null and allowNull is false.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <param name="types">Allowed types</param>
        /// <param name="allowNull">True if the value is allowed to be null.</param>
        public static void AssertType(object value, IEnumerable<Type>[] types, bool allowNull)
        {
            if (value == null)
            {
                if (allowNull) return;
                throw new ArgumentNullException();
            }
            var valueType = value.GetType();
            foreach (Type type in types)
            {
                // We could use type.IsAssignableFrom here, but we want to ensure it actually is that type, not merely castable.
                if (valueType == type || valueType.IsSubclassOf(type)) return;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Raises an exception if not all of the values in the collection are of the specified type or if any of them are null and allowNull is false.
        /// </summary>
        /// <typeparam name="TType">Required type</typeparam>
        /// <param name="collection">Collection of values to test.</param>
        /// <param name="allowNull">True if values are allowed to be null.</param>
        /// <returns>value if no exception is raised.</returns>
        public static void AssertAllOfType<TType>(IEnumerable collection, bool allowNull = false)
        {
            foreach(DictionaryEntry kvp in CountingIterator.GetKeyedEnumerable(collection))
            {
                try
                {
                    AssertType<TType>(kvp.Value, allowNull);
                } catch (ArgumentException ex) {
                    throw ElementException.Wrap(kvp.Key, ex);
                }
            }
            return;
        }

        /// <summary>
        /// Raises an exception if not all of the values in the collection are of the specified type or if any of them are null and allowNull is false.
        /// </summary>
        /// <param name="collection">Collection of values to test.</param>
        /// <param name="type">Required type</param>
        /// <param name="allowNull">True if values are allowed to be null.</param>
        public static void AssertAllOfType(IEnumerable collection, Type type, bool allowNull)
        {
            foreach (DictionaryEntry kvp in CountingIterator.GetKeyedEnumerable(collection))
            {
                try
                {
                    AssertType(kvp.Value, type, allowNull);
                }
                catch (ArgumentException ex)
                {
                    throw ElementException.Wrap(kvp.Key, ex);
                }
            }
            return;
        }

        /// <summary>
        /// Raises an exception if not all of the values in the collection are one of the specified types or if any of them are null and allowNull is false.
        /// Raises an exception if the value is not of one of the specified types, or if it is null and allowNull is false.
        /// </summary>
        /// <param name="collection">Collection of values to test.</param>
        /// <param name="types">Allowed types</param>
        /// <param name="allowNull">True if values are allowed to be null.</param>
        public static void AssertAllOfType(IEnumerable collection, IEnumerable<Type>[] types, bool allowNull)
        {
            foreach (DictionaryEntry kvp in CountingIterator.GetKeyedEnumerable(collection))
            {
                try
                {
                    AssertType(kvp.Value, types, allowNull);
                }
                catch (ArgumentException ex)
                {
                    throw ElementException.Wrap(kvp.Key, ex);
                }
            }
            return;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using KIPC.Util;

namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;
    using IJsonDict = IDictionary<string, object>;

    public class TypeHandler : JsonDict
    /// Base implementation for SerializedTypes.
    /// 
    /// Serialized Types turn into JSON Dictionaries (i.e. string: object).  They contain -- at minimum -- a 'type' field to identify them
    /// and a 'data' field to represent their data.
    /// 
    /// TODO: This really shouldn't be a Dictionary subclass (it should instead contain a dictionary), but I haven't figured out how to really
    /// control JsonFx and the documentation, frankly, sucks.
    {
        #region Properties
        /// <summary>
        /// Keep a pointer to our own SerializesAttribute, which will be initialized during a reflection pass.  It cannot be changed afterwards.
        /// </summary>
        public readonly TypeSerializer info = null;

        /// <summary>
        /// Know who our serializer is in case we need to call it for support functions.
        /// </summary>
        public readonly Serializer serializer = null;
        #endregion
        internal TypeHandler(TypeSerializer info, Serializer serializer)
        {
            this.info = info;
            this.serializer = serializer;
            this["type"] = info.Identifier;
            this["data"] = null;
        }

        internal MethodInfo SerializeMethod, DeserializeMethod;
        public TypeHandler ProxySerialize(object input)
        {
            object[] args = { input };  // Hopefully this is stack allocated
            return (TypeHandler) SerializeMethod.Invoke(this, new object[] { input });
        }

        public object ProxyDeserialize(IJsonDict source)
        {
            return DeserializeMethod.Invoke(this, new object[] { source });
        }

        public void Import(IJsonDict source)
        {
            foreach(JsonKey kvp in source)
            {
                this[kvp.Key] = kvp.Value;
            }
        }
    }
    /// <summary>
    /// Base implementation for SerializedTypes.
    /// 
    /// Serialized Types turn into JSON Dictionaries (i.e. string: object).  They contain -- at minimum -- a 'type' field to identify them
    /// and a 'data' field to represent their data.
    /// 
    /// TODO: This really shouldn't be a Dictionary subclass (it should instead contain a dictionary), but I haven't figured out how to really
    /// control JsonFx and the documentation, frankly, sucks.
    /// </summary>
    public abstract class TypeHandler<T> : TypeHandler
    {
        /// <summary>
        /// Construct a new TypeHandler
        /// </summary>
        /// <param name="serializer">Serializer that will provide services for us.</param>
        protected TypeHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        /// <summary>
        /// Serialize the specified input, generally by setting appropriate key/value pairs in the dictionary we subclass.
        /// Generally returns this, but may return something else in special cases.
        /// </summary>
        /// <param name="input">Input object.</param>
        /// <returns></returns>
        public abstract TypeHandler Serialize(T input);

        /// <summary>
        /// Deserialize ourselves and recreate the requested original input.
        /// </summary>
        /// <returns>Deserialized object</returns>
        public abstract T Deserialize(IJsonDict source);

        /// <summary>
        /// Convenience method; raises a SerializationException if the specified key (default 'data') does not exist.
        /// </summary>
        /// <param name="key">Key in dictionary to look for </param>
        protected void EnsureValueIsType<TType>(string key = "data", bool mustExist = true, bool allowNull = false)
        {
            EnsureValueIsType<TType>(this, key, mustExist, allowNull);
        }
        protected void EnsureValueIsType<TType>(IJsonDict source, string key = "data", bool mustExist = true, bool allowNull = false)
        {
            object value;
            try
            {
                value = source[key];
            } catch (KeyNotFoundException)
            {
                if (mustExist)
                {
                    throw new SerializationException(
                        string.Format(
                            "{0} object: Expected attribute '{1}' of type '{2}' was not found.",
                            this.info.Name, key, typeof(TType).Name
                        )
                    );

                }
                return;
            }
            try
            {
                Validation.AssertType<TType>(value, allowNull);
            } catch (ArgumentNullException)
            {
                throw new SerializationException(
                    string.Format(
                        "{0} object: Expected attribute '{1}' to be of type '{2}', but it was null instead.",
                        this.info.Name, key, typeof(TType).Name, value.GetType().Name
                    )
                );
            } catch (ArgumentException) {
                throw new SerializationException(
                    string.Format(
                        "{0} object: Expected attribute '{1}' to be of type '{2}', but encountered '{3}' instead.",
                        this.info.Name, key, typeof(TType).Name, value.GetType().Name
                    )
                );
            }
        }
    }
}

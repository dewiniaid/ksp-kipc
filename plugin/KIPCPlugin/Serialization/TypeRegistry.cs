using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using System.Linq;

using UnityEngine;

using KIPC.Extensions;

namespace KIPC.Serialization
{
    /// <summary>
    /// Keeps track of which TypeHandler/TypeSerializerAttributes handle which things.
    /// </summary>
    public class TypeRegistry
    {
        /// <summary>
        /// List of all type serializers in priority order.
        /// </summary>
        protected List<TypeSerializer> typeSerializers;

        /// Mapping of type identifiers to serializers.
        protected Dictionary<object, TypeSerializer> serializersByIdentifier = null;

        /// Cache of Types to serializers.
        protected Util.CacheDictionary<Type, TypeSerializer> serializerCache = null;

        public TypeRegistry()
        {
            RefreshTypeData();
        }

        public void RefreshTypeData()
        {
            var typeSerializers = new List<TypeSerializer>();
            var byIdentifier = new Dictionary<object, TypeSerializer>();
            var serializerCache = new Util.CacheDictionary<Type, TypeSerializer>();
            TypeSerializer ts;

            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(TypeHandler).IsAssignableFrom(t)))
            {
                foreach (SerializesAttribute attr in type.GetCustomAttributes(typeof(SerializesAttribute), false))
                {
                    Debug.Log(string.Format("[KIPCPlugin] Registering type serializer '{0}' with identifier {1}, handled by {2} for items of type {3}", attr.Name, attr.Identifier, type.FullName, attr.NativeType.FullName));
                    ts = new TypeSerializer(attr, type);
                    if (byIdentifier.ContainsKey(attr.Identifier))
                    {
                        throw new InvalidOperationException(
                            string.Format("More than one serializer references the type identifier {0}.  This is most certainly not supposed to happen.  Please report this bug.", attr.Identifier)
                        );
                    }
                    if (serializerCache.ContainsKey(ts.NativeType))
                    {
                        throw new InvalidOperationException(
                            string.Format("More than one serializer references the native type identifier {0}.  This is most certainly not supposed to happen.  Please report this bug.", ts.NativeType.FullName)
                        );
                    }
                    typeSerializers.Add(ts);
                    serializerCache[ts.NativeType] = ts;
                    byIdentifier[ts.Identifier] = ts;
                }
            }
            serializerCache.MaxSize = 3 * typeSerializers.Count;  // Arbitrary multiplier.  Should handle various subclasses and such, without growing infinitely if something creates a ton of dynamic types.
            typeSerializers.Sort();  // Sort in priority order.
            Debug.LogWarning(string.Format("[KIPCPlugin] Registered {0} serializer(s)", typeSerializers.Count));

            this.typeSerializers = typeSerializers;
            this.serializersByIdentifier = byIdentifier;
            this.serializerCache = serializerCache;
        }

        public TypeSerializer GetDeserializer(object identifier)
        {
            try
            {
                return serializersByIdentifier[identifier];
            }
            catch (KeyNotFoundException) 
            {
                throw new SerializationException(string.Format("No deserializer is defined for object class {0}", identifier));
            }
        }

        public TypeSerializer GetSerializer(object instance)
        {
            var type = instance.GetType();
            TypeSerializer result;
            if (serializerCache.TryGetValue(type, out result))
            {
                if (result != null) return result;
            }
            else
            {
                result = typeSerializers.Find(x => x.NativeType.IsParentClassOf(type));
                serializerCache[type] = result;
            }
            if (result == null)
            {
                throw new SerializationException(string.Format("No serializer is defined for native type {0}", type.FullName));
            }
            return result;
        }
    }
}

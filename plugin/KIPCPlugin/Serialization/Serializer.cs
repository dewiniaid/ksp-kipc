using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using System.Linq;
using kOS.Safe.Encapsulation;
using UnityEngine;
using KIPC.Extensions;
using kOS;
using JsonFx.Json;
using JsonFx.Bson;

namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;
    using IJsonDict = IDictionary<string, object>;

    /// <summary>
    /// An instance of Serializer handles a single serialization run, including maintaining internal state.
    /// </summary>
    public class Serializer
    {
        public SharedObjects SharedObjects { get; private set; }

        public Serializer(SharedObjects sharedObjects)
        {
            SharedObjects = sharedObjects;
        }

        #region Static JSON/BSON I/O
        private static JsonReader jsonReader = new JsonReader(
            new JsonFx.Serialization.DataReaderSettings(new JsonFx.Serialization.Resolvers.PocoResolverStrategy())
        );
        private static JsonWriter jsonWriter = new JsonWriter(
            new JsonFx.Serialization.DataWriterSettings(new JsonFx.Serialization.Resolvers.PocoResolverStrategy())
        );

        public static string WriteJson(SharedObjects sharedObjects, object input)
        {
            return jsonWriter.Write(new Serializer(sharedObjects).Serialize(input));
        }

        public static object ReadJson(SharedObjects sharedObjects, string input) 
        {
            return new Serializer(sharedObjects).Deserialize(jsonReader.Read(input));
        }
        #endregion
        #region Type Registry
        public static TypeRegistry registry = new TypeRegistry();
        public static void RefreshTypeRegistry()
        {
            registry.RefreshTypeData();
        }
        #endregion

        #region Serialization
        public class SerializerStateInfo
        {
            public int nextReferenceId = 1;  // ID that will be assigned next time we notice a reference.
            public Dictionary<object, ICollectionHandler> references = new Dictionary<object, ICollectionHandler>();  // Collections that we've encountered this pass.
        }
        public SerializerStateInfo serializerState = new SerializerStateInfo();
        protected int nextReferenceId = 1;  // ID that will be assigned next time we notice a reference.
        protected Dictionary<object, ICollectionHandler> references = new Dictionary<object, ICollectionHandler>();  // Collections that we've encountered this pass.

        protected object MakeReference(ICollectionHandler referenced)
        {
            if (referenced.ObjectId == null) referenced.ObjectId = serializerState.nextReferenceId++;
            return new JsonDict {
                {"type", "ref" },
                {"ref", referenced.ObjectId }
            };
        }

        public object Serialize(object input)
        {
            // If Unity supported a modern C# version with, say, dynamic, we could go back to the overloaded methods approach
            // Instead of this giant mess of if-checks
            if (input is PrimitiveStructure)
            {
                return ((PrimitiveStructure)input).ToPrimitive();
            }
            ICollectionHandler referenced = serializerState.references.GetValueOrDefault(input);
            if (referenced != null) return MakeReference(referenced);
            TypeHandler handler = registry.GetSerializer(input).CreateHandler(this);  // Will throw on unsupported types.
            if (handler is ICollectionHandler)
            {
                // It's important that the reference is set up BEFORE we serialize the handler, otherwise a nested reference to the input may fail to notice
                // the existing reference since we haven't created it yet.
                serializerState.references[input] = (ICollectionHandler)handler;
            }
            return handler.ProxySerialize(input);
        }
        #endregion

        #region Deserialization
        public class DeserializerStateInfo
        {
            public Dictionary<object, IJsonDict> references;
            public Stack<IEnumerable> pending;
            public Dictionary<IJsonDict, object> results;
        }

        public DeserializerStateInfo deserializerState = null;

        /// <summary>
        /// Returns the object type identifier from the specified JSON Dict.  Raises an exception if no type identifier is specified.
        /// </summary>
        /// <param name="input"></param>
        protected object GetTypeIdentifier(IJsonDict input)
        {
            try
            {
                return input["type"];
            } catch (KeyNotFoundException) {
                throw new SerializationException("Object type not declared.");
            }
        }

        /// <summary>
        /// Helper function for resolving references.  Updates the passed list of references.  Returns a non-null value if the parent container should replace
        /// its current item with the returned item.
        /// </summary>
        /// <param name="references"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal IJsonDict ResolveItem(IJsonDict item)
        {
            object typeId = GetTypeIdentifier(item);
            object objectId = item.GetValueOrDefault("ref");
            // Debug.Log(string.Format("Identified object of type {0} -- object ID: {1}", typeId, objectId));
            IJsonDict referenced = (objectId == null) ? null : deserializerState.references.GetValueOrDefault(objectId);
            if (typeId as string == "ref") { // Found a reference
                if (objectId == null) throw new SerializationException("Found a missing or null reference ID.");
                // This works because we don't want to replace if the item didn't exist (and thus would return NULL),
                // and we do want to replace if it did (and thus would return an actual value)
                if (referenced == null)
                {
                    // Debug.Log(string.Format("Adding new pending reference to {0}", objectId));
                    deserializerState.references[objectId] = item;
                } else
                {
                    // Debug.Log(string.Format("Replacing reference with actual object of type ", referenced["type"]));
                }
                return referenced;
            }
            // Still here?  Okay.
            // Nested container check.
            TypeHandler handler = registry.GetDeserializer(typeId).CreateHandler(this);
            if (handler is ICollectionHandler)
            {
                // FIXME: See if this code is actually needed.
                if (item.ContainsKey("_visited"))
                {
                    Debug.LogWarning("*** WARNING ***: Container already visited.");
                }
                else
                {
                    item["_visited"] = true;
                    foreach (var container in ((ICollectionHandler)handler).GetContainers(item))
                    {
                        deserializerState.pending.Push(container);
                    }
                }
            }

            if (objectId == null) return null; // Unreferenced object, nothing to do.
            if (referenced == null)
            {
                deserializerState.references[objectId] = item;
                // Debug.Log("Saving reference.");
            }
            else
            {
                // Already references something else.  Let's hope it's a reference.
                if (GetTypeIdentifier(referenced) as string != "ref")
                {
                    // ... nope.  Complain loudly.
                    throw new SerializationException(string.Format("Object reference '{0}' refers to multiple non-reference objects", objectId));
                }
                // Everything we've previously seen up until now points to the old object.  It's way too much of a hassle to replace it, so...
                // instead we clear it and copy all of our data to it.
                // Debug.Log(string.Format("Copying over to actual object of type ", referenced["type"]));
                referenced.Clear();
                foreach(JsonKey kvp in item)
                {
                    referenced[kvp.Key] = kvp.Value;    // Resistance is futile.  You will be assimilated.
                }
                // Debug.Log("Done copying over");
            }
            return referenced;
        }
        public void ResolveReferences(ICollectionHandler handler, IJsonDict item)
        {
            deserializerState = new DeserializerStateInfo();
            deserializerState.references = new Dictionary<object, IJsonDict>();
            deserializerState.pending = new Stack<IEnumerable>(handler.GetContainers(item));
            deserializerState.results = new Dictionary<IJsonDict, object>();

            object referenceId = item.GetValueOrDefault("ref");
            if (referenceId != null) deserializerState.references[referenceId] = item;

            IEnumerable enumerable;
            while (deserializerState.pending.Count > 0)
            {
                enumerable = deserializerState.pending.Pop();
                bool dummy;
                // Debug.Log(string.Format("Processing queued item {0}", idgen.GetId(enumerable, out dummy)));
                
                var list = enumerable as IJsonList;
                if (list != null) {
                    for (int index = 0; index < list.Count; ++index)
                    {
                        if ((item = list[index] as IJsonDict) == null) continue;  // Not an object
                        if ((item = ResolveItem(item)) == null) continue;  // Replacement not needed
                        list[index] = item;
                    }
                    continue;
                }
                var dict = enumerable as IJsonDict;
                if (dict != null) {
                    foreach (JsonKey kvp in dict)
                    {
                        if ((item = kvp.Value as IJsonDict) == null) continue;  // Not an object
                        if ((item = ResolveItem(item)) == null) continue;  // Replacement not needed.
                        dict[kvp.Key] = item;
                    }
                    continue;
                }
                throw new SerializationException(string.Format("Unexpected enumerable of type {0}", enumerable.GetType().FullName));
            }
        }

        public object Deserialize(object input)
        {
            if (input == null) throw new SerializationException("Encountered NULL while deserializing input.");
            // Primitives.
            input = Structure.FromPrimitive(input);
            if (input is Structure) return (Structure)input;
            IJsonDict dict = input as IJsonDict;
            if (dict == null)
            {
                throw new SerializationException("Deserializing from a " + input.GetType().Name + " is unsupported.");
            }
            // Get the deserializer for this object.
            // This will fail and throw an exception(by design) on unsupported types. 
            // "ref" is also considered an unsupported type.  If we see it here, it's either the top-level element 
            // (in which case there are no other elements it could possibly be referencing), or we somehow failed
            // to replace it during the reference replacement run.
            TypeHandler handler = registry.GetDeserializer(GetTypeIdentifier(dict)).CreateHandler(this);
            ICollectionHandler ich = handler as ICollectionHandler;
            object result;
            bool dummy;

            if (ich != null) {
                // If we have a collection handler and haven't scanned for backrefs yet, scan for them.
                if (deserializerState == null) ResolveReferences(ich, dict);
                // Is this something we've already resolved?  If so, return that reference.
                result = deserializerState.results.GetValueOrDefault(dict);
                if (result != null)
                {
                    return result;
                }
            }
            result = handler.ProxyDeserialize(dict);
            if (ich != null)
            {
                deserializerState.results[dict] = result;
            }
            return result;
        }
        #endregion
    }
}
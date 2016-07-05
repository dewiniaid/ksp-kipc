using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using UnityEngine;

using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.AddOns;

using JsonFx.Json;

namespace KIPCPlugin.KOS
{
    [kOSAddon("KIPC")]
    [kOS.Safe.Utilities.KOSNomenclature("KIPC")]
    public class Addon : kOS.Suffixed.Addon
    {
        private static JsonWriter jsonWriter = new JsonWriter();

        public Addon(kOS.SharedObjects shared) : base(shared)
        {
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("SERIALIZE", new OneArgsSuffix<StringValue, Structure>(Serialize));
        }

        private static StringValue Serialize(Structure input)
        {
            return jsonWriter.Write(new Serializer().Serialize(input));
        }

        public override BooleanValue Available()
        {
            return true;
        }
    }

    /// <summary>
    /// Exists solely to provide a way for kOS scripts to determine if KRPC is present.
    /// </summary>
    [kOSAddon("KRPC")]
    [kOS.Safe.Utilities.KOSNomenclature("KRPC")]
    public class KRPCAvailabilityIndicator : Addon
    {
        public KRPCAvailabilityIndicator(kOS.SharedObjects shared) : base(shared) { }

        public override BooleanValue Available()
        {
            return KIPC.Addon.hasKRPC;
        }
    }

    public enum WrappedType
    {
        List, Queue, Stack, Dict,
        Reference,
        Vector, Direction, Quaternion,
        Vessel, Part, Body,
        Invalid = -1
    }

    public class Typewrapper : Dictionary<string, object>
    {
        public WrappedType type { get; private set; }
        public object data {
            get { return this["data"]; }
            set { this["data"] = value; }
        }
        public Typewrapper(WrappedType type, object data = null) : base()
        {
            this["type"] = type.ToString().ToLowerInvariant();
            this["data"] = data;
        }
    }

    public class SerializationException : Exception
    {
        public SerializationException() { }
        public SerializationException(String message) : base(message) { }
        public SerializationException(String message, Exception inner) : base(message, inner) { }
    }

    public class Serializer
    {
        /// <summary>
        /// Per-instance object ID generator for recursive reference detection.
        /// </summary>
        private ObjectIDGenerator idgenerator = new ObjectIDGenerator();
        /// <summary>
        /// Current mapping of references so we can detect recursive references.
        /// </summary>
        public Dictionary<long, Typewrapper> idMap = new Dictionary<long, Typewrapper>();
        
        public Serializer()
        {
            // rrefDict = new Dictionary<Structure, Typewrapper>();
        }

        private Typewrapper RefCapableWrapper(WrappedType type, Structure s, out bool cached)
        {
            bool firstTime;
            long objectId = idgenerator.GetId(s, out firstTime);
            Debug.LogFormat(
                "Identified object type '{0}' with id {1}, firstTime={2}; in rrefDict={3}", s.KOSName, objectId, firstTime, idMap.ContainsKey(objectId)
            );
            if ( (cached = !firstTime) )  // Assignment intentional
            {
                idMap[objectId]["ref"] = objectId;
                return new Typewrapper(WrappedType.Reference, objectId);
            }
            return idMap[objectId] = new Typewrapper(type);
        }

        public object Serialize(Structure s)
        {
            // If Unity supported a modern C# version with, say, dynamic, we could go back to the overloaded methods approach
            // Instead of this giant mess of if-checks
            if(s is PrimitiveStructure)
            {
                return ((PrimitiveStructure)s).ToPrimitive();
            }
            if (s is Lexicon)
            {
                bool cached;
                var result = RefCapableWrapper(WrappedType.Dict, s, out cached);
                if (cached) return result;
                var input = (Lexicon)s;
                var data = new Dictionary<string, object>();
                var keys = new List<object>();
                var values = new List<object>();
                result.data = data;
                result["keys"] = keys;
                result["values"] = values;
                result["sensitive"] = false;    // FIXME: but kOS caseSensitive attr is private so we can't currently examine it.

                object key, value;
                foreach (var item in input)
                {
                    key = Serialize(item.Key);
                    value = Serialize(item.Value);
                    if (key is string)
                    {
                        data[(string)key] = value;
                    }
                    else
                    {
                        keys.Add(key);
                        values.Add(value);
                    }
                }
                return result;

            }
            if (s is IEnumerable<Structure>)
            {
                var input = (IEnumerable<Structure>)s;
                var t = WrappedType.Invalid;
                if (s is ListValue)
                {
                    t = WrappedType.List;
                }
                else if (s is QueueValue)
                {
                    t = WrappedType.Queue;
                }
                else if (s is StackValue)
                {
                    t = WrappedType.Stack;
                }
                if(t != WrappedType.Invalid) {
                    bool cached;
                    var result = RefCapableWrapper(t, s, out cached);
                    if (cached) return result;
                    result.data = input.Select(x => Serialize(x)).ToArray<object>();
                    return result;
                }
            }
            if (s is BodyTarget)
            {
                return new Typewrapper(WrappedType.Body, ((BodyTarget)s).Body.flightGlobalsIndex);
            }
            if (s is VesselTarget)
            {
                return new Typewrapper(WrappedType.Vessel, ((VesselTarget)s).Vessel.id);
            }
            if (s is kOS.Suffixed.Part.PartValue) {
                return new Typewrapper(WrappedType.Part, ((kOS.Suffixed.Part.PartValue)s).Part.flightID);
            }
            throw new SerializationException("Objects of type " + s.KOSName + " cannot be serialized in this version of KIPC.");
        }
    }
}


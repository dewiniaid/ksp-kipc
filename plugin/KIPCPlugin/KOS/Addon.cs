using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using UnityEngine;

using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.AddOns;
using kOS;

using KIPC.Serialization;

namespace KIPC.KOS
{
    /// <summary>
    /// Implements the kOS-level addon.
    /// </summary>
    [kOSAddon("KIPC")]
    [kOS.Safe.Utilities.KOSNomenclature("KIPC")]
    public class Addon : kOS.Suffixed.Addon
    {
        private SharedObjects sharedObjects;
        public Addon(SharedObjects shared) : base(shared)
        {
            sharedObjects = shared;
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("SERIALIZE", new OneArgsSuffix<StringValue, Structure>(Serialize, "Encodes a value for messaging."));
            AddSuffix("DESERIALIZE", new OneArgsSuffix<Structure, StringValue>(Deserialize, "Deserializes an encoded message."));
        }

        private StringValue Serialize(Structure input)
        {
            return new StringValue(Serializer.WriteJson(sharedObjects, input));
        }

        private Structure Deserialize(StringValue input)
        {
            return (Structure)Serializer.ReadJson(sharedObjects, input);
        }

        public override BooleanValue Available()
        {
            return true;
        }
    }
}


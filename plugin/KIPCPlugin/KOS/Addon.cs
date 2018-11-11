using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using UnityEngine;

using kOS;
using kOS.AddOns;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.Suffixed.PartModuleField;
using kOS.Communication;

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
        private KRPCClient client;
        private KRPCConnection connection;

        public Addon(SharedObjects shared) : base(shared)
        {
            sharedObjects = shared;
            client = new KRPCClient(shared);
            connection = new KRPCConnection(shared, client);

            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("SERIALIZE", new OneArgsSuffix<StringValue, Structure>(Serialize, "Encodes a value for messaging.  Unstable API."));
            AddSuffix("DESERIALIZE", new OneArgsSuffix<Structure, StringValue>(Deserialize, "Deserializes an encoded message.  Unstable API."));
            AddSuffix("CONNECTION", new StaticSuffix<KRPCConnection>(() => this.connection, "Returns the Connection representing all connected KRPC Clients."));
            AddSuffix("SEND", new TwoArgsSuffix<BooleanValue, VesselTarget, Structure>(SendImmediate, "Immediately send a message to the specified target.  Developer API."));
        }

        private BooleanValue SendImmediate(VesselTarget target, Structure content)
        {
            double sentAt = Planetarium.GetUniversalTime();
            double receivedAt = sentAt;
            Message message = Message.Create(content, sentAt, receivedAt, VesselTarget.CreateOrGetExisting(shared), shared.Processor.Tag);
            MessageQueueStructure queue = InterVesselManager.Instance.GetQueue(target.Vessel, shared);
            queue.Push(message);
            return true;
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


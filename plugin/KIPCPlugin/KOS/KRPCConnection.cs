using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using kOS.Safe.Encapsulation;

using KIPC.Serialization;

namespace KIPC.KOS
{
    /// <summary>
    /// Represents a KRPC connection.  Placeholder.
    /// </summary>
    [kOS.Safe.Utilities.KOSNomenclature("KRPCConnection")]
    class KRPCConnection : kOS.Safe.Communication.Connection
    {
        public KRPCClient Client { get; private set; }
        private kOS.SharedObjects shared;
        public KRPCConnection(kOS.SharedObjects shared, KRPCClient client)
        {
            Client = client;
            this.shared = shared;
            // placeholder = (shared.Processor as kOS.Module.kOSProcessor).ExecuteInterProcCommand(command)

        }

        public override bool Connected { get; } = true;
        public override double Delay { get; } = 0;

        protected override Structure Destination()
        {
            return Client;
        }

        protected override BooleanValue SendMessage(Structure content)
        {
            KIPC.Addon.krpcMessageQueue.Enqueue(Serializer.WriteJson(shared, content));
            return true;
        }
    }
}

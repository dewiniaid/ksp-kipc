using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KRPC.Service;
using KRPC.Service.Attributes;

using KIPC.Serialization;

namespace KIPC.KRPC
{
    using global::KRPC.SpaceCenter.Services;
    /// <summary>
    /// Service for KIPC, a means of communication between kOS and KRPC.
    /// </summary>
    [KRPCService(GameScene = GameScene.All, Name = Service.SERVICE_NAME)]
    public static class Service
    {
        internal const string SERVICE_NAME = "KIPC";
        // Placeholder class for now

        /// <summary>
        /// Returns and removes the top message on the message queue.  Raises an exception if the queue is empty (since Null isn't a valid return value).
        /// </summary>
        /// <returns></returns>
        [KRPCProcedure]
        public static string PopMessage()
        {
            return KIPC.Addon.krpcMessageQueue.Dequeue();
        }

        /// <summary>
        /// Returns the top message on the message queue.  Raises an exception if the queue is empty (since Null isn't a valid return value).
        /// </summary>
        /// <returns></returns>
        [KRPCProcedure]
        public static string PeekMessage()
        {
            return KIPC.Addon.krpcMessageQueue.Peek();
        }

        [KRPCProcedure]
        public static IList<Processor> GetProcessors(Vessel vessel)
        {
            return vessel.InternalVessel.parts.SelectMany(x => x.Modules.OfType<kOS.Module.kOSProcessor>()).Select(x => new Processor(x)).ToList();
        }

        [KRPCProcedure]
        public static Vessel ResolveVessel(string vesselGuid)
        {
            return new Vessel(VesselHandler.GetVesselById(new Guid(vesselGuid)));

        }

        [KRPCProcedure]
        public static CelestialBody ResolveBody(int bodyId)
        {
            return new CelestialBody(BodyHandler.GetBodyById(bodyId));
        }
    }
}

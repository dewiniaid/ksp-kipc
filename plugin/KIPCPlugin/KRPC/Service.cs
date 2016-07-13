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
    using global::KRPC.SpaceCenter.Services.Parts;
    /// <summary>
    /// Service for KIPC, a means of communication and interaction between kOS and KRPC.
    /// </summary>
    [KRPCService(GameScene = GameScene.All, Name = "KIPC")]
    public static class Service
    {
        internal const string SERVICE_NAME = "KIPC";
        /// <summary>
        /// Returns and removes the top message on the message queue.  Raises an exception if the queue is empty (since Null isn't a valid return value).
        /// </summary>
        /// <returns>The JSON-encoded message.</returns>
        [KRPCProcedure]
        public static string PopMessage()
        {
            return KIPC.Addon.krpcMessageQueue.Dequeue();
        }

        /// <summary>
        /// Returns the top message on the message queue.  Raises an exception if the queue is empty (since Null isn't a valid return value).
        /// </summary>
        /// <returns>The JSON-encoded message.</returns>
        [KRPCProcedure]
        public static string PeekMessage()
        {
            return KIPC.Addon.krpcMessageQueue.Peek();
        }

        /// <summary>
        /// Returns all messages on the message queue in order.
        /// </summary>
        [KRPCProcedure]
        public static IList<string> GetMessages()
        {
            var result = KIPC.Addon.krpcMessageQueue.ToList();
            KIPC.Addon.krpcMessageQueue.Clear();
            return result;
        }

        /// <summary>
        /// The number of messages currently waiting in the queue.
        /// </summary>
        [KRPCProperty]
        public static int CountMessages { get { return KIPC.Addon.krpcMessageQueue.Count; } }
        
        /// <summary>
        /// Returns all kOSProcessors on the specified vessel.
        /// </summary>
        /// <param name="vessel">Target vessel</param>
        /// <returns></returns>
        [KRPCProcedure]
        public static IList<Processor> GetProcessors(Vessel vessel)
        {
            return vessel.InternalVessel.parts.SelectMany(x => x.Modules.OfType<kOS.Module.kOSProcessor>()).Select(x => new Processor(x)).ToList();
        }

        /// <summary>
        /// Returns all kOSProcessors on the specified part (usually 0 or 1).
        /// </summary>
        /// <param name="part">Target part</param>
        /// <returns></returns>
        [KRPCProcedure]
        public static IList<Processor> GetProcessor(Part part)
        {
            return part.InternalPart.Modules.OfType<kOS.Module.kOSProcessor>().Select(x => new Processor(x)).ToList();
        }

        /// <summary>
        /// Resolves a vessel GUID to an actual vessel.
        /// </summary>
        /// <param name="vesselGuid">Vessel GUID</param>
        /// <returns></returns>
        [KRPCProcedure]
        public static Vessel ResolveVessel(string vesselGuid)
        {
            return new Vessel(VesselHandler.GetVesselById(new Guid(vesselGuid)));
        }

        /// <summary>
        /// Resolves multiple vessel GUIDs to actual vessels.  Returned vessels will be in the same order as the inputs.
        /// </summary>
        /// <param name="vesselGuids">Vessel GUID</param>
        /// <returns></returns>
        [KRPCProcedure]
        public static IList<Vessel> ResolveVessels(IList<string> vesselGuids)
        {
            return vesselGuids.Select(guid => new Vessel(VesselHandler.GetVesselById(new Guid(guid)))).ToList();
        }

        /// <summary>
        /// Resolves a celestial body ID to an actual celestial body.
        /// </summary>
        /// <param name="bodyId"></param>
        /// <returns></returns>
        [KRPCProcedure]
        public static CelestialBody ResolveBody(int bodyId)
        {
            return new CelestialBody(BodyHandler.GetBodyById(bodyId));
        }

        /// <summary>
        /// Resolves a celestial body ID to an actual celestial body.
        /// </summary>
        /// <param name="bodyId"></param>
        /// <returns></returns>
        [KRPCProcedure]
        public static IList<CelestialBody> ResolveBodies(IList<int> bodyIds)
        {
            return bodyIds.Select(bodyId => new CelestialBody(BodyHandler.GetBodyById(bodyId))).ToList();
        }

        /// <summary>
        /// Returns a list of all parts tagged with the specified kOSNameTag on the specified vessel.
        /// </summary>
        /// <param name="vessel">Vessel</param>
        /// <param name="tag">Tag name</param>
        /// <returns></returns>
        [KRPCProcedure]
        public static IList<Part> GetPartsTagged(Vessel vessel, string tag)
        {
            return vessel.InternalVessel.parts.FindAll(p => p.Modules.OfType<kOS.Module.KOSNameTag>().Any(m => m.nameTag == tag)).Select(p => new Part(p)).ToList();
        }
    }
}

using System;
using kOS.Suffixed;
using KIPC.Util;
using KIPC.Extensions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;
    using IJsonDict = IDictionary<string, object>;

    [Serializes(typeof(VesselTarget), "vessel")]
    public class VesselHandler : TypeHandler<VesselTarget>
    {
        // Remember the last ~10 or so vessels we've looked up.
        protected static CacheDictionary<Guid, WeakReference> vesselCache = new CacheDictionary<Guid, WeakReference>(10);

        public VesselHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        public static Vessel GetVesselById(Guid id, bool useCache = true)
        {
            // Fast resolve if it's the active vessel, which is probably the most likely case.
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == id)
            {
                return FlightGlobals.ActiveVessel;
            }

            // Check cache.
            if (useCache)
            {
                var item = vesselCache.GetValueOrDefault(id, null);
                if (item != null)
                {
                    if (item.IsAlive) return (Vessel)item.Target;
                    vesselCache.Remove(id);
                }
            }

            Vessel vessel = FlightGlobals.Vessels.Find(v => v.id == id);
            if (vessel != null && useCache)
            {
                vesselCache[id] = new WeakReference(vessel);
            }
            return vessel;
        }

        public override VesselTarget Deserialize(IJsonDict source)
        {
            EnsureValueIsType<string>(source);
            Guid vesselId;
            try
            {
                vesselId = new Guid((string)source["data"]);
            }
            catch (Exception ex)
            {
                throw new SerializationException("Provided vessel GUID is invalid.", ex);
            }
            Vessel vessel = GetVesselById(vesselId);
            if (vessel == null) return null;  // kOS won't really like this, but meh.
            return VesselTarget.CreateOrGetExisting(vessel, serializer.SharedObjects);
        }

        public override TypeHandler Serialize(VesselTarget input)
        {
            this["data"] = input.Vessel.id.ToString();
            return this;
        }
    }
}
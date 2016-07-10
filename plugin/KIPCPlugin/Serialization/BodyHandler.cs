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

    [Serializes(typeof(BodyTarget), "body")]
    public class BodyHandler : TypeHandler<BodyTarget>
    {
        public BodyHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        public override BodyTarget Deserialize(IJsonDict source)
        {
            EnsureValueIsType<int>(source);
            int bodyId = (int)source["data"];
            CelestialBody body;
            try
            {
                body = FlightGlobals.Bodies[bodyId];
            } catch (IndexOutOfRangeException)
            {
                throw new SerializationException("Provided body ID is invalid.");
            }
            return new BodyTarget(body, serializer.SharedObjects);
        }

        public override TypeHandler Serialize(BodyTarget input)
        {
            this["data"] = input.Body.flightGlobalsIndex;
            return this;
        }
    }
}
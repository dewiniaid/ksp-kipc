using System;
using System.Collections;
using System.Collections.Generic;
using kOS.Suffixed;
using KIPC.Extensions;
using System.Linq;
using UnityEngine;

namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;
    using IJsonDict = IDictionary<string, object>;

    public abstract class GeometryHandler<T> : TypeHandler<T>
    {
        public GeometryHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        protected List<double> GetElements(IJsonDict source, int numElements = 0)
        {
            EnsureValueIsType<IJsonList>(source);
            IJsonList value = (IJsonList) source["data"];
            if(numElements != 0 && value.Count != numElements)
            {
                throw new SerializationException(
                    string.Format(
                        "{0} object: Expected data field to contain {1} elements, but it contained {2} instead.",
                        this.info.Name, numElements, value.Count
                    )
                );
            }
            try
            {
                return value.Cast<double>().ToList();
            } catch(InvalidCastException) {
                throw new SerializationException(string.Format("{0} object: All elements of data must be numeric values.", info.Name));
            }
        }
    }

    [Serializes(typeof(Vector), "vector")]
    public class SerializedVector : GeometryHandler<Vector>
    {
        public SerializedVector(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        public override Vector Deserialize(IJsonDict source)
        {
            var elements = GetElements(source, 3);
            return new Vector(elements[0], elements[1], elements[2]);
        }

        public override TypeHandler Serialize(Vector input)
        {
            this["data"] = new JsonList { input.X, input.Y, input.Z };
            return this;
        }
    }

    /// <summary>
    /// TODO: Better Quaternion support.
    /// </summary>
    [Serializes(typeof(Direction), "direction")]
    public class SerializedDirection : GeometryHandler<Direction>
    {
        public SerializedDirection(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        public override Direction Deserialize(IJsonDict source)
        {
            EnsureValueIsType<string>(source, "from", false, true);
            string directionType = (string)source.GetValueOrDefault("from", null);
            int numElements = 0;
            switch (directionType)
            {
                case null:
                    break;
                case "quaternion":
                    numElements = 4;
                    break;
                case "euler":
                case "vector":
                    numElements = 3;
                    break;
                default:
                    throw new SerializationException(string.Format("{0} object: 'from' must be one of (\"quaternioun\", \"euler\", \"vector\", or null).", info.Name));
            }
            var elements = GetElements(source, numElements);
            switch (elements.Count)
            {
                case 3: // From Euler angles (default) or vector (explicit)
                    return new Direction(new Vector3d(elements[0], elements[1], elements[2]), directionType != "vector");
                case 4: // Quaternion
                    return new Direction(new QuaternionD(elements[0], elements[1], elements[2], elements[3]));
                default:
                    throw new SerializationException(string.Format("{0} object: data field must contain exactly 3 or 4 elements.", info.Name));
            }
        }

        public override TypeHandler Serialize(Direction input)
        {
            this["data"] = new JsonList { input.Euler.x, input.Euler.y, input.Euler.z };
            return this;
        }
    }
}

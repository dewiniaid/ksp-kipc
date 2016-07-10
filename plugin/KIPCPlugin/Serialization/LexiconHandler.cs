using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using UnityEngine;

using kOS.Safe.Encapsulation;
using kOS.Suffixed;

using JsonFx.Json;
using JsonFx.Bson;

using KIPC.Extensions;

namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;
    using IJsonDict = IDictionary<string, object>;
    using System.Collections;

    [Serializes(typeof(Lexicon), "dict")]
    public class LexiconHandler : CollectionHandler<Lexicon>
    {
        public LexiconHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        protected void Validate(IJsonDict source)
        {
            EnsureValueIsType<IJsonDict>(source, "data");
            EnsureValueIsType<IJsonList>(source, "keys");
            EnsureValueIsType<IJsonList>(source, "values");
            EnsureValueIsType<Boolean>(source, "sensitive", mustExist: false);
        }

        public override Lexicon Deserialize(IJsonDict source)
        {
            Validate(source);
            bool caseSensitive = (bool)source.GetValueOrDefault("sensitive", false);
            var data = (IJsonDict)source["data"];
            var keys = (IJsonList)source["keys"];
            var values = (IJsonList)source["values"];
            if (keys.Count != values.Count)
            {
                throw new SerializationException("Dict must have the same number of values as it has keys.");
            }
            var output = new Lexicon();
            output.SetSuffix("CASESENSITIVE", caseSensitive);
            serializer.deserializerState.results[source] = output;
            data.Select(kvp => output[(Structure)serializer.Deserialize(kvp.Key)] = (Structure)serializer.Deserialize(kvp.Value));

            foreach (var kvp in data.Select(x => new KeyValuePair<Structure, Structure>((Structure)serializer.Deserialize(x.Key), (Structure)serializer.Deserialize(x.Value))))
            {
                output[kvp.Key] = kvp.Value;
            }
            return output;
        }

        public override TypeHandler Serialize(Lexicon input)
        {
            var data = new JsonDict();
            var keys = new JsonList();
            var values = new JsonList();
            foreach (var kvp in input.Select(x => new KeyValuePair<object, object>(serializer.Serialize(x.Key), serializer.Serialize(x.Value))))
            {
                if (kvp.Key is string)
                {
                    data[(string)kvp.Key] = kvp.Value;
                }
                else
                {
                    keys.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
            }
            this["data"] = data;
            this["keys"] = keys;
            this["values"] = values;
            if ((BooleanValue)input.GetSuffix("CASESENSITIVE").Value)
            {
                this["sensitive"] = true;
            }
            else
            {
                this.Remove("sensitive");
            }
            return this;
        }

        public override IEnumerable<IEnumerable> GetContainers(IJsonDict source)
        {
            this.Validate(source);
            var result = new List<IEnumerable>(3);
            if (source.ContainsKey("data")) result.Add((IJsonDict)source["data"]);
            if (source.ContainsKey("keys")) result.Add((IJsonList)source["keys"]);
            if (source.ContainsKey("values")) result.Add((IJsonList)source["values"]);
            return result;
        }
    }
}

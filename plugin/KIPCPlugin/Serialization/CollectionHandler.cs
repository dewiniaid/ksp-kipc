using System.Collections.Generic;
using System.Collections;
using KIPC.Extensions;

namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;
    using IJsonDict = IDictionary<string, object>;

    public interface ICollectionHandler
    {
        object ObjectId { get; set; }
        IEnumerable<IEnumerable> GetContainers();
        IEnumerable<IEnumerable> GetContainers(IJsonDict source);
    }

    public abstract class CollectionHandler<T> : TypeHandler<T>, ICollectionHandler
    {

        public object ObjectId
        {
            get { return this.GetValueOrDefault("ref"); }
            set { this["ref"] = value; }
        }

        protected CollectionHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        /// <summary>
        /// Returns any lists that may contain nested objects in the serialized data.
        /// </summary>
        public virtual IEnumerable<IEnumerable> GetContainers()
        {
            return GetContainers(this);
        }

        public abstract IEnumerable<IEnumerable> GetContainers(IJsonDict source);
    }
}
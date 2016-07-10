using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using System.Reflection;

namespace KIPC.Serialization
{
    using JsonList = List<object>;
    using JsonDict = Dictionary<string, object>;
    using JsonKey = KeyValuePair<string, object>;
    using IJsonList = IList;  // <object>;
    using IJsonDict = IDictionary<string, object>;


    // Lists, stacks, and queues are mostly the same.
    public abstract class BaseListHandler<TCollection, TItem> : CollectionHandler<TCollection>
        where TCollection : IEnumerable<TItem>
    {
        static ConstructorInfo factory = typeof(TCollection).GetConstructor(new Type[0]);
        ConstructorInfo Factory { get { return BaseListHandler<TCollection, TItem>.factory; } }

        public BaseListHandler(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        public override IEnumerable<IEnumerable> GetContainers(IJsonDict source)
        {
            EnsureValueIsType<IJsonList>(source);
            return new List<IEnumerable>{ (IJsonList)source["data"] };
        }

        public override TypeHandler Serialize(TCollection input)
        {
            this["data"] = input.Select(v => serializer.Serialize(v)).ToList<object>();
            return this;
        }
         
        protected abstract void AddItems(TCollection result, IEnumerable<TItem> items);

        public override TCollection Deserialize(IJsonDict source)
        {
            EnsureValueIsType<IJsonList>(source);
            TCollection result = (TCollection) Factory.Invoke(new object[0]);
            serializer.deserializerState.results[source] = result;

            AddItems(
                result, 
                ((IJsonList)source["data"]).Cast<object>().Select(element => (TItem)(serializer.Deserialize(element)))
            );
            return result;
        }
    }

    [Serializes(typeof(ListValue), "list")]
    public class SerializedList : BaseListHandler<ListValue, Structure>
    {
        public SerializedList(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        protected override void AddItems(ListValue result, IEnumerable<Structure> items)
        {
            foreach(var item in items) { result.Add(item); }
        }
    }

    [Serializes(typeof(QueueValue), "queue")]
    public class SerializedQueue : BaseListHandler<QueueValue, Structure>
    {
        public SerializedQueue(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        protected override void AddItems(QueueValue result, IEnumerable<Structure> items)
        {
            foreach(var item in items) { result.Push(item); }
        }
    }

    [Serializes(typeof(StackValue), "stack")]
    public class SerializedStack : BaseListHandler<StackValue, Structure>
    {
        public SerializedStack(TypeSerializer info, Serializer serializer) : base(info, serializer) { }

        protected override void AddItems(StackValue result, IEnumerable<Structure> items)
        {
            foreach (var item in items) { result.Push(item); }
        }
    }
}
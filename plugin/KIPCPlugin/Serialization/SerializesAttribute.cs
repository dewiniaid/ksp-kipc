using System;

namespace KIPC.Serialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SerializesAttribute : Attribute
    {
        /// <summary>
        /// The Structure subclass that this serializer handles.
        /// </summary>
        public Type NativeType { get; private set; }

        /// <summary>
        /// The structure's identifier in data files.  Must be unique.
        /// </summary>
        public object Identifier { get; private set; }

        /// <summary>
        /// The structure's friendly name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// In the event of multiple types matching a particular input, the lowest priority will take precedence.
        /// </summary>
        public int Priority { get; private set; } = 0;

        /// <summary>
        /// Create a new SimpleTypeSerializerAttribute
        /// </summary>
        /// <param name="nativeType">Native type that this serialized from/deserializes to</param>
        /// <param name="name">Friendly type name</param>
        /// <param name="priority">Priority in case multiple serializers match a given input.</param>
        public SerializesAttribute(Type nativeType, string name, int priority = 0) : this(nativeType, name, name, priority) { }
        public SerializesAttribute(Type nativeType, object identifier, string name, int priority = 0) : base()
        {
            this.NativeType = nativeType;
            this.Identifier = identifier;
            this.Name = name;
            this.Priority = priority;
        }
        public SerializesAttribute() : base() { }
    }
}

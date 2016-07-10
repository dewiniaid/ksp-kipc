using System;

using UnityEngine;

namespace KIPC.Serialization
{
    public class TypeSerializer: IComparable
    {
        /// <summary>
        /// Underlying Attribute that we were created off of.
        /// </summary>
        public SerializesAttribute Attribute { get; private set; }

        /// <summary>
        /// The Structure subclass that this serializer handles.
        /// </summary>
        public Type NativeType { get; private set; }

        /// <summary>
        /// The structure's identifier in data files.  Must be unique.
        /// </summary>
        public object Identifier { get; set; }

        /// <summary>
        /// The structure's friendly name.
        /// </summary>
        public string Name { get { return Attribute.Name; } }

        /// <summary>
        /// In the event of multiple types matching a particular input, the lowest priority will take precedence.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// The Type of the class that handles actual serialization.
        /// </summary>
        public Type HandlerType { get; private set; }

        /// <summary>
        /// Factory method that actually creates an instance of HandlerType.
        /// </summary>
        public System.Reflection.ConstructorInfo Factory { get; private set; }

        public System.Reflection.MethodInfo SerializeMethod { get; private set; }
        public System.Reflection.MethodInfo DeserializeMethod { get; private set; }

        /// <summary>
        /// Signature for factory method (usually a constructor)
        /// </summary>
        static private Type[] factorySignature = { typeof(TypeSerializer), typeof(Serializer) };

        public TypeSerializer(SerializesAttribute attribute, Type handlerType)
        {
            if (attribute == null) throw new ArgumentNullException("attribute");
            if (handlerType == null) throw new ArgumentNullException("handler");
            Type[] typeArgs = { attribute.NativeType };

            // Handle generics
            if (handlerType.IsGenericTypeDefinition)
            {
                handlerType = handlerType.MakeGenericType(typeArgs);
            }

            if(!typeof(TypeHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException("Specified type is not a TypeHandler", "handler");
            }
            var factory = handlerType.GetConstructor(factorySignature);
            if(factory == null) {
                throw new ArgumentException("Specified handler type lacks a public constructor matching the required signature.", "handler");
            }
            var serializeMethod = handlerType.GetMethod("Serialize", typeArgs);
            if (serializeMethod == null)
            {
                throw new ArgumentException("Specified handler type lacks an appropriate serializer method", "handler");
            }
            var deserializeMethod = handlerType.GetMethod("Deserialize");
            if (deserializeMethod == null)
            {
                throw new ArgumentException("Specified handler type lacks an appropriate deserializer method", "handler");
            }

            // It's terribly written code sir, but it checks out.
            Attribute = attribute;
            NativeType = attribute.NativeType;
            Identifier = attribute.Identifier;
            Priority = attribute.Priority;
            HandlerType = handlerType;
            Factory = factory;
            SerializeMethod = serializeMethod;
            DeserializeMethod = deserializeMethod;
        }

        public TypeHandler CreateHandler(Serializer serializer)
        {
            object[] args = { this, serializer };
            TypeHandler result = (TypeHandler)Factory.Invoke(args);
            result.DeserializeMethod = DeserializeMethod;
            result.SerializeMethod = SerializeMethod;
            return result;
        }
        public int CompareTo(object obj)
        {
            var other = obj as TypeSerializer;
            if (other == null)
            {
                //Debug.LogWarning("[KIPCPlugin] TypeSerializer: Attempted comparison against NULL!");
                throw new ArgumentNullException();
            }
            //Debug.Log(String.Format("Our priority is: {0}.  Other priority is {1}", Priority, other.Priority))
            int result = Priority.CompareTo(other.Priority);
            if (result == 0) return Name.CompareTo(other.Name);
            return result;
        }
    }
}

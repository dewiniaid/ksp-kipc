using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KRPC.Service.Attributes;
using KRPC.Utils;
using kOS.Module;
using kOS.Safe.Encapsulation;
using kOS.Safe.Module;
using global::KRPC.SpaceCenter;


namespace KIPC.KRPC
{
    using Part = global::KRPC.SpaceCenter.Services.Parts.Part;

    /// <summary>
    /// Used to properly deserialize messages.
    /// 
    /// To play with kOS's message API, we unfortunately have to deserialize message data, and then send it to Message.Create() which serializes it AGAIN only for kOS to deserialize it.
    /// Hopefully we'll be able to fix that at some point.
    /// </summary>
    public class JsonMessageProxy : kOS.Communication.InterProcCommand
    {
        string json;
        public JsonMessageProxy(string json)
        {
            this.json = json;
        }

        public override void Execute(kOS.SharedObjects shared)
        {
            kOSProcessor processor = shared.Processor as kOSProcessor;
            if(processor == null) { throw new ArgumentException("Processor is not a kOSProcessor"); }
            processor.Send((Structure) KIPC.Serialization.Serializer.ReadJson(shared, json));
        }
    }

    /// <summary>
    /// A kOSProcessor.
    /// </summary>
    [KRPCClass(Service = Service.SERVICE_NAME)]
    public class Processor : Equatable<Processor>
    {
        readonly kOSProcessor processor;

        internal static bool Is(Part part)
        {
            return part.InternalPart.Modules.OfType<kOSProcessor>().Any();
        }

        internal Processor(Part part)
        {
            Part = part;
            processor = part.InternalPart.Modules.OfType<kOSProcessor>().FirstOrDefault();  // We expect this to always
            if (processor == null) throw new ArgumentException("Part is not a kOS Processor");
        }

        internal Processor(kOSProcessor processor)
        {
            this.processor = processor;
            Part = new Part(this.processor.part);
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        public override bool Equals(Processor other)
        {
            return !ReferenceEquals(other, null) && Part == other.Part && processor == other.processor;
        }

        /// <summary>
        /// Hash code for the object.
        /// </summary>
        public override int GetHashCode()
        {
            return Part.GetHashCode() ^ processor.GetHashCode();
        }

        /// <summary>
        /// The part object for this kOSProcessor
        /// </summary>
        [KRPCProperty]
        public Part Part { get; private set; }

        /// <summary>
        /// Total disk space
        /// </summary>
        [KRPCProperty]
        public int DiskSpace { get { return processor.diskSpace; } }

        /// <summary>
        /// Returns or sets whether the processor is currently turned on.  Note that power-starved still counts as turned on.
        /// Can be set to change the processor's power state.
        /// </summary>
        [KRPCProperty]
        public bool Powered
        {
            get { return processor.ProcessorMode != ProcessorModes.OFF; }
            set { if(value != Powered) { processor.TogglePower(); } } 
        }

        /// <summary>
        /// Returns or sets whether the terminal is currently turned visible.
        /// </summary>
        [KRPCProperty]
        public bool TerminalVisible
        {
            get { return processor.WindowIsOpen(); }
            set { if (value) processor.OpenWindow(); else processor.CloseWindow(); }
        }

        /// <summary>
        /// Sends a message to the specified kOS Processor which it can receive using kOS's inter-processor communication system.
        /// </summary>
        /// <param name="json">JSON message content formatted according to our allowing format.</param>
        /// <returns>Whether the message was successfully sent.</returns>
        [KRPCMethod]
        public bool SendMessage(string json)
        {
            processor.ExecuteInterProcCommand(new JsonMessageProxy(json));
            return true;
        }
    }
}
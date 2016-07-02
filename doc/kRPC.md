# Using KIPC with KRPC.

## KIPC Service
Handles general functionality.

### GetProcessor(*part*)
Returns a `KOSProcessor` if the part is a kOSProcessor, otherwise `None`.  Equivalent to the various analogues on 
`SpaceCenter.Part`.

### GetProcessors(*vessel*)
Returns all `KOSProcessors` on the specified `vessel`.

### GetPartsTagged(*vessel*, *tag*)
Returns a list of all parts tagged with the specified kOS `tag` on the specified `vessel`.

### CreateMessage(*collectiontype* = CollectionType.LIST)
Creates a new `Message`, with an initially empty collection of the specified `collectiontype`.

### CreateCollection(*collectiontype*)
Creates a collection of the particular *collectiontype*.

## Collection class.
kRPC does not have an optimal way to represent a list of items when the individual items may have varying types.  The
`Collection` class serves as a workaround for this mechanism.

`Collection`s function as a sort of list, but this mechanism serves as an abtract representation of other data 
structures.  You generally don't want to use a `Collection` anything beyond the initial load or retrival of values from
it -- convert it to something appropriate based on its type and values instead.

Collections only persist as long as their underlying container is around.  This container is usually a `Message`, so
it is important to read all of the data off of a message before acknowledging it.

### Collection.Type
Returns a description of the underlying data type that this collection is intended to function as. 

| Value                     | Meaning
| ------------------------- | -------
| CollectionType.List       | Function like a `List`.  Items are appended to the list in order.
| CollectionType.Queue      | Function like a `Queue`.  Items are added to the queue in order.
| CollectionType.Stack      | Function like a `Stack`.  The top of the stack is the first item.
| CollectionType.Dictionary | Function like a dictionary (`Lexicon` in kOS).  Items alternate between dictionary keys and their values.  If this contains an odd number of items, the final item is discarded silently.

### Collection.ItemCount
Represents the number of items in the collection.

### Collection.ItemTypes
Represents a list of type names that describe the argument type of each parameter in the message.

### Collection.ID
Returns the internal identifier of this collection.  Can be used to detect recursion or circular structures.

### Collection.Get*{type}*(*index*=-1)
Retrieves a item of the requested type, or the type's version of `NULL` / `0` / `""` if the conversion was
not feasible.  Using an index of -1 is equivalent to "1 + the last index accessed", or 0 if that has never been accessed.

### Collection.Add*{type}(*value*)
Adds a parameter of the requested type to the end of the collection.

### Supported Item Types
Here's information on the types that KIPC supports.

| Typename   | kOS Equivalent | Notes
| ---------- | -------------- | -----
| Integer    | ScalarValue    | kOS `ScalarValues` can be either integers or doubles.
| Float      | ScalarValue    | Is actually a `Double`.
| String     | String         | |
| Boolean    | Boolean        | |
| Vessel     | Vessel         | |
| Body       | Body           | |
| Vector     | Vector         | KRPC's `Vector3` type
| Quaternion | Quaternion     | KRPC's `Quaternion` type
| Collection | Lexicon, List, Queue or Stack | Determined by `Collection.Type`  

Note that `Direction` is not yet supported.

### Collection.CaseSensitive=*false*
If this collection has keys (e.g. a dictionary or set), whether they should be treated as case-sensitive.

## Message class

### Message.Data
Returns a `Collection` corresponding to the data of this message.

### Message.ID
Returns an ID representing the message.  

### Message.SenderCPU
Returns the kOSProcessor that sent this message, or `NULL` if the message wasw not sent by a kOSProcessor.

### Message.SenderName
Returns the kRPC Client Name that sent this message, or `NULL` if the message wasw not sent by a kRPC Client.

## KOSProcessor class

### KOSProcessor.HasMessage()
Returns `True` if the kOSProcessor has any current outbound message(s). 

### KOSProcessor.GetMessage()
Returns the kOSProcessor's current outbound message, if any.  The messages are not acknowledged. 

### KOSProcessor.AckMessage(*messageid*=0, *message*=None, *sendercpu*=None, *sendername*=None)
Acknowledges the message from the `KOSProcessor` if any of the attributes match.  If all of the parameters are 
defaulted, the message is acknowledged regardless of attributes.

### KOSProcessor.CanSendMessage
Returns `True` if the kOSProcessor is able to receive a message.  Will be `False` if it currently has any messages
pending.

 **Note**: There is no guarantee that this state will not change between examining it and actually attempting to send 
  a message.

### KOSProcessor.SendMessage(message)
Sets the `KOSProcessor`'s inbound message to this message.  Returns `True` if successful, `False` otherwise.

### KOSProcessor.CancelMessage(*messageid*=0, *message*=None, *sendercpu*=None, *sendername*=None)
### KOSProcessor.CancelMessage(*messageid*=0, *message*=None)
Cancels the `KOSProcessor`'s inbound message if any of the attributes match.  If all of the parameters are 
defaulted, the message is cancelled regardless of attributes.


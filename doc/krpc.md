# Using KIPC with KRPC.

**[ [KIPC Overview](index.md) ] [ [kOS API](kos.md) ] [ [kRPC API](krpc.md) ] [ [Changelog](CHANGELOG.md) ] [ [License](LICENSE.md) ]** 

## KIPC Service
Handles general functionality.

**GetProcessors(_vessel_)**

Returns all `KOSProcessors` on the specified `vessel`.

**GetProcessor(_part_)** _(Added in 0.1.1)_

Returns all `KOSProcessors` on the specified `part`.  Generally this will be an empty list or a list with one element.

**ResolveVessel(_guid_)**

Given a vessel GUID, returns the vessel.  Use this to handle vessel references in JSON results.

**ResolveVessels(_guids_)** _(Added in 0.1.1)_

Given a list of vessel GUIDs, resolves all of them.  Results are in the same order as the input.

**ResolveBody(_id_)**

Given a body ID, returns the celestial body.  Use this to handle body references in JSON results.

**ResolveBodies(_ids_)** _(Added in 0.1.1)_

Given a list of body IDs, returns the corresponding celestial bodies.  Results are in the same order as the inputs.

**PopMessage()**

Removes and returns the next message in the queue, or an exception if no message exists.
See the JSON Format section for details on how to parse these messages.

**PeekMessage()**

Returns the next message in the queue, or an exception if no message exists.  The message is not removed.  
See the JSON Format section for details on how to parse these messages.

**GetMessages()** _(Added in 0.1.1)_

Returns _all_ messages currently in the queue and empties the queue.  Returns an empty list if the queue is empty.
See the JSON Format section for details on how to parse these messages.

**_property_ CountMessages** (get) _(Added in 0.1.1)_

Returns the number of messages in the queue.

**GetPartsTagged(_vessel_, _tag_)** _(Added in 0.1.1)_

Returns a list of all parts tagged with the specified kOS `tag` on the specified `vessel`.


## Processor class

(May change names in a later build)

**_property_ Part** (get)

Returns the parent part of this `kOSProcessor`.

**_property_ DiskSpace** (get)

Returns the total disk space of this processor.

**_property_ Powered** (get, set)

Returns or sets whether the processor is currently turned on.  Note that power-starved still counts as turned on.

**_property_ TerminalVisible** (get, set)

Returns or sets whether the terminal is currently visible.

**SendMessage(_json_)**

Sends the particular JSON-encoded message to the processor's message queue.

## JSON format

Strings, integers, double/floats, booleans, and `null` are represented using their native JSON representation.

All other types (including lists and dictionaries) are in an encapsulated format

### Dictionary/Lexicon
```
{
  "type": "dict",
  "data": {"string key": 42, ...},
  "keys", [1, 2, 3, 4],
  "values", ["One", "Two", "Three", "Four"]
}
```
JSON objects cannot have non-string keys, but kOS Lexicons can.  All string keys will be contained in `data`.

Non-string keys will be listed in `keys`, with their corresponding values in `values`.  (In Python, you can construct
that portion of the corresponding dictionary using `dict(zip(keys, values))`) 

### Lists, Stacks, and Queues
```
{
  "type": "list",  // or "stack" or "queue"
  "data": [1, 2, 3, 4]
}
```
For all three types, data elements are in insertion order: the oldest item on the queue is listed first, and the 
bottom-most element of a stack is listed queue.

### References

KIPC can handle recursive data structures: e.g. a list that contains itself, or a dict that is referenced multiple 
times within a complicated structure.

If this occurs, all references to the same object will have a `ref` field with a common value.  One of the objects will
be its normal representation, others will look like this:
```
{
  "type": "ref",
  "ref": 4,
}
```

For instance, a list that contains another list that contains the first may look like:
```
{
  "type": "list",
  "ref": 1,
  "data": [{
    "type": "list",
    "data" [{
      "type": "ref",
      "ref", 1
    }]
  }]
}
```

Only collection types (the four above) support references within the deserializer.

### Vectors
```
{
  "type": "vector",
  "data": [x, y, z]
}
```

### Vessels
```
{
  "type": "vessel",
  "data": "vessel-guid"
}
```
Use `KIPC.ResolveVessel` to convert a vessel reference to an actual kRPC-usable `Vessel`.

### Celestial Bodies
```
{
  "type": "body",
  "data": 1,  // flightGlobals index
}
```
Use `KIPC.ResolveBody` to convert a body reference to an actual kRPC-usable `CelestialBody`.

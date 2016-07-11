# Using KIPC within kOS

## Supported data types
All currently serializable data types should be supported; these are the same restrictions as what you can use when
sending messages between processors or vessels.

These types include simple types like `ScalarValue`, `Boolean` and `String`, most of the collection types 
(`Lexicon`, `List`, `Queue`, `Stack`), and some object references (`Vessel` and `Body`).  `Vector` is currently
supported, but `Direction` is not available in this build due to limitations within kOS.

## Sending Messages

**ADDON:KIPC:CONNECTION**
Returns a `KRPCConnection` that can be used to send messages to kRPC.  This functions like a normal `Connection`
within processors on the same vessel.

## Unstable/Developer API
These are all mostly intended for the developer build; they may change or be removed at any time.

**ADDON:KIPC:SERIALIZE(_content_)**

Serializes _content_ and returns the result.

**ADDON:KIPC:DESERIALIZE(_json_)**

Deserializes _json_ and returns the result.

**ADDON:KIPC:SEND(_vessel_, _content_)**
Immediately sends a message to the target vessel, ignoring RemoteTech restrictions (if any).

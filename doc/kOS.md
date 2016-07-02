# Using KIPC within kOS

## Supported data types
KIPC message values must be one of the following types: `Lexicon`, `List`, `Queue`, or `Stack`.

Values (and keys, if relevant) within the collection may be any of the aforementioned types or one of `ScalarValue`,
`String`, `Boolean`, `Vessel`, `Body`, `Vector` or `Quaternion`.

Note that `Direction` is not currently supported.

## Messages

### ADDON:KIPC:CanEncode(*value*)
Returns `True` if the specified value can safely be encoded into a `Message`.

### ADDON:KIPC:Message(*value*)
Creates a new `Message` consisting of the specified `value`, which must be a `List`, `Lexicon`, `Queue` or `Stack`.

## Message structure

### MESSAGE:DATA
Returns the message data (a `List` of one or more values)

### MESSAGE:ID
Returns an arbitrary identifier for this message.  Will be 0 if the message has not been sent.

### MESSAGE:SENDER
Returns the sender of a message.  For messages sent from kOS, this will be a `kOSProcessor`.  For messages sent from
KRPC, this will be a `String` consisting of the client name specified during the connection.

## Sending messages from the current `kOSProcessor`

### ADDON:KIPC:CANSEND
`True` if the current `kOSProcessor` does not already have an outbound message, `False` otherwise.

### ADDON:KIPC:SEND(*message*)
"Sends" `message` by assigning it to the current `kOSProcessor`s outbound message.  Returns `True` if the message
was sent, `False` otherwise (probably due to an existing outbound message existing).

### ADDON:KIPC:CANCEL()
Cancels the current outbound message, if any.  This is the same action as acknowledging it.

Returns `True` if such a message actually existed, `False` otherwise.

### ADDON:KIPC:LASTACK
Returns the message ID of the last acknowledged message, or 0 if no messages have been acknowledged.  This is 
primarily useful if you are implementing a message queue, because you can do:

```
LOCAL messages IS QUEUE().
// ... etc
ON ADDON:KIPC:LASTACK {  // will trigger every time LASTACK changes.
    IF messages:length() > 0 {
        ADDON:KIPC:SEND(messages:pop())
    }
    PRESERVE.
}
```

## Receiving messages sent to the current `kOSProcessor`

### ADDON:KIPC:HASMESSAGE
`True` if the current `kOSProcessor` has an inbound message waiting, `False` otherwise.

### ADDON:KIPC:RECEIVE()
Acknowledges and returns a `List` of current inbound messages.  The list will contain up to one `Message`.

## Sending messages to another `kOSProcessor`

### ADDON:KIPC:CANSENDTO(*kOSProcessor*)
Returns `True` if the target `kOSProcessor` has no other inbound messages waiting.
 
* **Note**: There is no guarantee that the target `kOSProcessor` will still have no inbound messages waiting when you 
  attempt to send to it, since it is possible that another processor (or kRPC) may have sent a message to it in the 
  meantime.  It is generally better to just use `ADDON:KIPC:SENDTO` and use the return value to determine if you were 
  successful. 

### ADDON:KIPC:SENDTO(*kOSProcessor*, *message*)

"Sends" the message by assigning it to the target `kOSProcessor`s inbound message.  Returns `True` if the message
could be sent, `False` if it could not be due to an inbound message already being assigned.

### ADDON:KIPC:CANCELSENDTO(*kOSProcessor*)

Removes the target `kOSProcessor`'s inbound message.  Returns `True` if such a message actually existed, `False` 
otherwise.

* **Note**: There is no guarantee as to *which* message is ultimately cancelled, or even if you're the sender.

### ADDON:KIPC:CANCELIFSENTBY(*kOSProcessor*, *sender*)
Removes the target kOSProcessor's inbound message if and only if it was sent by the specified `sender`, which 
corresponds to `MESSAGE:SENDER`.  Returns `True` if such a message actually existed, `False` otherwise.

```
 // Cancel any message waiting on the target if we were the sender.
 ADDON:KIPC:CANCELIFSENTBY(target_cpu, CORE).
 ```

### ADDON:KIPC:CANCELIFMATCH(*kOSProcessor*, *messageid*)
Removes the target kOSProcessor's inbound message if and only if it matches the specified `messageid`.  Returns `True`
if such a message actually existed, `False` otherwise.

## Receiving messages from another `kOSProcessor`

### ADDON:KIPC:CANRECEIVEFROM(*kOSProcessor*)

Returns `True` if the target `kOSProcessor` has an outbound message waiting.
 
**Note**: There is no guarantee that the `kOSProcessor` will still have an outbound messages waiting when you attempt to
receive it, since it is possible that another processor (or kRPC) may have received it or cancelled it in the meantime.
It is generally better to just use `ADDON:KIPC:RECEIVEFROM` and use the return value to determine if you were
successful.

### ADDON:KIPC:PEEKAT(*kOSProcessor*)
Returns a list of the target kOSProcessor's outbound messages without acknowledging them.  The list will contain up to 
one instance of `ADDON:KIPC:Message`.  The message, if any, will not be acknowledged.

You can use this to examine the message and see if it is intended for you without consuming it if that is not the case.

### ADDON:KIPC:ACKFROM(*kOSProcessor*)
Acknowledges the outbound message(s) from the target kOSProcessor.

### ADDON:KIPC:ACKNOWLEDGEFROM(*kOSProcessor*)
Acknowledges the outbound message(s) from the target kOSProcessor.

### ADDON:KIPC:ACKIFMATCH(*kOSProcessor*, *messageid*)
Acknowledges the outbound message(s) from the target kOSProcessor if they match the specified ID.

### ADDON:KIPC:RECEIVEFROM(*kOSProcessor*)
Returns a list of the target `kOSProcessor`'s outbound messages and acknowledges them.  The list will contain up to one
instance of `ADDON:KIPC:Message`.

Equivalent to combining `PEEKAT` and `ACKFROM`, but guarunteed to be atomic. 

# KIPC Overview

## About

KIPC (Kerbal Inter-Process Communication) provides a bridge between kOS and KRPC.  Specifically, it adds a primitive 
messaging system that allows Kerboscript code (running in kOS) to communicate with outside programs using kRPC.

## Why not just use kOS?

kOS is poorly suited to certain tasks, including complex calculations.  The particular incentive to write this mod came
about after attempting to make a kOS version of [AlexMoon's Launch Window Planner](http://alexmoon.github.io/ksp/) and

Additionally, someone may already have a large kRPC-using project -- or sufficient experience with mainstream
programming languages -- to prefer kRPC.

## Why not just use kRPC?

kRPC introduces a small amount of latency in fine control of a craft, since messages go over a TCP/IP communication
channel and KSP may have additional physics frames before a message to cut throttle is received.

Additionally, someone whose first experience in programming comes from kOS may not want to have to start completely 
from scratch if they're inspired to start learning a mainstream programming language.

## Use Cases

There's two main scenarios where I can see the bridge being useful, though others likely exist.

 * **kOS As Mission Control**: kOS handles the overall control of a particular vessel, with requests sent to kRPC for
   complex tasks like maneuver planning.
 * **kRPC as Mission Control**: kRPC handles the overall control of a particular vessel, with requests sent to kOS for
   real-time tasks like executing a planned maneuver.
   
## Architecture

Each kOSProcessor can have one outbound message and one inbound message.  There is no message queueing capability 
directly implemented in KIPC, so any sort of queueing will need to be implemented in kOS or kRPC.

A message is a structure that contains information about its sender, a unique iD, and its data.  The data must be one 
of the values that KIPC supports, or -- more likely -- a list of said values.

Once a processor has an inbound or outbound message, that message may only be cleared out (to allow another message in
its place) by acknowledging it.  In most cases, acknowledging messages is automatic on the kOS side -- but it is manual
on the kRPC side.  "Cancelling" a message is a form of acknowledgement.

## A Note On Atomicity

When handling concurrency -- such as two kOSProcessors executing code (which isn't truly concurrent) or kOS + any 
number of kRPC processes examining a data -- there is a risk that state may change at unexpected times.  For instance,
you might check to see if a particular condition is true and start executing code after being told it is only to have
that code fail due to the condition no longer being true.

In KIPC, this can happen in a number of cases.  A notable example is that you might check to see a `kOSProcessor` is
ready to receive a message, and between that and actually sending the message another message might arrive.

All of KIPC's methods and suffixes are *atomic*, which means that the state that a single method call sees is 
guaranteed not to change in the middle of its execution.  Most of them are also designed to be able to gracefully 
handle the case of said state unexpectedly changing -- this is why various kOS suffixes for receiving messages will
return a list containing either 0 or 1 messages rather than simply failing if no message exists.

This is why the "cancel if..." functions exist: While you could examine the data and make a determination whether to
cancel or not in your own code, using the "cancel if" functions ensures that the state won't change between the "if" 
and the "cancel"

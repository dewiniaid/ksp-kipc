# KIPC Overview

**[ [KIPC Overview](index.md) ] [ [kOS API](kos.md) ] [ [kRPC API](krpc.md) ] [ [Changelog](CHANGELOG.md) ] [ [License](LICENSE.md) ]** 

## About

KIPC (Kerbal Inter-Process Communication) provides a bridge between kOS and KRPC.  Specifically, it adds the ability
to send messages from kOS to KIPC using a mechanism simpilar to the communications mechanisms added in kOS 1.0.0

## Why not just use kOS?

kOS is poorly suited to certain tasks, including complex calculations.  It also lacks certain visualization
capabilities that can be tapped into using kIPC.  The particular incentive to write this mod came about after
attempting to make a kOS version of [AlexMoon's Launch Window Planner](http://alexmoon.github.io/ksp/) and
encountering unreasonably long delays while attempting to compute something similar to a porkchop plot.

Additionally, someone may already have a large kRPC-using project -- or sufficient experience with mainstream
programming languages -- to prefer kRPC.

## Why not just use kRPC?

kRPC introduces a small amount of latency in fine control of a craft, since messages go over a TCP/IP communication
channel and KSP may have additional physics frames before a message to cut throttle is received.

Additionally, someone whose first experience in programming comes from kOS may not want to have to start completely 
from scratch if they're inspired to start learning a mainstream programming language.

## Use Cases

Here's a few scenarios where the combination of kRPC and kOS can be useful.  You can probably think of some more.

 * **kOS As Mission Control**: kOS handles the overall control of a particular vessel, with requests sent to kRPC for
   complex tasks like maneuver planning.
 * **kRPC as Mission Control**: kRPC handles the overall control of a particular vessel, with requests sent to kOS for
   real-time tasks like executing a planned maneuver.
 * **kRPC for user input**: Display a rudimentary user interface in a kRPC client which then relays interface 
   selections back to kOS.
 * **kRPC for data visualization**: Let kOS fly while kRPC renders pretty graphs in realtime.
     
## Architecture

From kOS's perspective, `addons:kipc:connection` functions similar to a connection another `kOSProcessor` on the 
same vessel.  Messages can be sent to that connection.

On the kRPC side, KIPC adds a `KIPC` service which provides mechanisms to get information about `kOSProcessors`
and send messages to them.  The KIPC service also provides procedures for retrieving the next message in the message
queue and some limited kOS control (like toggling power or visibility of the terminal)

Messages are represented as JSON in KRPC.  Complex types all have a simple format that describes what the type is and
the data it contains -- this includes collections (lists, stacks, queues and lexicons/dicts) as well as references to
vessels, celestial bodies and vectors.

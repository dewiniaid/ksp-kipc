# ksp-kipc
kIPC - Inter-Process(or) Communication for kOS and kRPC in KSP

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

## Documentation

 * [Overview and architecture](/doc/index.md)
 * [kOS API](/doc/kos.md)
 * [KRPC API](/doc/krpc.md)
 * [License](/doc/LICENSE.md)
 * [All documentation](/doc/)

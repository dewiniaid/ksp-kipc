# Changelog

**[ [KIPC Overview](index.md) ] [ [kOS API](kos.md) ] [ [kRPC API](krpc.md) ] [ [Changelog](CHANGELOG.md) ] [ [License](LICENSE.md) ]** 

## 0.2.0-beta (2016-07-16)

- Added client libraries for C#, C++ and Java.
- Added preliminary support for CKAN and KSP-AVC.  This release *does not* include MiniAVC and *does not* use the internet to check for version updates, but other providers of MiniAVC/KSP-AVC might.

## 0.1.1-dev2 (2016-07-12)

- Added `KIPC.GetMessages` and `KIPC.CountMessages` for better control and information about the message queue.
- Added `KIPC.ResolveBodies` and `KIPC.ResolveVessels` to handle multiple bodies/vessels at once.
- Added `KIPC.GetProcessor` to retrieve the kOSProcessor of a single part (compare to `KIPC.GetProcessors` which receives all processors on a given vesse)
- Added `KIPC.GetPartsTagged` to find parts with a given `kOSNameTag`.

## 0.1.0-dev1 (2016-07-10)

- First development release.

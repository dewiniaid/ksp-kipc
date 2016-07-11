using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("KIPC")]
[assembly: AssemblyDescription("KSP IPC plugin - bridge between kOS and kRPC")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Next Phase Technologies")]
[assembly: AssemblyProduct("KIPC")]
[assembly: AssemblyCopyright("Copyright © 2016 Daniel J Grace a.k.a. dewin <thisgenericname@gmail.com> - Licensed GPLv3")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("febe111a-d442-48bf-90b3-9135b577a4f6")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

[assembly: KSPAssembly("KIPC", 0, 1)]
[assembly: KSPAssemblyDependency("kOS", 0, 0)]
[assembly: KSPAssemblyDependency("KRPC", 0, 0)]
[assembly: KSPAssemblyDependency("KRPC.SpaceCenter", 0, 0)]

// This is the default value for ck.stamp.fody, but explicitly defining it means that our own code won't break if
// ck.stamp.fody isn't present.
[assembly: AssemblyInformationalVersion("%ck-standard%")]  

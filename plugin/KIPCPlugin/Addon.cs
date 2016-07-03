using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KSP;
using UnityEngine;

namespace KIPC
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Addon : MonoBehaviour
    {
        public readonly static Boolean hasKOS = true;
        public static Boolean hasKRPC { get; private set; }

        private Boolean LogOwnPlugin(string name, Assembly assembly)
        {
            if (assembly == null)
            {
                Debug.LogFormat("[KIPCPlugin] {0}: not available", name);
                return false;
            }
            var att = (AssemblyInformationalVersionAttribute) assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault();
            if (att == null)
            {
                Debug.LogWarningFormat("[KIPCPlugin] {0}: lacks expected version information", name);
                return LogOtherPlugin(name, assembly);
            }
            Debug.LogFormat("[KIPCPlugin] {0}: {1}", name, att.InformationalVersion);
            return true;
        }

        private Boolean LogOtherPlugin(string name, Assembly assembly)
        {
            if (assembly == null)
            {
                Debug.LogFormat("[KIPCPlugin] {0}: not detected", name);
                return false;
            }
            Debug.LogFormat("[KIPCPlugin] {0}: {1}", name, assembly.GetName().Version.ToString());
            return true;
        }

        void Start() 
        {
            Debug.Log("[KIPCPlugin] Hello, Solar System!");
            var findAssembly = new Func<string, Assembly>(x => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == x));
            // Some informational output
            LogOwnPlugin("Base plugin", typeof(Addon).Assembly);  // Our own version information
            LogOtherPlugin("kOS", findAssembly("kOS"));
            hasKRPC = LogOwnPlugin("KRPC support", findAssembly("KIPCPlugin-KRPC"));
            // Order of && matters, because we don't want to short-circuit the LogOtherPlugin calls.
            hasKRPC = LogOtherPlugin("KRPC", findAssembly("KRPC")) && hasKRPC;
            hasKRPC = LogOtherPlugin("KRPC.SpaceCenter", findAssembly("KRPC")) && hasKRPC;
        }
    }
}

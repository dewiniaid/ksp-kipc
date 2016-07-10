using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KSP;
using UnityEngine;

using System.IO;
using JsonFx;

namespace KIPC
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Addon : MonoBehaviour
    {
        void Start() 
        {
            Debug.Log("[KIPCPlugin] Hello, Solar System!");
        }
    }
}

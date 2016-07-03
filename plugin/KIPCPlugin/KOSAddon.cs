using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.AddOns;

using UnityEngine;

namespace KIPCPlugin.KOS
{
    [kOSAddon("KIPC")]
    [kOS.Safe.Utilities.KOSNomenclature("KIPC")]
    public class Addon : kOS.Suffixed.Addon
    {
        public static int magic_number = 0;
        public Addon(kOS.SharedObjects shared) : base(shared)
        {
            InitializeSuffixes();
        }

        public void InitializeSuffixes()
        {

        }

        public override BooleanValue Available()
        {
            return true;
        }
    }

    /// <summary>
    /// Exists solely to provide a way for kOS scripts to determine if KRPC is present.
    /// Registers as a dummy addon.  The KRPC extension will set this to true
    /// </summary>
    [kOSAddon("KRPC")]
    [kOS.Safe.Utilities.KOSNomenclature("KRPC")]
    public class KRPCAvailabilityIndicator : kOS.Suffixed.Addon
    {
        public KRPCAvailabilityIndicator(kOS.SharedObjects shared) : base(shared) { }

        protected static bool hasKRPC = false;

        public override BooleanValue Available()
        {
            return hasKRPC;
        }

    }

}


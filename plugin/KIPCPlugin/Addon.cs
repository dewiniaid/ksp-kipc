using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.AddOns;

namespace kOS.Addons.KIPC
{
    [kOSAddon("KIPC")]
    [kOS.Safe.Utilities.KOSNomenclature("KIPCAddon")]
    public class Addon : Suffixed.Addon
    {
        public Addon(SharedObjects shared) : base (shared)
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
}

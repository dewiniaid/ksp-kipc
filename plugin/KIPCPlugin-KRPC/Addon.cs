using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KRPC.Service;
using KRPC.Service.Attributes;

namespace KIPC.KRPC
{
    [KRPCService(Name ="KIPC", GameScene=GameScene.All)]
    public static class Addon
    {
        [KRPCProperty] public static int TestProperty { get; set; }
    }
}

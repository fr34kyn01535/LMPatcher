using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMPatcher.Patches
{
    [Class("GameManager")]
    public class Test : Patch
    {
        public override void Apply()
        {
            UnlockMethodByName("ClearObservers");
        }
    }
}

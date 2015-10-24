using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// Bos怪
    /// </summary>
    public class FightBos : FightApc
    {
        public override FighterType FType
        {
            get { return FighterType.Boss; }
        }

        public FightBos(int p, Apc apc)
            : base(p, apc)
        {
        }

    }
}

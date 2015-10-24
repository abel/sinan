using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 宝宝怪
    /// </summary>
    public class FightBB : FightBos
    {
        /// <summary>
        /// 捕捉上限
        /// </summary>
        public int MaxCatch
        { get; private set; }

        /// <summary>
        /// 阶
        /// </summary>
        public int Rank
        { get; private set; }

        /// <summary>
        /// 捕捉几率
        /// </summary>
        public double ClapP
        { get; private set; }

        public override FighterType FType
        {
            get { return FighterType.BB; }
        }

        public FightBB(int p, Apc apc)
            : base(p, apc)
        {
            ClapP = apc.Value.GetDoubleOrDefault("ClapP");
            int max = apc.Value.GetIntOrDefault("MaxCatch");
            MaxCatch = max == 0 ? int.MaxValue : max;

            string petid = apc.Value.GetStringOrDefault("PetID");
            GameConfig c = GameConfigAccess.Instance.FindOneById(petid);
            if (c != null) Rank = c.Value.GetIntOrDefault("PetsRank");
        }
    }
}

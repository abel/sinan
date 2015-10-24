using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Data;

namespace Sinan.FrontServer
{
    sealed public class RoleBusiness
    {
        static Dictionary<string, GameConfig> cache = new Dictionary<string, GameConfig>();

        static public GameConfig GetRole(string goodsID)
        {
            GameConfig v;
            if (!cache.TryGetValue(goodsID, out v))
            {
                v = GameConfigAccess.Instance.FindOneById(goodsID);
                if (v != null)
                {
                    cache[goodsID] = v;
                }
            }
            return v;
        }
    }
}

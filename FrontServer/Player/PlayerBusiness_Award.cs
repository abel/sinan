using System;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 奖励
    /// </summary>
    partial class PlayerBusiness
    {
        Dictionary<string, int> m_awardGoods = new Dictionary<string, int>();

        /// <summary>
        /// 奖励物品
        /// </summary>
        public Dictionary<string, int> AwardGoods
        {
            get { return m_awardGoods; }
        }
    }
}

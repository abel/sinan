using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 晶币限制
    /// </summary>
    public class LimitCoin : ILimit
    {
        /// <summary>
        /// 所需石币
        /// </summary>
        readonly int m_coin;

        readonly string m_msg;

        public LimitCoin(string name, int coin)
        {
            m_coin = coin;
            if (m_coin > 0)
            {
                m_msg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype2), name, m_coin);
            }
        }

        public static LimitScore Create(string name, Variant v)
        {
            int coin = v.GetIntOrDefault("Coin");
            if (coin > 0)
            {
                return new LimitScore(name, coin);
            }
            return null;
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            if (player.Coin >= m_coin)
            {
                msg = null;
                return true;
            }
            msg = m_msg;
            return false;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            //消耗晶币
            if (m_coin > 0)
            {
                if (!player.AddCoin(-m_coin, FinanceType.IntoScene))
                {
                    msg = m_msg;
                    return false;
                }
            }
            msg = null;
            return true;
        }

        public bool Rollback(PlayerBusiness player)
        {
            //退回晶币
            if (m_coin > 0)
            {
                player.AddCoin(m_coin, FinanceType.IntoScene);
            }
            return true;
        }
    }
}

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
    /// 点券限制
    /// </summary>
    public class LimitBond : ILimit
    {
        /// <summary>
        /// 所需石币
        /// </summary>
        readonly int m_bond;

        readonly string m_msg;

        public LimitBond(string name, int bond)
        {
            m_bond = bond;
            if (m_bond > 0)
            {
                m_msg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype2), name, m_bond);
            }
        }

        public static LimitScore Create(string name, Variant v)
        {
            int bond = v.GetIntOrDefault("Bond");
            if (bond > 0)
            {
                return new LimitScore(name, bond);
            }
            return null;
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            if (player.Bond >= m_bond)
            {
                msg = null;
                return true;
            }
            msg = m_msg;
            return false;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            //消耗绑金
            if (m_bond > 0)
            {
                if (!player.AddBond(-m_bond, FinanceType.IntoScene))
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
            //退回绑金
            if (m_bond > 0)
            {
                player.AddBond(m_bond, FinanceType.IntoScene);
            }
            return true;
        }
    }
}

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
    /// 石币限制
    /// </summary>
    public class LimitScore : ILimit
    {
        /// <summary>
        /// 所需石币
        /// </summary>
        readonly int m_score;

        readonly string m_msg;

        public LimitScore(string name, int score)
        {
            m_score = score;
            if (m_score > 0)
            {
                m_msg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype3), name, m_score);
            }
        }

        public static LimitScore Create(string name, Variant v)
        {
            int score = v.GetIntOrDefault("Score");
            if (score > 0)
            {
                return new LimitScore(name, score);
            }
            return null;
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            if (player.Score >= m_score)
            {
                msg = null;
                return true;
            }
            msg = m_msg;
            return false;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            //消耗石币
            if (m_score > 0)
            {
                if (!player.AddScore(-m_score, FinanceType.IntoScene))
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
            //退回石币
            if (m_score > 0)
            {
                player.AddScore(m_score, FinanceType.IntoScene);
            }
            return true;
        }
    }
}

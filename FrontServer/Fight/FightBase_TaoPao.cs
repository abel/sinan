using System;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Extensions;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 逃跑
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        /// 逃跑成功
        /// </summary>
        protected bool m_taoPao;

        /// <summary>
        /// 逃跑者
        /// </summary>
        protected FightObject m_fugitive;

        /// <summary>
        /// 逃跑
        /// </summary>
        /// <param name="fighter"></param>
        /// <returns></returns>
        protected virtual void TaoPao(FightObject fighter)
        {
            FightPlayer f2 = fighter as FightPlayer;
            if (f2 != null)
            {
                if (f2.Player.Team != null && f2.Player.TeamJob == TeamJob.Member)
                {
                    return;
                }
            }

            double m = fighter.Life.TaoPaoLv;
            if (fighter.Level <= 10 || NumberRandom.NextDouble() < m)
            {
                m_taoPao = true;
                m_fugitive = fighter;
            }
            fighter.Action.Value = new Variant(2);
            fighter.Action.Value["TaoPao"] = m_taoPao;
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
        }
    }
}

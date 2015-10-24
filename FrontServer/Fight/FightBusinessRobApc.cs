using System.Collections.Generic;
using System.Linq;
using Sinan.Entity;
using Sinan.Extensions;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 夺宝奇兵跟明雷战斗
    /// </summary>
    public class FightBusinessRobApc : FightSceneApc
    {
        RobBusiness m_rob;

        public FightBusinessRobApc(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players,
            string npcID, bool isEctype, SceneApc apc, int subID, RobBusiness rob) :
            base(teamA, teamB, players, npcID, isEctype, apc, subID)
        {
            m_rob = rob;
            //m_rob = PartProxy.TryGetPart(Part.Rob) as RobBusiness;
        }

        /// <summary>
        /// 分配物品
        /// </summary>
        /// <param name="awards"></param>
        protected override void AwardPlayersGoods(Dictionary<string, int> awards)
        {
            foreach (var award in awards)
            {
                for (int i = 0; i < award.Value; i++)
                {
                    int index = NumberRandom.Next(m_teamA.Length);
                    FightPlayer fp = m_teamA[index] as FightPlayer;
                    if (fp != null && fp.IsLive)
                    {
                        PlayerBusiness player = fp.Player;
                        bool add = false;
                        if (m_rob.Elements.Contains(award.Key))
                        {
                            add = player.AddGoodsNobingOne(award.Key, m_rob.EndTime.AddHours(1), GoodsSource.KillApc);
                        }
                        else
                        {
                            add = player.AddGoodsNobingOne(award.Key, GoodsSource.KillApc);
                        }
                        if (add)
                        {
                            player.AwardGoods.SetOrInc(award.Key, 1);
                        }
                    }
                }
            }
        }


        protected override void GameOver()
        {
            try
            {
                //失败
                if (!m_teamB.All(x => (x.Over || x.FType == FighterType.BB)))
                {
                    PlayerBusiness auraOwner = null;
                    foreach (PlayerBusiness p in m_players)
                    {
                        if (p == null) continue;
                        //死亡回城
                        m_scene.TownGate(p, TransmitType.Dead);
                        if (p.ID == m_rob.AuraOwner)
                        {
                            auraOwner = p;
                        }
                    }
                    if (auraOwner != null)
                    {
                        m_rob.LoseAuraOwner(auraOwner, true);
                    }
                }
            }
            finally
            {
                base.GameOver();
            }
        }
    }
}

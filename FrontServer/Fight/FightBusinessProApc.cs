using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;

namespace Sinan.FrontServer
{
    public class FightBusinessProApc : FightSceneApc
    {
        PartBusiness m_rb;

        public FightBusinessProApc(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players, string npcID, bool isEctype, SceneApc apc, int subID, PartBusiness pro) :
            base(teamA, teamB, players, npcID, isEctype, apc, subID)
        {
            m_rb = pro;
        }
        protected override void GameOver()
        {
            try
            {
                if (!m_teamB.All(x => (x.Over || x.FType == FighterType.BB)))
                {
                    IList list = m_rb.Part.Value["Element"] as IList;
                    if (list == null || list.Count == 0)
                        return;
                    string goodsid = list[0].ToString();
                    foreach (PlayerBusiness p in m_players)
                    {
                        if (p == null) continue;
                        int num = p.RemoveGoodsAll(goodsid, GoodsSource.Pro);
                        if (num > 0)
                        {
                            p.Call(ClientCommand.SendMsgToAllPlayerR, new object[] { string.Empty, TipManager.GetMessage(ActivityReturn.ProApcFail) });
                        }
                        //死亡回城
                        m_scene.TownGate(p, TransmitType.Dead);
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

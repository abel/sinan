using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 夺宝奇兵PK
    /// </summary>
    public class FightBusinessRob : FightBusinessPK
    {
        RobBusiness m_rob;

        public FightBusinessRob(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players)
            : base(teamA, teamB, players)
        {
            m_lost = 0;
            m_protectTime = 0;
            m_rob = PartProxy.TryGetPart(Part.Rob) as RobBusiness;
        }

        /// <summary>
        /// 处理结果
        /// </summary>
        /// <param name="winTeam">获胜的一方</param>
        /// <param name="lostTeam">失败的一方</param>
        protected override void FightEnd(FightObject[] winTeam, FightObject[] lostTeam)
        {
            //排名:
            foreach (FightPlayer v in winTeam)
            {
                if (v.Online)
                {
                    if (v.FType == FighterType.Player)
                    {
                        int count = m_rob.AddWiner(v.Player.PID);
                        if (count == 10 || count == 50 || count == 100)
                        {
                            string msg = string.Format(TipManager.GetMessage((int)PartMsgType.PKWin), v.Player.Name, count);
                            PlayersProxy.CallAll(ClientCommand.SendActivtyR, new object[] { "T03", msg });
                        }
                        //v.Player.AddExperience(m_rob.WinExp, FinanceType.RobPK);
                    }
                    else
                    {
                        v.Player.AddPetExp(v.Player.Pet, m_rob.WinExp, true,(int)GoodsSource.RobPK);
                    }
                }
            }

            foreach (FightPlayer v in lostTeam)
            {
                if (v.Online)
                {
                    if (v.FType == FighterType.Player)
                    {
                        m_rob.AddLoser(v.Player.PID);
                        //v.Player.AddExperience(m_rob.LostExp, FinanceType.Part);
                    }
                    else
                    {
                        //v.Player.AddPetExp(m_rob.LostExp);
                    }
                }
            }
            Variant winA = new Variant();
            winA["RExp"] = m_rob.WinExp;
            winA["PExp"] = m_rob.WinExp;

            Variant loseA = new Variant();
            loseA["RExp"] = m_rob.LostExp;
            loseA["PExp"] = m_rob.LostExp;

            SendToTeam(winTeam, FightCommand.FightEndR, (int)FightResult.Win, winA, string.Empty);
            SendToTeam(lostTeam, FightCommand.FightEndR, (int)FightResult.Lose, loseA, string.Empty);

            //改变所有者
            TryChangeAuraOwner(winTeam, lostTeam);

            foreach (FightPlayer v in lostTeam)
            {
                if (v.Online)
                {
                    if (v.FType == FighterType.Player)
                    {
                        m_scene.TownGate(v.Player, TransmitType.Dead);
                    }
                }
            }
        }

        private void TryChangeAuraOwner(FightObject[] winTeam, FightObject[] lostTeam)
        {
            if (string.IsNullOrEmpty(m_rob.AuraOwner))
            {
                return;
            }
            FightPlayer loster = lostTeam.FirstOrDefault(x => x.ID == m_rob.AuraOwner) as FightPlayer;
            if (loster != null)
            {
                FightPlayer newowner = winTeam.FirstOrDefault(x => x.Online) as FightPlayer;
                if (newowner != null)
                {
                    PlayerBusiness old = loster.Player;
                    PlayerBusiness newp = newowner.Player;

                    string msg = string.Format(TipManager.GetMessage((int)PartMsgType.AuraLose), old.Name, newp.Scene.Name, newp.Name);
                    m_rob.ChangeAuraOwner(loster.ID, newowner.ID, newp.Name, msg);
                }
            }
        }
    }
}

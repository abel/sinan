using System;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 守护战争(活动)
    /// </summary>
    public sealed class ProBusiness : PartBusiness
    {
        public ProBusiness(Part part)
            : base(part)
        {
        }

        /// <summary>
        /// 进入扣除
        /// </summary>
        /// <param name="player"></param>
        public override bool IntoPart(PlayerBusiness player)
        {
            if (BurdenManager.GoodsCount(player.B0, m_elements[0]) > 0)
            {
                //请先处理包袱中的【守护凭证】后再进入
                player.Call(ClientCommand.IntoSceneR, false, null,TipManager.GetMessage(ClientReturn.ProBusiness1));
                return false;
            }

            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant v = new Variant();
            v.Add("Number1", 1);
            v.Add("EndTime", this.EndTime.AddHours(1));//活动结束后一个小时道具过期

            dic.Add(m_elements[0], v);

            if (BurdenManager.IsFullBurden(player.B0, dic))
            {
                //包袱满,请整理你的包袱再进行该操作
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.ProBusiness2));
                return false;
            }

            bool pass = (m_part.Coin == 0 && m_part.Score == 0);
            if ((!pass) && m_part.Score > 0)
            {
                pass = player.AddScore(-m_part.Score, FinanceType.Part);
            }
            if ((!pass) && m_part.Coin > 0)
            {
                pass = player.AddCoin(-m_part.Coin, FinanceType.Part);
            }
            if (!pass)
            {
                player.Call(ClientCommand.IntoSceneR, false, null, m_part.CoinMsg);
                return false;
            }

            if (m_part.MaxInto > 0)
            {
                m_playerInto.SetOrInc(player.PID, 1);
            }

            //进入场景成功，发送道具
            player.AddGoods(dic, GoodsSource.Pro);
            player.MoreFight = 0;
            player.NoFight = 0;
            player.Call(ActivityCommand.GetDefendCardR, MainScene.Players.Count);
            //进入守护战争,记录活跃度
            player.AddAcivity(ActivityType.Pro, 1);
            return true;
        }

        /// <summary>
        /// 开始活动
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public override void Start(DateTime startTime, DateTime endTime)
        {
            m_start = startTime;
            m_end = endTime;
            PlayersProxy.CallAll(PartCommand.PartStartR, new object[] { this });
            foreach (ScenePart scene in m_scenes)
            {
                if (scene != null)
                {
                    scene.Start(this);
                }
            }
        }

        /// <summary>
        /// 结束活动
        /// </summary>
        /// <returns></returns>
        public override void End()
        {
            try
            {
                ScenePart scene = m_mainScene as ScenePart;
                if (scene != null)
                {
                    scene.End();
                }
            }
            finally
            {
                base.End();
            }
        }
    }
}
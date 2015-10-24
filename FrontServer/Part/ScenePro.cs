using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 守护战争(不能组队进)
    /// </summary>
    public sealed class ScenePro : ScenePart
    {
        private Timer m_timer;
        private string m_goods;
        private int i = 0;
        public override SceneType SceneType
        {
            get { return SceneType.Pro; }
        }

        public string Goods
        {
            get { return m_goods; }
        }

        public DateTime EndTime
        {
            get { return m_part.EndTime; }
        }

        public ScenePro(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
            m_fightType = FightType.ProPK;
        }

        public override SceneBusiness CreateNew()
        {
            return this;
        }

        /// <summary>
        /// 活动开始
        /// </summary>
        /// <param name="robID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public override void Start(PartBusiness robID)
        {
            base.Start(robID);
            m_goods = robID.Elements[0];
            m_timer = new Timer(m_timer_Elapsed);
            m_timer.Change(10000, 10000);            
        }

        /// <summary>
        /// 定时取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_timer_Elapsed(object sender)
        {
            //表示活动还没有开始
            if (m_part.StartTime > DateTime.UtcNow)
            {
                return;
            }
            foreach (PlayerBusiness pb in m_players.Values)
            {
                //判断是否在战斗中
                if (pb.AState != ActionState.Fight)
                {
                    pb.NoFight += 10;
                    ExpPro(pb);
                }
            }
            i++;
            if (i == 6)
            {
                i = 0;
            }
        }

        /// <summary>
        /// 活动结束
        /// </summary>
        /// <returns></returns>
        public override bool End()
        {
            m_timer.Dispose();
            foreach (PlayerBusiness OnlineBusiness in m_players.Values)
            {
                OnlineBusiness.NoFight = 0;
            }
            return base.End();
        }


        /// <summary>
        /// 角色断线
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Disconnected(PlayerBusiness player)
        {
            //断开，移除所有道具
            FightBusinessPro pro = player.Fight as FightBusinessPro;
            if (pro == null)
            {
                player.RemoveGoodsAll(m_goods,GoodsSource.ProExit);
            }
            return base.Disconnected(player);
        }

        /// <summary>
        /// 根据所在活动中的时间,得到相关经验
        /// </summary>
        /// <param name="pb">角色</param>
        /// <returns></returns>
        private void ExpPro(PlayerBusiness pb)
        {
            if (pb.NoFight % 30 == 0)
            {
                //玩家等级X（玩家等级-2）X活动时间（秒）/600+2000，-1
                //OnlineBusiness.ExpLv()));

                int exp = Convert.ToInt32(Math.Pow(pb.Level - 45, 2) * 200 + Math.Pow(pb.NoFight, 2) / 100 + 50000);

                int count = BurdenManager.GoodsCount(pb.B0, m_goods, m_part.EndTime.AddHours(1));
                if (count > 5)
                {
                    count = 5;
                }

                exp *= count;
                if (exp > 0)
                {
                    pb.AddExperience(exp, FinanceType.Pro);
                    pb.Call(ActivityCommand.ProExpR, exp);
                }
            }
        }
    }
}

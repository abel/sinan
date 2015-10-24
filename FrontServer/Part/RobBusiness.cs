using System;
using System.Collections;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 夺宝奇兵(活动)
    /// </summary>
    public sealed class RobBusiness : PartBusiness
    {
        readonly int m_winExp;
        readonly int m_lostExp;
        readonly int m_totalExp;
        /// <summary>
        /// 统计玩家进入的次数
        /// Key:玩家ID, Value:进入的次数
        /// </summary>
        readonly ConcurrentDictionary<string, int> m_frequency;

        /// <summary>
        /// PK胜利方获得的经验
        /// </summary>
        public int WinExp
        {
            get { return m_winExp; }
        }

        /// <summary>
        /// PK失败方获得的经验
        /// </summary>
        public int LostExp
        {
            get { return m_lostExp; }
        }

        public int TotalExp
        {
            get { return m_totalExp; }
        }

        Variant m_familyAward;
        string m_auraOwner;
        string m_ownerName;

        /// <summary>
        /// 元素光环所有者ID
        /// </summary>
        public string AuraOwner
        {
            get { return m_auraOwner; }
        }

        /// <summary>
        /// 光环所有者名称
        /// </summary>
        public string OwnerName
        {
            get { return m_ownerName; }
        }


        /// <summary>
        /// 家族奖励
        /// </summary>
        public Variant FamilyAward
        {
            get { return m_familyAward; }
        }

        public RobBusiness(Part part)
            : base(part)
        {
            m_frequency = new ConcurrentDictionary<string, int>();

            m_winExp = part.Value.GetIntOrDefault("VExp");
            m_lostExp = part.Value.GetIntOrDefault("LExp");
            m_totalExp = part.Value.GetIntOrDefault("TotalExp");
            m_familyAward = part.Value.GetVariantOrDefault("FamilyAward");
        }

        public override bool IntoCheck(PlayerBusiness player)
        {
            if (string.IsNullOrEmpty(player.FamilyName))
            {
                player.Call(ClientCommand.IntoSceneR, false, null,TipManager.GetMessage(ClientReturn.RobBusiness1));
                return false;
            }
            return base.IntoCheck(player);
        }

        public void Call(string command, IList objs)
        {
            var buffer = AmfCodec.Encode(command, objs);
            foreach (var v in m_scenes)
            {
                v.CallAll(0, buffer);
            }
        }


        /// <summary>
        /// 合成光环
        /// </summary>
        /// <param name="note"></param>
        public bool CombinAura(UserNote note)
        {
            PlayerBusiness player = note.Player;
            //TODO:扣除玩家的元素..
            if (string.IsNullOrEmpty(m_auraOwner))
            {
                DateTime time = EndTime.AddHours(1);
                foreach (var goodsid in m_elements)
                {
                    if (BurdenManager.GoodsCount(note.Player.B0, goodsid, time) == 0)
                    {
                        string msg = TipManager.GetMessage((int)PartMsgType.AuraLack);
                        note.Call(PartCommand.AuraChangeR, false, m_auraOwner, m_ownerName, msg);
                        return false;
                    }
                };
                m_auraOwner = player.ID;
                m_ownerName = player.Name;
                foreach (var goodsid in m_elements)
                {
                    note.Player.RemoveGoods(goodsid, time, 1, GoodsSource.CombinAura);
                }

                string msg2 = string.Format(TipManager.GetMessage((int)PartMsgType.AuraCompose), player.Name);
                Call(PartCommand.AuraChangeR, new object[] { true, player.ID, player.Name, msg2 });
                return true;
            }

            string msg3 = string.Format(TipManager.GetMessage((int)PartMsgType.AuraLate), m_ownerName);
            note.Call(PartCommand.AuraChangeR, false, m_auraOwner, m_ownerName, msg3);
            return false;
        }

        /// <summary>
        /// 改变光环所有者
        /// </summary>
        /// <param name="player"></param>
        /// <param name="oldID"></param>
        public bool ChangeAuraOwner(string oldID, string newID, string name, string msg)
        {
            if (m_auraOwner == oldID)
            {
                m_auraOwner = newID;
                m_ownerName = name;
                Call(PartCommand.AuraChangeR, new object[] { true, m_auraOwner, m_ownerName, msg });
                return true;
            }
            return false;
        }

        /// <summary>
        /// 断线或打怪失败
        /// 遗失光环(由系统回收)
        /// </summary>
        /// <param name="player"></param>
        public void LoseAuraOwner(Player player, bool online)
        {
            if (m_auraOwner == player.ID)
            {
                string msg;
                if (online)
                {
                    msg = string.Format(TipManager.GetMessage((int)PartMsgType.AuraLoseApc), player.Name);
                }
                else
                {
                    msg = string.Format(TipManager.GetMessage((int)PartMsgType.AuraLoseDis), player.Name);
                }
                m_auraOwner = string.Empty;
                m_ownerName = string.Empty;
                Call(PartCommand.AuraChangeR, new object[] { true, m_auraOwner, m_ownerName, msg });
            }
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
            m_frequency.Clear();
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
                foreach (ScenePart scene in m_scenes)
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

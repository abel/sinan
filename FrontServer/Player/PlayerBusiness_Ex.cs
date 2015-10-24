using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 定义扩展部分
    /// </summary>
    partial class PlayerBusiness
    {
        /// <summary>
        /// 参与竞技场编号
        /// </summary>
        public string SoleID
        {
            get;
            set;
        }

        /// <summary>
        /// 竞技场分组,表示加入的组
        /// </summary>
        public string GroupName
        {
            get;
            set;
        }

        PlayerEx m_ePro;
        PlayerEx m_sPro;

        PlayerEx m_assist;
        PlayerEx m_skill;
        PlayerEx m_hotKeys;
        PlayerEx m_equips;
        PlayerEx m_social;
        PlayerEx m_family;
        PlayerEx m_title;
        PlayerEx m_effort;
        PlayerEx m_home;
        PlayerEx m_petBook;
        PlayerEx m_ectype;
        PlayerEx m_daily;
        PlayerEx m_buy;

        PlayerEx m_b0;
        PlayerEx m_b1;
        PlayerEx m_b2;
        PlayerEx m_b3;

        PlayerEx m_star;

        PlayerEx m_lottery;

        PlayerEx m_member;

        PlayerEx m_taskday;


        PlayerEx m_wardrobe;

        /// <summary>
        /// 战斗辅助
        /// </summary>
        public PlayerEx Assist
        {
            get { return m_assist; }
        }

        /// <summary>
        /// 玩家技能
        /// </summary>
        public PlayerEx Skill
        {
            get { return m_skill; }
        }

        /// <summary>
        /// 热键设置
        /// </summary>
        public PlayerEx HotKeys
        {
            get { return m_hotKeys; }
        }

        /// <summary>
        /// 装备
        /// </summary>
        public PlayerEx Equips
        {
            get { return m_equips; }
        }

        /// <summary>
        /// 称号
        /// </summary>
        public PlayerEx Title
        {
            get { return m_title; }
        }

        /// <summary>
        /// 成就
        /// </summary>
        public PlayerEx Effort
        {
            get { return m_effort; }
        }

        /// <summary>
        /// 家族信息
        /// </summary>
        public PlayerEx Family
        {
            get { return m_family; }
        }

        /// <summary>
        /// 御宠图鉴
        /// </summary>
        public PlayerEx PetBook
        {
            get { return m_petBook; }
        }
        /// <summary>
        /// 社交
        /// </summary>
        public PlayerEx Social
        {
            get { return m_social; }
        }
        /// <summary>
        /// 家园
        /// </summary>
        public PlayerEx Home
        {
            get { return m_home; }
        }

        /// <summary>
        /// 普通包袱
        /// </summary>
        public PlayerEx B0
        {
            get { return m_b0; }
        }
        /// <summary>
        /// 普通仓库
        /// </summary>
        public PlayerEx B1
        {
            get { return m_b1; }
        }
        /// <summary>
        /// 宠物仓库
        /// </summary>
        public PlayerEx B2
        {
            get { return m_b2; }
        }
        /// <summary>
        /// 宠物兽栏
        /// </summary>
        public PlayerEx B3
        {
            get { return m_b3; }
        }

        /// <summary>
        /// 副本信息(保存进入副本的次数及杀怪信息)
        /// 玩家最后一次进入的场景信息
        /// </summary>
        public PlayerEx Ectype
        {
            get { return m_ectype; }
        }

        /// <summary>
        /// 限定用户每天做事情的次数
        /// 如(开箱记录,地图进入记录,组队副本进入记录)
        /// </summary>
        public PlayerEx Daily
        {
            get { return m_daily; }
        }

        /// <summary>
        /// 1点券惊爆价购买记录
        /// </summary>
        public PlayerEx Buylog
        {
            get { return m_buy; }
        }

        /// <summary>
        /// 星座
        /// </summary>
        public PlayerEx Star
        {
            get { return m_star; }
        }

        /// <summary>
        /// 摇奖扩展
        /// </summary>
        public PlayerEx Lottery
        {
            get { return m_lottery; }
        }

        /// <summary>
        /// 会员
        /// </summary>
        public PlayerEx Member
        {
            get { return m_member; }
        }

        /// <summary>
        /// 每日任务统计
        /// </summary>
        public PlayerEx TaskDay
        {
            get { return m_taskday; }
        }

        /// <summary>
        /// 衣柜
        /// </summary>
        public PlayerEx Wardrobe
        {
            get { return m_wardrobe; }
        }

        /// <summary>
        /// 设置扩展信息
        /// </summary>
        private void setPlayerEx()
        {
            NewPlayerEx(ref m_skill, "Skill");
            NewPlayerEx(ref m_equips, "Equips");
            NewPlayerEx(ref m_social, "Social");
            NewPlayerEx(ref m_family, "Family");
            NewPlayerEx(ref m_title, "Title");
            NewPlayerEx(ref m_effort, "Effort");
            NewPlayerEx(ref m_home, "Home");
            NewPlayerEx(ref m_petBook, "PetBook");
            NewPlayerEx(ref m_daily, "Daily");
            NewPlayerEx(ref m_buy, "Buy");
            NewPlayerEx(ref m_b0, "B0");
            NewPlayerEx(ref m_b1, "B1");
            NewPlayerEx(ref m_b2, "B2");
            NewPlayerEx(ref m_b3, "B3");


            NewPlayerEx(ref m_star, "Star");
            NewPlayerEx(ref m_lottery, "Lottery");
            NewPlayerEx(ref m_member, "Member");
            NewPlayerEx(ref m_taskday, "TaskDay");
            NewPlayerEx(ref m_wardrobe, "Wardrobe");

            NewPlayerEx(ref m_ePro, "EPro");
            //NewPlayerEx(ref m_sPro, "SPro");
            //重新计算,修复星座缓存问题.
            //if (Level > 25 && m_sPro.Value.Count == 0)
            {
                m_sPro = new PlayerEx(this.ID, "SPro");
                PlayerProperty p = new PlayerProperty();
                ResetSkillsAdd(p);
            }

            if (NewPlayerEx(ref m_hotKeys, "HotKeys"))
            {
                Variant hotkey = m_hotKeys.Value;
                for (int i = 0; i <= 9; i++)
                {
                    hotkey.Add(i.ToString(), string.Empty);
                }
                hotkey.Add("-", string.Empty);
                hotkey.Add("=", string.Empty);
                hotkey.Add("B", "WopenPackageWin");
                hotkey.Add("C", "WopenSKConfigWin");
                hotkey.Add("P", string.Empty);
                hotkey.Add("R", "WopenPlayerWin");
                m_hotKeys.Save();
            }

            if (NewPlayerEx(ref m_ectype, "Ectype"))
            {
                m_ectype.Value["IntoLog"] = new Variant();
                m_ectype.Save();
            }

            if (!NewPlayerEx(ref m_assist, "Assist"))
            {
                //把剩余秒数转化为时间(双倍经验)
                SecondToTime();
                //设置红黄名时间
                SetNameTime();
            }

            List<Mounts> list = MountsAccess.Instance.GetMounts(ID);
            if (list != null && list.Count > 0)
            {
                m_mounts = list[0];
            }

            //得到装备面板信息
            IList c = m_b3.Value.GetValue<IList>("C");
            Variant v = null;
            foreach (Variant pv in c)
            {
                if (pv.GetIntOrDefault("I") == 1)
                {
                    v = pv;
                    break;
                }
            }
            if (v != null)
            {
                this.m_pet = PetAccess.Instance.FindOneById(v.GetStringOrDefault("E"));
                if (m_pet != null)
                {
                    m_pet.Init();
                    PetAccess.PetReset(m_pet, Skill, false, m_mounts);
                }
            }
            this.m_slippets = PlayerExAccess.Instance.SlipPets(m_b3);
            this.IsVIP();
        }

        /// <summary>
        /// 保存玩家的扩展数据.
        /// </summary>
        private void SaveEx()
        {
            // 外理退出不计时的("RExp":双倍经验,"AF":战斗计时器,"AH":"额外增加的遇怪数")
            TimeToSecond();
            // 保存玩家的扩展数据
            foreach (var v in Value)
            {
                PlayerEx ex = v.Value as PlayerEx;
                if (ex != null && ex.Changed)
                {
                    ex.Save();
                }
            }
        }

        private bool NewPlayerEx(ref PlayerEx ex, string name)
        {
            ex = this.Value.GetValueOrDefault<PlayerEx>(name);
            if (ex == null)
            {
                ex = new PlayerEx(this.ID, name);
                this.Value[name] = ex;
            }
            if (ex.Value == null)
            {
                ex.Value = new Variant();
                return true;
            }
            return false;
        }
    }
}

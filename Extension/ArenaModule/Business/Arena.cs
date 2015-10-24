using System;
using System.Collections.Generic;

#if mono
using Sinan.Collections;
#else

#endif

namespace Sinan.ArenaModule.Business
{
    /// <summary>
    /// 竞技场基本信息
    /// </summary>
    public class Arena
    {
        private string m_soleid;
        private string m_playerid;
        private string m_name;

        private int m_wartype;
        private bool m_iswatch;
        private int m_wintype;
        private bool m_isotherinto;
        private int m_petnumber;
        private int m_petmin;
        private int m_petmax;
        private int m_group;
        private string m_scene;
        private string m_arenaid;
        private bool m_isgoods;
        private int m_preparetime;
        private int m_gametime;
        private int m_status;
        private int m_userpets;
        private string m_password;
        private int m_fightpoor;
        private Int64 m_fightvalue;
        private List<string> m_groupname=new List<string>();

        /// <summary>
        /// 唯一标识
        /// </summary>
        public string SoleID 
        {
            get { return m_soleid; }
            set { m_soleid = value; }
        }

        /// <summary>
        /// 竞技创建者
        /// </summary>
        public string PlayerID 
        {
            get { return m_playerid; }
            set { m_playerid=value; }
        }
        /// <summary>
        /// 创建者名字
        /// </summary>
        public string Name 
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// 战斗类型
        /// </summary>
        public int WarType
        {
            get { return m_wartype; }
            set { m_wartype = value; }
        }

        /// <summary>
        /// 是否可以观战
        /// </summary>
        public bool IsWatch 
        {
            get { return m_iswatch; }
            set { m_iswatch = value; }
        }

        /// <summary>
        /// 胜利方式
        /// </summary>
        public int WinType 
        {
            get { return m_wintype; }
            set { m_wintype = value; }
        }

        /// <summary>
        /// 是否中途参战
        /// </summary>
        public bool IsOtherInto 
        {
            get { return m_isotherinto; }
            set { m_isotherinto = value; }
        }

        /// <summary>
        /// 允许各组参战宠物数量
        /// </summary>
        public int PetNumber 
        {
            get { return m_petnumber; }
            set { m_petnumber = value; }
        }

        /// <summary>
        /// 宠物允许最小值
        /// </summary>
        public int PetMin 
        {
            get { return m_petmin; }
            set { m_petmin = value; }
        }

        /// <summary>
        /// 宠物允许最大值
        /// </summary>
        public int PetMax 
        {
            get { return m_petmax; }
            set { m_petmax = value; }
        }

        /// <summary>
        /// 允许组数
        /// </summary>
        public int Group 
        {
            get { return m_group; }
            set { m_group = value; }
        }

        /// <summary>
        /// 场景
        /// </summary>
        public string Scene 
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        /// 基本配置
        /// </summary>
        public string ArenaID 
        {
            get { return m_arenaid; }
            set { m_arenaid = value; }
        }

        /// <summary>
        /// 是否可以使用道具
        /// </summary>
        public bool IsGoods 
        {
            get { return m_isgoods; }
            set { m_isgoods = value; }
        }

        /// <summary>
        /// 预期时间
        /// </summary>
        public int PrepareTime 
        {
            get { return m_preparetime; }
            set { m_preparetime = value; }
        }

        /// <summary>
        /// 时长
        /// </summary>
        public int GameTime 
        {
            get { return m_gametime; }
            set { m_gametime = value; }
        }

        /// <summary>
        /// 当前竞技场状态
        /// 0表示没有开始
        /// 1表示正在进行中
        /// 2表示结束
        /// </summary>
        public int Status 
        {
            get { return m_status; }
            set { m_status = value; }
        }

        /// <summary>
        /// 允许角色参战宠物数量
        /// </summary>
        public int UserPets 
        {
            get { return m_userpets; }
            set { m_userpets = value; }
        }
        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord 
        {
            get { return m_password; }
            set { m_password = value; }
        }

        /// <summary>
        /// 战绩差
        /// </summary>
        public int FightPoor 
        {
            get { return m_fightpoor; }
            set { m_fightpoor = value; }
        }

        /// <summary>
        /// 当前战绩值
        /// </summary>
        public Int64 FightValue 
        {
            get { return m_fightvalue; }
            set { m_fightvalue = value; }
        }

        /// <summary>
        /// 分组名称
        /// </summary>
        public List<string> GroupName 
        {
            get { return m_groupname; }
            set { m_groupname = value; }
        }
    }
}

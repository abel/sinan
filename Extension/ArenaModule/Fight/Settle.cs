
namespace Sinan.ArenaModule.Fight
{
    public class Settle
    {
        private string m_playerid;
        private string m_groupname;
        private int m_ranking;

        private string m_playername;
        private int m_totalwin;

        private int m_winfight;
        private int m_totalloss;

        private int m_lossfight;
        private int m_otherfight;

        private int m_totalfight;

        private int m_resulttype;


        /// <summary>
        /// 角色
        /// </summary>
        public string PlayerID { get { return m_playerid; } set { m_playerid = value; } }

        /// <summary>
        /// 分组
        /// </summary>
        public string GroupName { get { return m_groupname; } set { m_groupname = value; } }
        /// <summary>
        /// 名次
        /// </summary>
        public int Ranking { get { return m_ranking; } set { m_ranking = value; } }
        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName { get { return m_playername; } set { m_playername = value; } }
        /// <summary>
        /// 总击杀数
        /// </summary>
        public int TotalWin { get { return m_totalwin; } set { m_totalwin = value; } }
        /// <summary>
        /// 击杀获得战绩
        /// </summary>
        public int WinFight 
        { 
            get 
            { 
                return m_winfight; 
            } 
            set 
            { 
                m_winfight = value;                
            } 
        }
        /// <summary>
        /// 被击杀数
        /// </summary>
        public int TotalLoss { get { return m_totalloss; } set { m_totalloss = value; } }
        /// <summary>
        /// 损失战绩
        /// </summary>
        public int LossFight 
        { 
            get 
            { 
                return m_lossfight; 
            }
            set 
            { 
                m_lossfight = value;                 
            } 
        }

        /// <summary>
        /// 附加战绩
        /// </summary>
        public int OtherFight
        {
            get
            {
                return m_otherfight;
            }
            set
            {
                m_otherfight = value;                
            }
        }

        /// <summary>
        /// 总战绩
        /// </summary>
        public int TotalFight 
        {
            get { return m_totalfight; }
            set { m_totalfight = value; }
        }

        /// <summary>
        /// 0输,1赢，2平
        /// </summary>
        public int ResultType 
        {
            get { return m_resulttype; }
            set { m_resulttype = value; }
        }
    }
}

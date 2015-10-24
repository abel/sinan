using System;
using System.Collections.Generic;
using Sinan.ArenaModule.Detail;
using Sinan.FrontServer;
using Sinan.Command;
using Sinan.Entity;
using Sinan.ArenaModule.Fight;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.ArenaModule.Business
{
    public class ArenaBase : Arena
    {
        private ConcurrentDictionary<string, PlayerBusiness> m_players;
        private ConcurrentDictionary<string, Pet> m_pets;
        private DateTime m_starttime;
        private DateTime m_endtime;
        private ArenaScene m_scenesession;
        private ConcurrentDictionary<string, Settle> m_settle;

        public ArenaBase()
        {
            m_players = new ConcurrentDictionary<string, PlayerBusiness>();//进入竞技场的角色列表
            m_pets = new ConcurrentDictionary<string, Pet>();//所有参战的宠物列表
            m_settle = new ConcurrentDictionary<string, Settle>();//所有战斗记录
        }

        /// <summary>
        /// 进入竞技场的角色
        /// </summary>
        public ConcurrentDictionary<string, PlayerBusiness> Players
        {
            get { return m_players; }
            set { m_players = value; }
        }

        /// <summary>        
        /// 所有参战的宠物列表
        /// </summary>
        public ConcurrentDictionary<string, Pet> Pets
        {
            get { return m_pets; }
            set { m_pets = value; }
        }

        /// <summary>
        /// 所有战斗记录
        /// </summary>
        public ConcurrentDictionary<string, Settle> SettleInfo 
        {
            get { return m_settle; }
            set { m_settle = value; }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime
        {
            get { return m_starttime; }
            set { m_starttime = value; }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime
        {
            get { return m_endtime; }
            set { m_endtime = value; }
        }

        /// <summary>
        /// 场景基本信息
        /// </summary>
        public ArenaScene SceneSession
        {
            get { return m_scenesession; }
            set { m_scenesession = value; }
        }

        /// <summary>
        /// 得到角色基本信息
        /// </summary>
        /// <returns></returns>
        public void ArenaUserCount()
        {
            //参战人数
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Pet p in m_pets.Values)
            {
                if (!dic.ContainsKey(p.PlayerID))
                {
                    dic.Add(p.PlayerID, p.PlayerID);
                }
            }
            CallAll(ArenaCommand.ArenaUserCountR, m_players.Count, dic.Count);
        }

        /// <summary>
        /// 当前竞技场所有玩家
        /// </summary>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        public void CallAll(string command, params object[] objs)
        {
            var buffer = AmfCodec.Encode(command, objs);
            foreach (PlayerBusiness player in m_players.Values)
            {
                player.Call(buffer);
            }
        }


        /// <summary>
        /// 角色断线,移除所有参战宠
        /// </summary>
        /// <param name="playerid"></param>
        public void UserDisconnected(string playerid)
        {
            if (this.Status == 1)
            {                
                ArenaFight.UseDis(this, playerid);
            }
            PlayerBusiness pb;
            if (m_players.TryRemove(playerid, out pb))
            {
                pb.SoleID = "";
                pb.GroupName = "";

                pb.SetActionState(ActionState.Standing);

                List<string> tmp = new List<string>();
                foreach (Pet p in m_pets.Values)
                {
                    if (p.PlayerID == playerid)
                    {
                        tmp.Add(p.ID);
                    }
                }

                List<PetDetail> list = new List<PetDetail>();
                foreach (string k in tmp)
                {
                    Pet p;
                    if (m_pets.TryRemove(k, out p))
                    {
                        //移除的宠物
                        PetDetail model = new PetDetail(p, pb.Name, 1);
                        list.Add(model);
                    }
                }

                if (list.Count > 0)
                {
                    //通知当前所有该竞技场上的角色
                    CallAll(ArenaCommand.PetOutArenaR, true, list);
                }
            }
        }

        /// <summary>
        /// 移除指定宠物
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <param name="petid">宠物</param>       
        /// <returns>true成功,false宠物不存在</returns>
        public bool RemovePet(string playerid, string petid)
        {
            List<PetDetail> list = new List<PetDetail>();
            Pet p;
            if (m_pets.TryRemove(petid, out p))
            {
                PetDetail detail = new PetDetail(p, "", 1);
                list.Add(detail);
            }

            if (list.Count > 0)
            {
                CallAll(ArenaCommand.PetOutArenaR, true, list);
                return true;
            }
            return false;
        }
    }
}

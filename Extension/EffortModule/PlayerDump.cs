using System;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Schedule;
using Sinan.FrontServer;

namespace Sinan.EffortModule.Business
{
    /// <summary>
    /// 玩家表快照Snapshot
    /// </summary>
    sealed public class PlayerDump : SchedulerBase
    {
        PlayerDump()
            : base(Timeout.Infinite, 24 * 3600 * 1000)
        {
        }

        public override void Start()
        {
            DateTime now = DateTime.Now;
            m_dueTime = (24 * 3600 * 1000) - (int)((now - now.Date).TotalMilliseconds);
            base.Start();
            Dump(true);
        }

        protected override void Exec()
        {
            Dump(false);
        }

        private void Dump(bool checkday)
        {
            try
            {
                string day = DateTime.Now.AddSeconds(5).ToString("yyyyMMdd");
                PlayerSortAccess.Instance.PlayerDump(day, checkday);
                CachePlayerRank(day);
                CachePetRank(day);
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error("Rank:", ex);
            }
        }

        #region PlayerRank
        public static Player[] DianPlayers = new Player[100];
        public static Player[] Lev0Players = new Player[100];
        public static Player[] Lev1Players = new Player[100];
        public static Player[] Lev2Players = new Player[100];
        public static Player[] Lev3Players = new Player[100];

        private void CachePlayerRank(string day)
        {
            List<Player> players = TopAccess.Instance.GetPlayerPaiHang("Topn" + day);
            foreach (var player in players)
            {
                int lev0 = player.Value.GetIntOrDefault("Lev0");
                int lev1 = player.Value.GetIntOrDefault("Lev1");
                int lev2 = player.Value.GetIntOrDefault("Lev2");
                int lev3 = player.Value.GetIntOrDefault("Lev3");
                int dian = player.Value.GetIntOrDefault("Dian");
                if (lev0 > 0 && lev0 <= Lev0Players.Length)
                {
                    Lev0Players[lev0 - 1] = player;
                }

                if (lev1 > 0 && lev1 <= Lev1Players.Length)
                {
                    Lev1Players[lev1 - 1] = player;
                }

                if (lev2 > 0 && lev2 <= Lev2Players.Length)
                {
                    Lev2Players[lev2 - 1] = player;
                }

                if (lev3 > 0 && lev3 <= Lev3Players.Length)
                {
                    Lev3Players[lev3 - 1] = player;
                }

                if (dian > 0 && dian <= DianPlayers.Length)
                {
                    DianPlayers[dian - 1] = player;
                }
            }
        }
        /// <summary>
        /// 获取玩家排名(按成就点)
        /// </summary>
        /// <returns></returns>
        static public List<Player> GetPlayerPaiHang(int pageSize, int pageIndex, string roleID)
        {
            Player[] players;
            if (roleID == "-1")
            {
                players = DianPlayers;
            }
            else if (roleID == "1")
            {
                players = Lev1Players;
            }
            else if (roleID == "2")
            {
                players = Lev2Players;
            }
            else if (roleID == "3")
            {
                players = Lev3Players;
            }
            else
            {
                players = Lev0Players;
            }
            List<Player> v = new List<Player>(pageSize);
            int start = pageSize * pageIndex;
            for (int i = 0; i < pageSize; i++)
            {
                if (players.Length > i + start)
                {
                    Player p = players[i + start];
                    if (p != null)
                    {
                        p.Online = PlayersProxy.Online(p.PID);
                        v.Add(p);
                        continue;
                    }
                }
                break;
            }
            return v;
        }
        #endregion


        #region PetRank
        public static PetRank[] CcdPets = new PetRank[100];
        public static PetRank[] Lev0Pets = new PetRank[100];
        public static PetRank[] Lev1Pets = new PetRank[100];
        public static PetRank[] Lev2Pets = new PetRank[100];
        public static PetRank[] Lev3Pets = new PetRank[100];

        private void CachePetRank(string day)
        {
            List<PetRank> pets = TopAccess.Instance.GetPetPaiHang("TopnPet" + day);
            foreach (var player in pets)
            {
                int lev0 = player.Value.GetIntOrDefault("Lev0");
                int lev1 = player.Value.GetIntOrDefault("Lev1");
                int lev2 = player.Value.GetIntOrDefault("Lev2");
                int lev3 = player.Value.GetIntOrDefault("Lev3");
                int ccd = player.Value.GetIntOrDefault("CCD");
                if (lev0 > 0 && lev0 <= Lev0Pets.Length)
                {
                    Lev0Pets[lev0 - 1] = player;
                }

                if (lev1 > 0 && lev1 <= Lev1Pets.Length)
                {
                    Lev1Pets[lev1 - 1] = player;
                }

                if (lev2 > 0 && lev2 <= Lev2Pets.Length)
                {
                    Lev2Pets[lev2 - 1] = player;
                }

                if (lev3 > 0 && lev3 <= Lev3Pets.Length)
                {
                    Lev3Pets[lev3 - 1] = player;
                }

                if (ccd > 0 && ccd <= CcdPets.Length)
                {
                    CcdPets[ccd - 1] = player;
                }
            }
        }
        /// <summary>
        /// 获取玩家排名(按成就点)
        /// </summary>
        /// <returns></returns>
        static public List<PetRank> GetPetPaiHang(int pageSize, int pageIndex, string roleID)
        {
            PetRank[] pets;
            if (roleID == "-1")
            {
                pets = CcdPets;
            }
            else if (roleID == "1")
            {
                pets = Lev1Pets;
            }
            else if (roleID == "2")
            {
                pets = Lev2Pets;
            }
            else if (roleID == "3")
            {
                pets = Lev3Pets;
            }
            else
            {
                pets = Lev0Pets;
            }
            List<PetRank> v = new List<PetRank>(pageSize);
            int start = pageSize * pageIndex;
            for (int i = 0; i < pageSize; i++)
            {
                if (pets.Length > i + start)
                {
                    PetRank p = pets[i + start];
                    if (p != null)
                    {
                        v.Add(p);
                        continue;
                    }
                }
                break;
            }
            return v;
        }
        #endregion
    }
}

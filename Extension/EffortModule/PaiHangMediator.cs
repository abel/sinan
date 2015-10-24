using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.EffortModule.Business;
using Sinan.Util;

namespace Sinan.EffortModule
{
    /// <summary>
    /// 排行
    /// </summary>
    sealed public class PaiHangMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                PaiHangCommand.GetRoleLev,
                PaiHangCommand.GetPetLev,
                PaiHangCommand.GetMyRank,
                PaiHangCommand.GetRobRank,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            switch (note.Name)
            {
                case PaiHangCommand.GetRoleLev:
                    GetRoleLev(note);
                    break;
                case PaiHangCommand.GetPetLev:
                    GetPetLev(note);
                    break;
                case PaiHangCommand.GetMyRank:
                    GetMyRank(note);
                    break;
                case PaiHangCommand.GetRobRank:
                    GetRobRank(note);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 夺宝奇兵排行
        /// </summary>
        /// <param name="note"></param>
        private void GetRobRank(UserNote note)
        {
            int index = note.GetInt32(0); //页数,从0开始
            int pageSize = 10;// note.GetInt32(1);  //每页大小
            PartBusiness pb = PartProxy.TryGetPart(Part.Rob);
            List<PlayerPKRank> result;
            if (pb == null)
            {
                result = new List<PlayerPKRank>();
            }
            else
            {
                result = pb.GetRank(index, pageSize);
            }
            note.Call(PaiHangCommand.GetRobRankR, index, pageSize, result);
        }

        private void GetRoleLev(UserNote note)
        {
            int index = note.GetInt32(0); //页数,从0开始
            int pageSize = 10;// note.GetInt32(1);  //每页大小
            string roleID = note.GetString(2); //种族

            //if (roleID == "-1")
            //{
            //    var dians = TopAccess.Instance.GetPlayerPaiHang(pageSize, index);
            //    note.Call(PaiHangCommand.GetRoleLevR, index, pageSize, dians.Select(x => new PlayerSimple(x, 2)));
            //    return;
            //}
            //var v = TopAccess.Instance.GetPlayerPaiHang(pageSize, index, roleID);
            //note.Call(PaiHangCommand.GetRoleLevR, index, pageSize, v.Select(x => new PlayerSimple(x, 1)));

            var v = PlayerDump.GetPlayerPaiHang(pageSize, index, roleID);
            note.Call(PaiHangCommand.GetRoleLevR, index, pageSize, v.Select(x => new PlayerSimple(x, 2)));
        }

        Dictionary<string, string> m_playerNames = new Dictionary<string, string>(1000);

        private void GetPetLev(UserNote note)
        {
            int index = note.GetInt32(0);          //页数,从0开始
            int pageSize = 10;// note.GetInt32(1); //每页大小
            string roleID = note.GetString(2);     //种族
            //List<Pet> v;
            //if (roleID == "-1") //成长总值排行
            //{
            //    v = PetAccess.Instance.GetPetPaiHang(pageSize, index);
            //}
            //else
            //{
            //    v = PetAccess.Instance.GetPetPaiHang(pageSize, index, roleID);
            //}
            //foreach (var pet in v)
            //{
            //    string playerName;
            //    if (!m_playerNames.TryGetValue(pet.PlayerID, out playerName))
            //    {
            //        int pid;
            //        if (Sinan.Extensions.StringFormat.TryHexNumber(pet.PlayerID, out pid))
            //        {
            //            playerName = PlayerAccess.Instance.GetPlayerName(pid);
            //        }
            //        m_playerNames[pet.PlayerID] = playerName;
            //    }
            //    pet.Value["PlayerName"] = playerName;
            //}
            //note.Call(PaiHangCommand.GetPetLevR, index, pageSize, v.Select(x => new PetSimple(x, 1)));

            var v = PlayerDump.GetPetPaiHang(pageSize, index, roleID);
            note.Call(PaiHangCommand.GetPetLevR, index, pageSize, v);
        }

        private void GetMyRank(UserNote note)
        {
            //int rank = TopAccess.Instance.GetMyRank(note.Player);
            //note.Call(PaiHangCommand.GetMyRankR, rank);

            for (int i = 0; i < PlayerDump.Lev0Players.Length; i++)
            {
                Player p = PlayerDump.Lev0Players[i];
                if (p == null) break;
                if (p.PID == note.PID)
                {
                    string fid = note.Player.Family.Value.GetStringOrDefault("FamilyID");
                    if (!string.IsNullOrEmpty(fid))
                    {
                        p.Value["LevF"] = GetLevF(fid);//TopAccess.Instance.GetMyFamilyRank(note.Player);
                    }
                    note.Call(PaiHangCommand.GetMyRankR, p.Value);
                    return;
                }
            }

            string fid2 = note.Player.Family.Value.GetStringOrDefault("FamilyID");
            int familRank = GetLevF(fid2);// TopAccess.Instance.GetMyFamilyRank(note.Player);
            if (familRank > 0)
            {
                Variant v = new Variant(1);
                v["LevF"] = familRank;
                note.Call(PaiHangCommand.GetMyRankR, v);
            }
            else
            {
                note.Call(PaiHangCommand.GetMyRankR, new object[] { null });
            }
        }

        static int GetLevF(string fid)
        {
            if (string.IsNullOrEmpty(fid))
            {
                return 0;
            }
            Family model = FamilyAccess.Instance.FindOneById(fid);
            int sort = FamilyAccess.Instance.FamilySort(model.Value.GetIntOrDefault("Level"), model.Modified, model.Created);
            return sort;//TopAccess.Instance.GetMyFamilyRank(note.Player);
        }
        #endregion

    }
}
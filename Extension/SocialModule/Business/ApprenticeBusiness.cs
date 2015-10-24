using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Observer;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Sinan.SocialModule.Business
{
    class ApprenticeBusiness
    {
        /// <summary>
        /// 收徒申请
        /// </summary>
        /// <param name="note"></param>
        //public static void ApprenticeApply(UserNote note)
        //{
        //    string name = note.GetString(0);

        //    if (name == note.Player.Name) 
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.NoSelf));
        //        return;
        //    }

        //    //徒弟信息
            
        //    Player PlayerBase = PlayerAccess.Instance.GetPlayerInfo(name);
        //    if (PlayerBase == null)
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.UserInfoError));
        //        return;
        //    }


        //    if (PlayerBase.Level < 20 || (note.Player.Level - PlayerBase.Level <3) || note.Player.Level < 40)
        //    {
        //        note.Call(SocialCommand.ApprenticeApplyR, false, TipAccess.GetMessage(SocialReturn.MasterLevelGap));
        //        return;
        //    }


        //    PlayerBusiness OnLinePlayer;
        //    if (!PlayersProxy.TryGetPlayerByID(PlayerBase.ID, out OnLinePlayer))
        //    {
        //        ///没有在线不能收徒
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.NoOnLine));
        //        return;
        //    }

        //    ///徒弟信息
        //    PlayerEx Social = OnLinePlayer.Value["Social"] as PlayerEx;
        //    if (SocialBusiness.IsLet(Social, note.PlayerID, new List<string> { "Enemy" }))
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Enemy));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(Social, note.PlayerID, new List<string> { "Master" }))
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Master));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(Social, note.PlayerID, new List<string> { "Apprentice" }))
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Apprentice));
        //        return;
        //    }

        //    dynamic AppMenter = Social.Dynamic.Mentor as Variant;


        //    if (AppMenter.Master != null)
        //    {
        //        if (AppMenter.Master.Count != 0)
        //        {
        //            //已经存在师傅，不能收为徒弟
        //            note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Master));
        //            return;
        //        }
        //    }

        //    if (AppMenter.FreezeDate != null)
        //    {
        //        //是否是冻结期
        //        if (AppMenter.FreezeDate > DateTime.UtcNow)
        //        {
        //            note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.NoFreezeDate));
        //            return;
        //        }
        //    }


        //    ///师傅信息
        //    PlayerEx MasterSocial = note.Player.Value["Social"] as PlayerEx;
        //    if (SocialBusiness.IsLet(MasterSocial, PlayerBase.ID, new List<string> { "Enemy" }))
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Enemy));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(MasterSocial, PlayerBase.ID, new List<string> { "Master" }))
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Master));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(MasterSocial, PlayerBase.ID, new List<string> { "Apprentice" }))
        //    {
        //        note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.Apprentice));
        //        return;
        //    }
        //    dynamic MasterMentor = MasterSocial.Dynamic.Mentor as Variant;

        //    if (MasterMentor.FreezeDate != null)
        //    {
        //        //冻结期
        //        if (MasterMentor.FreezeDate > DateTime.UtcNow)
        //        {
        //            note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.NoFreezeDate));
        //            return;
        //        }
        //    }

        //    if (note.Player.Value.ContainsKey("Apprentice"))
        //    {
        //        Dictionary<string, DateTime> dic = note.Player.Value["Apprentice"] as Dictionary<string, DateTime>;
        //        if (dic.ContainsKey(note.PlayerID)) 
        //        {
        //            note.Call(SocialCommand.MasterApplyR, false, TipAccess.GetMessage(SocialReturn.IsApprentice));
        //            return;
        //        }
        //        dic.Add(note.PlayerID,DateTime.UtcNow);
        //    }
        //    else
        //    {
        //        Dictionary<string, DateTime> dic = new Dictionary<string, DateTime>();
        //        dic.Add(note.PlayerID, DateTime.UtcNow);
        //        note.Player.Value.Add("Apprentice", dic);
        //    }

        //    //通知被申请的人，有人要收你为徒，你是否能够同意
        //    OnLinePlayer.Call(SocialCommand.ApprenticeApplyR, true, note.PlayerID);
        //    //通知申请人，申请已经发出
        //    note.Call(SocialCommand.ApprenticeApplyR, true, TipAccess.GetMessage(SocialReturn.ApprenticeApply));
        //}

        ///// <summary>
        ///// 收徒申请回复
        ///// </summary>
        ///// <param name="note"></param>
        //public static void ApprenticeBack(UserNote note)
        //{
        //    bool isSame = false;
        //    Boolean.TryParse(note.GetString(0), out isSame);
        //    string playerid = note.GetString(1);

        //    //师傅信息

        //    Player PlayerBase = PlayerAccess.Instance.FindOneById(playerid);
        //    if (PlayerBase == null)
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.UserInfoError));
        //        return;
        //    }


        //    if (note.Player.Level < 20 || (PlayerBase.Level - note.Player.Level < 3) || PlayerBase.Level < 40)
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.MasterLevelGap));
        //        return;
        //    }

        //    ///师傅是否在线
        //    PlayerBusiness OnLinePlayer;
        //    if (!PlayersProxy.TryGetPlayerByID(PlayerBase.ID, out OnLinePlayer))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.NoOnLine));
        //        return;
        //    }

        //    Dictionary<string, DateTime> dic = OnLinePlayer.Value["Apprentice"] as Dictionary<string, DateTime>;
        //    if (dic == null || (!dic.ContainsKey(playerid))) 
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.ApprenticeBackError));
        //        return;
        //    }
        //    dic.Remove(playerid);

        //    if (!isSame)
        //    {
        //        OnLinePlayer.Call(SocialCommand.MasterBackR, false, TipAccess.GetMessage(SocialReturn.MasterDeny));
        //        return;
        //    }

        //    PlayerEx MasSocial = OnLinePlayer.Value["Social"] as PlayerEx;
        //    if (SocialBusiness.IsLet(MasSocial, note.PlayerID, new List<string> { "Enemy" }))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Enemy));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(MasSocial, note.PlayerID, new List<string> { "Master" }))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Master));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(MasSocial, note.PlayerID, new List<string> { "Apprentice" }))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Apprentice));
        //        return;
        //    }

        //    dynamic MasterMenter = MasSocial.Dynamic.Mentor as Variant;


        //    if (MasterMenter.FreezeDate != null)
        //    {
        //        //是否是冻结期
        //        if (MasterMenter.FreezeDate > DateTime.UtcNow)
        //        {
        //            note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.NoFreezeDate));
        //            return;
        //        }
        //    }


        //    ///徒弟信息
        //    PlayerEx AppSocial = note.Player.Value["Social"] as PlayerEx;
        //    if (SocialBusiness.IsLet(AppSocial, OnLinePlayer.ID, new List<string> { "Enemy" }))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Enemy));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(AppSocial, OnLinePlayer.ID, new List<string> { "Master" }))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Master));
        //        return;
        //    }
        //    if (SocialBusiness.IsLet(AppSocial, OnLinePlayer.ID, new List<string> { "Apprentice" }))
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Apprentice));
        //        return;
        //    }


        //    dynamic AppMenter = AppSocial.Dynamic.Mentor as Variant;

        //    if (AppMenter.Master != null)
        //    {
        //        if (AppMenter.Master.Count != 0)
        //        {
        //            //已经存在师傅，不能收为徒弟
        //            note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Master));
        //            return;
        //        }
        //    }

        //    if (AppMenter.FreezeDate != null)
        //    {
        //        //是否是冻结期
        //        if (AppMenter.FreezeDate > DateTime.UtcNow)
        //        {
        //            note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.NoFreezeDate));
        //            return;
        //        }
        //    }

        //    if (AppMenter.Master != null)
        //    {
        //        note.Call(SocialCommand.ApprenticeBackR, false, TipAccess.GetMessage(SocialReturn.Master));
        //        return;
        //    }

        //    ///添加师傅
        //    Variant mast = new Variant();
        //    mast.Add("PlayerID", OnLinePlayer.ID);
        //    mast.Add("Created", DateTime.UtcNow);
        //    if (AppMenter.Master == null || AppMenter.Master.Count == 0)
        //    {
        //        AppMenter.Master = new List<Variant>() { mast };
        //    }
        //    else
        //    {
        //        AppMenter.Master.Add(mast);
        //    }

        //    ///添加徒弟
        //    Variant app = new Variant();
        //    app.Add("PlayerID", note.PlayerID);
        //    app.Add("Created", DateTime.UtcNow);
        //    if (MasterMenter.Apprentice == null || MasterMenter.Apprentice.Count == 0)
        //    {
        //        MasterMenter.Apprentice = new List<Variant>() { app };
        //    }
        //    else
        //    {
        //        MasterMenter.Apprentice.Add(app);
        //    }

        //    MasSocial.Save();
        //    AppSocial.Save();


        //    Variant master = new Variant();
        //    master.Add("Master", SocialBusiness.SocialInfo(MasterMenter.Apprentice));
        //    OnLinePlayer.Call(SocialCommand.ApprenticeBackR, true, master);

        //    Variant apprentice = new Variant();
        //    apprentice.Add("Apprentice",SocialBusiness.SocialInfo(MasterMenter.Master));
        //    note.Call(SocialCommand.ApprenticeBackR, true, apprentice);
        //}
    }
}

using System;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 创建队伍
    /// </summary>
    partial class FightBase
    {
        static FightObject[] EmptyFightObject = new FightObject[0];

        /// <summary>
        /// 创建APC的B队
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static FightObject[] CreateApcTeam(List<PlayerBusiness> players, FightType fightType, object obj)
        {
            Dictionary<int, string> apcPostions = null;
            switch (fightType)
            {
                case FightType.HideAPC:
                case FightType.SceneAPC:
                case FightType.RobAPC:
                case FightType.ProAPC:
                case FightType.EctypeApc:
                    VisibleApc c = obj as VisibleApc;
                    if (c == null)
                    {
                        return EmptyFightObject;
                    }
                    // 生成战斗怪位置/ID
                    int minCount = (int)(players.Count * c.MinCount);
                    int count = (int)(players.Count * c.MaxCount) + 1;
                    if (count > minCount)
                    {
                        count = NumberRandom.Next(minCount, count);
                    }
                    else
                    {
                        count = minCount;
                    }
                    if (fightType == FightType.HideAPC)
                    {
                        foreach (var player in players)
                        {
                            count += player.GetAddHideApc();
                        }
                    }
                    count = Math.Min(10, count);
                    apcPostions = c.CreateApc(count);
                    break;

                case FightType.TaskAPC:
                case FightType.FamilyBoss:
                    apcPostions = new Dictionary<int, string>(10);
                    Variant task = obj as Variant;
                    if (task != null)
                    {
                        int p;
                        foreach (var v in task)
                        {
                            if (int.TryParse(v.Key.Substring(1), out p))
                            {
                                apcPostions.Add(p, v.Value.ToString());
                            }
                        }
                    }
                    break;
            }
            if (apcPostions != null && apcPostions.Count > 0)
            {
                return CreateApcs(apcPostions);
            }
            return EmptyFightObject;
        }

        private static FightObject[] CreateApcs(Dictionary<int, string> apcPostions)
        {
            List<FightObject> teamB = new List<FightObject>();
            foreach (var item in apcPostions)
            {
                string apcID = item.Value;
                Apc apc = ApcManager.Instance.FindOne(apcID);
                if (apc != null)
                {
                    FightObject f = null;
                    if (apc.ApcType == FighterType.BB)
                    {
                        string petid = apc.Value.GetStringOrDefault("PetID");
                        if (!string.IsNullOrEmpty(petid))
                        {
                            f = new FightBB(item.Key, apc);
                        }
                    }
                    else if (apc.ApcType == FighterType.Boss)
                    {
                        f = new FightBos(item.Key, apc);
                    }
                    if (f == null)
                    {
                        f = new FightApc(item.Key, apc);
                    }
                    teamB.Add(f);
                }
            }
            return teamB.ToArray();
        }

        /// <summary>
        /// 创建玩家队伍
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static FightObject[] CreateFightPlayers(List<PlayerBusiness> players)
        {
            List<FightObject> team = new List<FightObject>();
            for (int i = 0; i < players.Count; i++)
            {
                PlayerBusiness player = players[i];
                player.SetActionState(ActionState.Fight);
                player.FightTime = DateTime.UtcNow;
                FightPlayer f = new FightPlayer(i, player);
                team.Add(f);
                if (player.Pet != null)
                {
                    FightPet pet = new FightPet(i + 5, player);
                    team.Add(pet);
                }
            }
            return team.ToArray();
        }

        /// <summary>
        /// 获取战斗玩家
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static List<PlayerBusiness> GetPlayers(PlayerBusiness player)
        {
            List<PlayerBusiness> players = new List<PlayerBusiness>(5);
            if (player.Fight != null)
            {
                return players;
            }
            players.Add(player);
            PlayerTeam team = player.Team;
            if (team != null && (player.TeamJob == TeamJob.Captain || player.TeamJob == TeamJob.Member))
            {
                for (int i = 0; i < team.Members.Length; i++)
                {
                    PlayerBusiness member = team.Members[i];
                    if (member != null && member.Fight == null && member != player
                        && (member.TeamJob == TeamJob.Captain || member.TeamJob == TeamJob.Member))
                    {
                        players.Add(member);
                    }
                }
            }
            return players;
        }
    }
}

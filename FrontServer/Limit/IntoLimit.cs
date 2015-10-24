using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Data;
using Sinan.Extensions.FluentDate;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 进入限制
    /// </summary>
    public class IntoLimit
    {
        List<ILimit> list = new List<ILimit>();

        public bool IsEmpty
        {
            get
            {
                return list.Count == 0;
            }
        }


        public IntoLimit(string ectypeID, string name, Variant v)
        {
            this.AddLimit(LimitLevel.Create(name, v));
            this.AddLimit(LimitOpentime.Create(name, v));
            this.AddLimit(LimitMaxinto.Create(ectypeID, name, v));
            this.AddLimit(LimitTeam.Create(name, v));
            //this.AddLimit(LimitFamily.Create(name, v));
            this.AddLimit(LimitGoods.Create(name, v));
            this.AddLimit(LimitScore.Create(name, v));
        }

        public bool AddLimit(ILimit limit)
        {
            if (limit == null || list.Contains(limit))
            {
                return false;
            }
            list.Add(limit);
            return true;
        }

        /// <summary>
        /// 进入验证
        /// </summary>
        /// <param name="members"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool IntoCheck(PlayerBusiness[] members, out string msg, out PlayerBusiness member)
        {
            foreach (var limit in list)
            {
                for (int i = 0; i < members.Length; i++)
                {
                    member = members[i];
                    if (member != null)
                    {
                        if (!limit.Check(member, out msg))
                        {
                            return false;
                        }
                    }
                }
            }
            member = null;
            msg = null;
            return true;
        }

        /// <summary>
        /// 进入(扣除)
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public bool IntoDeduct(PlayerBusiness[] members, out string msg, out PlayerBusiness member)
        {
            for (int t = 0; t < list.Count; t++)
            {
                ILimit limit = list[t];
                for (int i = 0; i < members.Length; i++)
                {
                    member = members[i];
                    if (member != null)
                    {
                        if (!limit.Execute(member, out msg))
                        {
                            Rollback(t, i, members);
                            return false;
                        }
                    }
                }
            }
            member = null;
            msg = null;
            return true;
        }

        void Rollback(int maxT, int maxM, PlayerBusiness[] members)
        {
            for (int t = 0; t < maxT; t++)
            {
                ILimit limit = list[t];
                for (int i = 0; i < members.Length; i++)
                {
                    PlayerBusiness member = members[i];
                    if (member != null)
                    {
                        limit.Rollback(member);
                    }
                }
            }

            ILimit limit2 = list[maxT];
            for (int i = 0; i < maxM; i++)
            {
                PlayerBusiness member = members[i];
                if (member != null)
                {
                    limit2.Rollback(member);
                }
            }
        }

        /// <summary>
        /// 进入检查
        /// </summary>
        /// <param name="members"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool IntoCheck(PlayerBusiness member, out string msg)
        {
            foreach (var limit in list)
            {
                if (member != null)
                {
                    if (!limit.Check(member, out msg))
                    {
                        return false;
                    }
                }
            }
            msg = null;
            return true;
        }

        /// <summary>
        /// 进入(扣除)
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public bool IntoDeduct(PlayerBusiness member, out string msg)
        {
            for (int t = 0; t < list.Count; t++)
            {
                ILimit limit = list[t];
                if (member != null)
                {
                    if (!limit.Execute(member, out msg))
                    {
                        Rollback(t, member);
                        return false;
                    }
                }
            }
            msg = null;
            return true;
        }

        void Rollback(int maxT, PlayerBusiness member)
        {
            for (int t = 0; t < maxT; t++)
            {
                ILimit limit = list[t];
                if (member != null)
                {
                    limit.Rollback(member);
                }
            }
        }
    }
}

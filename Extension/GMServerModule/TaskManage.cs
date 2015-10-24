using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.GMServerModule
{
    public class TaskManage
    {
        /// <summary>
        /// 任务触发d
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="gc"></param>
        public static void TaskBack(PlayerBusiness pb, GameConfig gc)
        {
            Task task = TaskAccess.Instance.TaskBase(pb.ID, pb.Name, gc, 0, 0);
            if (task != null)
            {
                pb.Call(TaskCommand.TaskActivationR, TaskAccess.Instance.GetTaskInfo(task));
            }
        }

        /// <summary>
        /// 任务放异
        /// </summary>
        /// <param name="note">角色信息</param>
        /// <param name="task">任务</param>
        /// <returns></returns>
        public static bool TaskGiveup(PlayerBusiness pb, Task task, bool isremove = true)
        {
            PlayerEx burden = pb.B0;
            Variant v = task.Value;
            //完成条件
            IList finish = v["Finish"] as IList;
            foreach (Variant k in finish)
            {
                string goodsid = k.GetStringOrDefault("GoodsID");
                int Cur = k.GetIntOrDefault("Cur");
                if (Cur == 0) continue;
                switch (k.GetIntOrDefault("Type"))
                {
                    case 10006:
                        Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                        Variant vn = new Variant();
                        vn.Add("Number1", Cur);
                        dic.Add(goodsid, vn);
                        if (BurdenManager.IsFullBurden(burden, dic))
                            return true;
                        pb.AddGoods(dic, GoodsSource.TaskGiveup);
                        break;
                    case 10003:
                    case 10009:
                    case 10010:
                        BurdenManager.Remove(burden, goodsid, Cur);
                        break;
                }
                k["Cur"] = 0;
            }
            v["Status"] = 0;
            if (isremove)
            {
                task.Save();
            }
            pb.Call(TaskCommand.GiveupR, true, task.ID);
            return false;
        }
    }
}

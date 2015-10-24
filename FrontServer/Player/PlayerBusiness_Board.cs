using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.GameModule;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 公告
    /// </summary>
    partial class PlayerBusiness
    {
        /// <summary>
        /// 添加公告
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddBoard(string value)
        {
            //被吸方写日记
            bool x = BoardAccess.Instance.AddNote(PID, value);
            if (x)
            {
                this.Call(HomeCommand.AddBoard, value);
            }
            return x;
        }

        /// <summary>
        /// 删除公告
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RemoveBoard(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BoardAccess.Instance.ClearNotes(PID);
            }
            else
            {
                return BoardAccess.Instance.RemoveNote(PID, value);
            }
        }
    }
}

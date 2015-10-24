using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Sinan.AMF3;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 活动明雷
    /// </summary>
    public class ActiveApc : ExternalizableBase
    {
        public int ID;

        /// <summary>
        /// 出现的位置X
        /// </summary>
        public int X;
        public int Y;

        /// <summary>
        /// 状态.
        /// 0:可攻击
        /// 1:战斗中
        /// 2:死亡
        /// </summary>
        public int State;

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("X");
            writer.WriteInt(X);
            writer.WriteKey("Y");
            writer.WriteInt(Y);

            writer.WriteKey("ID");
            writer.WriteInt(ID);
        }
    }

}

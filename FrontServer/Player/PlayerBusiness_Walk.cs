using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;


namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        /// <summary>
        /// 禁止发言
        /// </summary>
        [BsonIgnore]
        public DateTime Danned
        {
            get;
            set;
        }

        byte[] m_walk;
        void InitWalk()
        {
            Sinan.Collections.BytesSegment temp = AmfCodec.Encode(ClientCommand.WalkToR, new object[] { PID, double.Epsilon });
            m_walk = new byte[temp.Count];
            Buffer.BlockCopy(temp.Array, temp.Offset, m_walk, 0, m_walk.Length);
        }

        /// <summary>
        /// 行走
        /// </summary>
        /// <param name="point"></param>
        unsafe internal Sinan.Collections.BytesSegment WalkTo(double point)
        {
            m_point = point;
            X = (int)((((long)m_point) >> 8) & 0xff);
            Y = (int)(((long)m_point) & 0xff);

            long v = *((long*)&point);
            int index = m_walk.Length - 8;
            for (int i = 7; i >= 0; i--)
            {
                m_walk[index++] = (byte)(v >> (i << 3));
            }
            return new Sinan.Collections.BytesSegment(m_walk);
        }

        /// <summary>
        /// 保存所在基本场景的信息.
        /// </summary>
        public void SaveBaseScine()
        {
            m_ectype.Value["SceneID"] = this.SceneID;
            m_ectype.Value["X"] = this.X;
            m_ectype.Value["Y"] = this.Y;
            m_ectype.Save();
        }

        /// <summary>
        /// 重置玩家所在场景信息
        /// </summary>
        public string ResetScene()
        {
            string sceneID = m_ectype.Value.GetStringOrDefault("SceneID");
            if (string.IsNullOrEmpty(sceneID))
            {
                sceneID = SceneCity.DefaultID;
            }
            this.SceneID = sceneID;
            this.X = m_ectype.Value.GetIntOrDefault("X");
            this.Y = m_ectype.Value.GetIntOrDefault("Y");
            return sceneID;
        }

        /// <summary>
        /// 判断是否是有效距离
        /// </summary>
        /// <param name="point"></param>
        /// <param name="sumOfSquares"></param>
        /// <returns></returns>
        public bool EffectDist(Point point, int sumOfSquares)
        {
            int x = point.X - X;
            int y = point.Y - Y;
            return x * x + y * y < sumOfSquares;
        }

        /// <summary>
        /// 判断是否是有效行为
        /// </summary>
        /// <param name="npcid"></param>
        /// <param name="activeed"></param>
        /// <returns></returns>
        public bool EffectActive(string npcid, string activeed)
        {
            //Npc npc = NpcManager.Instance.FindOne(npcid);
            //if (npc == null)
            //{
            //    ///this.Call();
            //    return false;
            //}
            ////场景检查
            //if (npc.SceneID != this.SceneID)
            //{
            //    //
            //    return false;
            //}
            ////距离检查
            //int x = npc.X - X;
            //int y = npc.Y - Y;
            //if (x * x + y * y > 50)
            //{
            //    //
            //    return false;
            //}
            //功能检查:
            return true;
        }
    }
}

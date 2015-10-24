using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// PlayerSimple
    /// (简单的玩家类.用户取自己的列家列表时使用)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PlayerSimple : ExternalizableBase
    {
        int m_style;
        Player m_player;

        /// <summary>
        /// 简单的玩家类
        /// </summary>
        /// <param name="player"></param>
        /// <param name="style">
        /// style=0:登录时用,
        /// style=1:玩家等级排行,
        /// style=2:成就排行
        /// style=3:得到社会基础信息
        /// </param>
        public PlayerSimple(Player player, int style = 0)
        {
            m_style = style;
            this.m_player = player;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            m_player.WriteBase(writer);
            if (m_style == 0)
            {
                writer.WriteKey("Coat");
                writer.WriteUTF(m_player.Coat);
                writer.WriteKey("Weapon");
                writer.WriteUTF(m_player.Weapon);
                writer.WriteKey("Body");
                writer.WriteUTF(m_player.Body);
                writer.WriteKey("Mount");
                writer.WriteUTF(m_player.Mount);

                writer.WriteKey("SceneID");
                writer.WriteUTF(m_player.SceneID);

                writer.WriteKey("State");
                writer.WriteInt(m_player.State);
                if (m_player.State == 2)
                {
                    writer.WriteKey("DelTime");
                    writer.WriteDateTime(m_player.Modified.AddSeconds(GetRetainSecond(m_player.Level)));
                }
                return;
            }

            if (m_style == 2)
            {
                writer.WriteKey("Dian");
                writer.WriteInt(m_player.Dian);
            }

            writer.WriteKey("FamilyName");
            writer.WriteUTF(m_player.FamilyName);
            writer.WriteKey("FamilyJob");
            writer.WriteUTF(m_player.FamilyJob);

            writer.WriteKey("Online");
            writer.WriteBoolean(m_player.Online);
        }


        /// <summary>
        /// 删除角色时的保留时间(秒)
        /// </summary>
        /// <param name="level">角色等级</param>
        /// <returns></returns>
        public static int GetRetainSecond(int level)
        {
            if (level < 20) return 0;
            if (level < 30) return 60 * 60 * 24;
            if (level < 40) return 60 * 60 * 24 * 2;
            return 60 * 60 * 24 * 3;
        }
    }
}

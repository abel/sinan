using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// PlayerDetail
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PlayerDetail : ExternalizableBase
    {
        int m_style;
        IList m_keys;
        PlayerBusiness m_player;

        /// <summary>
        /// 玩家详细信息
        /// </summary>
        /// <param name="player"></param>
        /// <param name="style">
        /// 0:返回组队数据,
        /// 1:取玩家详细信息,
        /// 2:玩家登录成功时取自己的信息
        /// 3:用户自定义取扩展
        /// </param>
        public PlayerDetail(PlayerBusiness player, int style = 0)
        {
            this.m_player = player;
            m_style = style;
        }

        /// <summary>
        /// 玩家详细信息
        /// </summary>
        /// <param name="player"></param>
        /// <param name="keys">3:用户自定义取扩展</param>
        public PlayerDetail(PlayerBusiness player, IList keys)
        {
            this.m_player = player;
            m_style = 3;
            m_keys = keys;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            if (m_style == 3 && m_keys != null)
            {
                foreach (string key in m_keys)
                {
                    PlayerEx v = m_player.Value.GetValue<PlayerEx>(key);
                    writer.WriteKey(key);
                    writer.WriteIDictionary(v.Value);
                }
                writer.WriteKey("ID");
                writer.WriteUTF(m_player.ID);
                return;
            }

            m_player.WriteBase(writer);
            m_player.WriteShape(writer);
            m_player.WritePet(writer);
            //组队时用
            if (m_style == 0)
            {
                writer.WriteKey("TeamJob");
                writer.WriteInt((int)(m_player.TeamJob));

                writer.WriteKey("JingYan");
                MVPair.WritePair(writer, m_player.MaxExp, m_player.Experience);
                writer.WriteKey("ShengMing");
                MVPair.WritePair(writer, m_player.Life.ShengMing, m_player.HP);
                writer.WriteKey("MoFa");
                MVPair.WritePair(writer, m_player.Life.MoFa, m_player.MP);
            }
            //取玩家详细信息
            else if (m_style == 1)
            {
                if (m_player.Pet != null)
                {
                    writer.WriteKey("PetID");
                    writer.WriteUTF(m_player.Pet.ID);
                }

                m_player.WriteFightProperty(writer);
                m_player.WriteOther(writer);
                writer.WriteKey("Equips");
                writer.WriteIDictionary(m_player.Equips.Value);
            }
            //玩家登录成功时取自己的信息
            else if (m_style == 2)
            {
                m_player.WriteScene(writer);
                m_player.WriteFinance(writer);
                m_player.WritePlayerEx(writer);
                m_player.WriteFightProperty(writer);
                m_player.WriteOther(writer);
                writer.WriteKey("PID");
                writer.WriteInt(m_player.PID);
            }
        }

    }
}

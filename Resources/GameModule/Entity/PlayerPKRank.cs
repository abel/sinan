using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;

namespace Sinan.Entity
{
    /// <summary>
    /// PlayerSimple
    /// (简单的玩家类.用户取自己的列家列表时使用)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PlayerPKRank : ExternalizableBase
    {
        int m_winCount;
        Player m_player;

        /// <summary>
        /// 玩家类PK排行
        /// </summary>
        /// <param name="player"></param>
        /// <param name="winCount">
        /// </param>
        public PlayerPKRank(Player player, int winCount)
        {
            m_winCount = winCount;
            this.m_player = player;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            m_player.WriteBase(writer);


            writer.WriteKey("WinCount");
            writer.WriteInt(m_winCount);

            writer.WriteKey("FamilyName");
            writer.WriteUTF(m_player.FamilyName);
            writer.WriteKey("FamilyJob");
            writer.WriteUTF(m_player.FamilyJob);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// PlayerExDetail
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PlayerExDetail : ExternalizableBase
    {
        PlayerEx m_playerEx;

        public PlayerExDetail(PlayerEx playerEx)
        {
            this.m_playerEx = playerEx;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey(m_playerEx.Name);
            writer.WriteIDictionary(m_playerEx.Value);

            writer.WriteKey("ID");
            writer.WriteUTF(m_playerEx.PlayerID);
        }
    }
}

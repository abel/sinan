using Sinan.AMF3;
using Sinan.ArenaModule.Business;

namespace Sinan.ArenaModule.Detail
{
    /// <summary>
    /// [Serializable]
    /// [BsonIgnoreExtraElementsAttribute]
    /// </summary>
    public class ArenaDetail : ExternalizableBase
    {
        int m_style;
        ArenaBase m_ab;
        public ArenaDetail(ArenaBase ab, int style = 0)
        {
            m_ab = ab;
            m_style = style;
        }


        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("SoleID");
            writer.WriteUTF(m_ab.SoleID);

            writer.WriteKey("PlayerID");
            writer.WriteUTF(m_ab.PlayerID);

            writer.WriteKey("Name");
            writer.WriteUTF(m_ab.Name);

            writer.WriteKey("WarType");
            writer.WriteInt(m_ab.WarType);

            writer.WriteKey("IsWatch");
            writer.WriteBoolean(m_ab.IsWatch);

            writer.WriteKey("WinType");
            writer.WriteInt(m_ab.WinType);

            writer.WriteKey("IsOtherInto");
            writer.WriteBoolean(m_ab.IsOtherInto);

            writer.WriteKey("PetNumber");
            writer.WriteInt(m_ab.PetNumber);

            writer.WriteKey("PetLevel");
            writer.WriteUTF(m_ab.PetMin + "-" + m_ab.PetMax);
            writer.WriteKey("Group");
            writer.WriteInt(m_ab.Group);

            writer.WriteKey("Scene");
            writer.WriteUTF(m_ab.Scene);

            writer.WriteKey("ArenaID");
            writer.WriteUTF(m_ab.ArenaID);

            writer.WriteKey("IsGoods");
            writer.WriteBoolean(m_ab.IsGoods);

            writer.WriteKey("StartTime");
            writer.WriteDateTime(m_ab.StartTime);

            writer.WriteKey("EndTime");
            writer.WriteDateTime(m_ab.EndTime);

            writer.WriteKey("PetCount");
            writer.WriteInt(m_ab.Pets.Count);

            writer.WriteKey("PrepareTime");
            writer.WriteInt(m_ab.PrepareTime);

            writer.WriteKey("GameTime");
            writer.WriteInt(m_ab.GameTime);

            writer.WriteKey("UserPets");
            writer.WriteInt(m_ab.UserPets);

            //战绩差
            writer.WriteKey("FightPoor");
            writer.WriteInt(m_ab.FightPoor);
            //基本战绩
            writer.WriteKey("FightValue");
            writer.WriteDouble((double)m_ab.FightValue);
            //是否要求密码,如果为""或null不要求
            writer.WriteKey("PassWord");
            writer.WriteUTF(m_ab.PassWord);
        }
    }
}

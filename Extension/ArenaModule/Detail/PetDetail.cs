using Sinan.AMF3;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.ArenaModule.Detail
{
    public class PetDetail : ExternalizableBase
    {
        Pet m_pet;
        int m_style = 0;
        int m_level = 0;
        string m_name = "";
        Variant m_fatigue = new Variant(2);
        /// <summary>
        /// 宠物基本信息
        /// </summary>
        /// <param name="pet">宠物信息</param>
        /// <param name="style">0表示明细,1宠物ID</param>
        
        public PetDetail(Pet pet,string name, int style = 0)
        {
            m_pet = pet;
            m_style = style;
            m_name = name;
            m_level = pet.Value.GetIntOrDefault("PetsLevel");

            m_fatigue.Add("V", pet.Value.GetIntOrDefault("Fatigue"));
            m_fatigue.Add("M", PetAccess.Instance.MaxFatigue(m_level));
        }


        protected override void WriteAmf3(IExternalWriter writer)
        {
            Variant v = m_pet.Value;
            writer.WriteKey("ID");
            writer.WriteUTF(m_pet.ID);
            if (m_style != 2)
            {
                writer.WriteKey("PlayerID");
                writer.WriteUTF(m_pet.PlayerID);
            }

            if (m_style == 0)
            {
                writer.WriteKey("PlayerName");
                writer.WriteUTF(m_name);

                writer.WriteKey("CurSkill");
                writer.WriteUTF(m_pet.CurSkill);

                writer.WriteKey("Name");
                writer.WriteUTF(m_pet.Name);
                writer.WriteKey("Skin");
                writer.WriteUTF(v.GetStringOrDefault("Skin"));
                writer.WriteKey("PetsID");
                writer.WriteUTF(v.GetStringOrDefault("PetsID"));

                writer.WriteKey("IsWar");
                writer.WriteBoolean(m_pet.IsWar);

                writer.WriteKey("X");
                writer.WriteInt(m_pet.CurPoint.X);

                writer.WriteKey("Y");
                writer.WriteInt(m_pet.CurPoint.Y);

                writer.WriteKey("PetsRank");
                writer.WriteInt(v.GetIntOrDefault("PetsRank"));

                writer.WriteKey("PetsLevel");
                writer.WriteInt(m_level);

                writer.WriteKey("MoFa");
                writer.WriteValue(v.GetVariantOrDefault("MoFa"));

                writer.WriteKey("ShengMing");
                writer.WriteValue(v.GetVariantOrDefault("ShengMing"));
                    ;
                writer.WriteKey("SkillList");
                writer.WriteValue(v.GetVariantOrDefault("Skill"));

                writer.WriteKey("GroupName");
                writer.WriteUTF(m_pet.GroupName);

                writer.WriteKey("Fatigue");
                writer.WriteValue(m_fatigue);

                writer.WriteKey("FightDeath");
                writer.WriteInt(m_pet.FightDeath.Count);
            }
            if (m_style == 2) 
            {
                writer.WriteKey("FightDeath");
                writer.WriteInt(m_pet.FightDeath.Count);
            }
        }
    }
}

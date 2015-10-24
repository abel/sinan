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
    /// PetSimple
    /// (简单的宠物类)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PetSimple : ExternalizableBase
    {
        int m_style;
        Pet m_pet;

        /// <summary>
        /// 简单的玩家类
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="style">
        /// style=0:,
        /// style=1:宠物等级排行
        /// style=2:魔法和生命
        /// style=3:溜宠物
        /// </param>
        public PetSimple(Pet pet, int style = 0)
        {
            m_style = style;
            this.m_pet = pet;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            if (m_pet == null) return;

            writer.WriteKey("ID");
            writer.WriteUTF(m_pet.ID);

            if (m_style == 2)
            {
                WriteLife(writer);
            }
            else if (m_style == 3)
            {
                writer.WriteKey("Name");
                writer.WriteUTF(m_pet.Name);

                writer.WriteKey("PetsID");
                writer.WriteUTF(m_pet.Value.GetStringOrDefault("PetsID"));

                writer.WriteKey("PetsType");
                writer.WriteUTF(m_pet.Value.GetStringOrDefault("PetsType"));

                writer.WriteKey("PetsRank");
                writer.WriteUTF(m_pet.Value.GetStringOrDefault("PetsRank"));

                writer.WriteKey("PetsLevel");
                writer.WriteInt(m_pet.Value.GetIntOrDefault("PetsLevel"));

                writer.WriteKey("Skin");
                writer.WriteUTF(m_pet.Value.GetStringOrDefault("Skin"));

                writer.WriteKey("Skill");
                writer.WriteValue(m_pet.Value.GetVariantOrDefault("Skill"));

                writer.WriteKey("ZiZhi");
                writer.WriteInt(m_pet.Value.GetIntOrDefault("ZiZhi"));

                WriteLife(writer);
            }
            else  //0,和1
            {
                writer.WriteKey("Name");
                writer.WriteUTF(m_pet.Name);

                writer.WriteKey("Total");
                writer.WriteInt(m_pet.Value.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V"));

                writer.WriteKey("Level");
                writer.WriteInt(m_pet.Value.GetIntOrDefault("PetsLevel"));

                writer.WriteKey("PetsType");
                writer.WriteUTF(m_pet.Value.GetStringOrDefault("PetsType"));

                writer.WriteKey("PlayerName");
                writer.WriteUTF(m_pet.Value.GetStringOrDefault("PlayerName"));
            }
        }

        private void WriteLife(IExternalWriter writer)
        {
            //writer.WriteKey("MoFa");
            //Variant mofa = m_pet.Value.GetVariantOrDefault("MoFa");
            //MVPair.WritePair(writer, mofa.GetIntOrDefault("M"), mofa.GetIntOrDefault("V"));
            //writer.WriteKey("ShengMing");
            //Variant shengming = m_pet.Value.GetVariantOrDefault("ShengMing");
            //MVPair.WritePair(writer, shengming.GetIntOrDefault("M"), shengming.GetIntOrDefault("V"));

            writer.WriteKey("MoFa");
            writer.WriteIDictionary(m_pet.Value.GetVariantOrDefault("MoFa"));

            writer.WriteKey("ShengMing");
            writer.WriteIDictionary(m_pet.Value.GetVariantOrDefault("ShengMing"));
        }
    }
}

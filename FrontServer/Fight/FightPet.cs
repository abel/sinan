using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    public class FightPet : FightPlayer
    {
        Pet m_pet;
        public override FighterType FType
        {
            get { return FighterType.Pet; }
        }

        public FightPet(int p, PlayerBusiness player)
            : base(player)
        {
            this.P = p;
            initPet();
        }

        private void initPet()
        {
            this.m_pet = m_player.Pet;
            this.Name = m_pet.Name;
            this.Level = m_pet.Value.GetIntOrDefault("PetsLevel");
            this.Skin = GetPetSkin(m_pet.Value);
            this.m_hp = m_pet.HP;
            this.m_mp = m_pet.MP;
            this.Life = m_pet.Life;
            this.m_skills = m_pet.Value.GetVariantOrDefault("Skill");
            this.m_fixBuffer = m_pet.FixBuffer;
            this.ID = m_pet.ID;
        }

        private static string GetPetSkin(Variant v)
        {
            string skin = v.GetStringOrDefault("Skin");
            int rank = v.GetIntOrDefault("PetsRank");
            int index = skin.IndexOf('|');
            if (index >= 0)
            {
                string[] skins = skin.Split('|');
                if (rank < skins.Length)
                {
                    return skins[rank];
                }
            }
            return skin + rank.ToString();
        }

        public override void ExitFight(bool changeLife)
        {
            m_online = false;
            if (changeLife)
            {
                //修改HP和MP
                if (this.Level == m_pet.Value.GetIntOrDefault("PetsLevel"))
                {
                    int hp = Math.Max(this.HP, 1);
                    m_pet.SetHPAndHP(hp, m_mp);
                    this.m_player.AutoFullPet();
                    m_pet.Save();
                }
            }
            UpdatePet();
        }

        private void UpdatePet()
        {
            Variant v = new Variant(4);
            v["ShengMing"] = m_pet.Value.GetVariantOrDefault("ShengMing");
            v["MoFa"] = m_pet.Value.GetVariantOrDefault("MoFa");
            v["Experience"] = m_pet.Value.GetVariantOrDefault("Experience");
            v["ID"] = m_pet.ID;
            m_player.Call(PetsCommand.UpdatePetR, true, v);
        }

        /// <summary>
        /// 换宠
        /// </summary>
        /// <param name="changeLife"></param>
        /// <returns></returns>
        public bool ChangePet(bool changeLife)
        {
            //先回写当前宠.
            this.ExitFight(changeLife);
            initPet();
            m_online = true;
            m_buffers.Clear();
            m_action = null;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;

namespace Sinan.Entity
{
    public class PetRank : VariantExternalizable
    {
        public PetRank(Pet pet)
        {
            this.ID = pet.ID;
            this.Name = pet.Name;

            this.Total = pet.Value.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");
            this.Level = pet.Value.GetIntOrDefault("PetsLevel");
            this.PetsType = pet.Value.GetStringOrDefault("PetsType");
            this.Value = pet.Value.GetVariantOrDefault("Topn");
            int pid;
            if (Sinan.Extensions.StringFormat.TryHexNumber(pet.PlayerID, out pid))
            {
                this.PlayerID = pid;
            }
            this.PlayerName = string.Empty;
            //this.PlayerName = pet.Value.GetStringOrDefault("PlayerName");
        }

        public string ID
        {
            get;
            set;
        }

        public int PlayerID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int Total
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public string PetsType
        {
            get;
            set;
        }

        public string PlayerName
        {
            get;
            set;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(ID);

            writer.WriteKey("Name");
            writer.WriteUTF(Name);

            writer.WriteKey("Total");
            writer.WriteInt(Total);

            writer.WriteKey("Level");
            writer.WriteInt(Level);

            writer.WriteKey("PetsType");
            writer.WriteUTF(PetsType);

            writer.WriteKey("PlayerName");
            writer.WriteUTF(PlayerName);
        }
    }
}

using Sinan.AMF3;
using Sinan.ArenaModule.Fight;

namespace Sinan.ArenaModule.Detail
{
    public class PetFightDetail : ExternalizableBase
    {

        FightBase m_fb;
        public PetFightDetail(FightBase fb) 
        { 
            m_fb = fb; 
        }


        protected override void WriteAmf3(IExternalWriter writer)
        {

            writer.WriteKey("ID");
            writer.WriteUTF(m_fb.ID);

            writer.WriteKey("PlayerID");
            writer.WriteUTF(m_fb.PlayerID);

            writer.WriteKey("CurSkill");
            writer.WriteUTF(m_fb.CurSkill);

            writer.WriteKey("FightType");
            writer.WriteInt((int)m_fb.FB);

            writer.WriteKey("RangePet");
            writer.WriteUTF(m_fb.RangePet);

            writer.WriteKey("MPCost");
            writer.WriteInt(m_fb.MPCost);

            writer.WriteKey("HPcost");
            writer.WriteInt(m_fb.HPcost);

            writer.WriteKey("MoFa");
            writer.WriteValue(m_fb.MoFa);

            writer.WriteKey("ShengMing");
            writer.WriteValue(m_fb.ShengMing);


            //Console.WriteLine("Name:"+m_fb.Name+",FightType:" + (int)m_fb.FB + ",RangePet:" + m_fb.RangePet + ",HPcost:" + m_fb.HPcost + ",MPCost:" + m_fb.MPCost + ",MoFa:" + m_fb.MoFa.GetIntOrDefault("V") +",ShengMing:" + m_fb.ShengMing.GetIntOrDefault("V"));
        }
    }
}

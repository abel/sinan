using Sinan.AMF3;
using Sinan.ArenaModule.Fight;

namespace Sinan.ArenaModule.Detail
{
    public class SettleDetail : ExternalizableBase
    {
        private Settle m_model;
        public SettleDetail(Settle model) 
        {
            m_model = model;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("Ranking");
            writer.WriteInt(m_model.Ranking);

            writer.WriteKey("GroupName");
            writer.WriteUTF(m_model.GroupName);

            writer.WriteKey("PlayerName");
            writer.WriteUTF(m_model.PlayerName);

            writer.WriteKey("WinFight");//击杀获得战绩
            writer.WriteInt(m_model.WinFight);
            writer.WriteKey("TotalWin");//击杀数
            writer.WriteInt(m_model.TotalWin);


            writer.WriteKey("LossFight");//损失战绩
            writer.WriteInt(m_model.LossFight);
            writer.WriteKey("TotalLoss");//被杀数
            writer.WriteInt(m_model.TotalLoss);


            writer.WriteKey("OtherFight");//附加战绩
            writer.WriteInt(m_model.OtherFight);

            writer.WriteKey("TotalFight");//总战绩值
            writer.WriteInt(m_model.TotalFight);

            writer.WriteKey("ResultType");//输赢0输,1赢,2平
            writer.WriteInt(m_model.ResultType);

            //Console.WriteLine(
            //    "Ranking:" + m_model.Ranking +
            //    ",GroupName:" + m_model.GroupName +
            //    ",PlayerName:" + m_model.PlayerName +
            //    ",WinFight:" + m_model.WinFight +
            //    ",TotalWin:" + m_model.TotalWin +
            //    ",TotalLoss:" + m_model.TotalLoss +
            //    ",LossFight:" + m_model.LossFight +
            //    ",OtherFight:" + m_model.OtherFight +
            //    ",TotalFight:" + m_model.TotalFight +
            //    ",ResultType:" + m_model.ResultType
            //    );
        }
    }
}

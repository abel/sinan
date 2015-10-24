using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 宝箱
    /// </summary>
    public class Box : ExternalizableBase
    {
        private Variant m_value;
        private Rectangle m_range;

        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 场景ID
        /// </summary>
        public string SceneID
        { get; set; }

        /// <summary>
        /// 开启需要的物品
        /// </summary>
        public string GoodsID
        { get; set; }

        /// <summary>
        /// 读条毫秒数
        /// </summary>
        public int OpenMS
        { get; set; }

        /// <summary>
        /// 消失后再次出现的间隔(秒)
        /// </summary>
        public int GrowSecond
        { get; set; }

        /// <summary>
        /// 单人每天可开启的最大次数
        /// </summary>
        public int MaxOpen
        { get; set; }

        /// <summary>
        /// 皮肤
        /// </summary>
        public string Skin
        { get; set; }

        /// <summary>
        /// 出现范围
        /// </summary>
        public Rectangle Range
        {
            get { return m_range; }
        }

        public Box(Variant config)
        {
            ID = config.GetStringOrDefault("_id");
            Name = config.GetStringOrDefault("Name");
            m_value = config.GetVariantOrDefault("Value");
            if (m_value != null)
            {
                this.SceneID = m_value.GetStringOrDefault("SceneID");
                this.GoodsID = m_value.GetStringOrDefault("GoodsID");
                this.OpenMS = m_value.GetIntOrDefault("OpenMS");
                this.GrowSecond = m_value.GetIntOrDefault("GrowSecond");
                this.MaxOpen = m_value.GetIntOrDefault("MaxOpen");
                m_range = RangeHelper.NewRectangle(m_value.GetVariantOrDefault("Range"), true);
            }
            Variant ui = config.GetVariantOrDefault("UI");
            if (ui != null)
            {
                this.Skin = ui.GetStringOrDefault("Skin");
            }
            if (this.GrowSecond <= 0)
            {
                this.GrowSecond = 30;
            }
        }

        /// <summary>
        /// 开箱成功奖励
        /// </summary>
        /// <returns></returns>
        public AwardBox GetAward()
        {
            int bond = Award.GetAwardCount(m_value.GetVariantOrDefault("Bond"));
            int score = Award.GetAwardCount(m_value.GetVariantOrDefault("Score"));

            AwardBox all = new AwardBox(this.ID);
            Variant award;
            if (m_value.TryGetValueT<Variant>("Award", out award) && award != null)
            {
                Award.GetPackets(award, all.Goods);
            }
            all.Bond = bond;
            all.Score = score;
            return all;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(this.ID);

            writer.WriteKey("Name");
            writer.WriteUTF(this.Name);

            writer.WriteKey("GoodsID");
            writer.WriteUTF(this.GoodsID);

            writer.WriteKey("OpenMS");
            writer.WriteInt(this.OpenMS);

            writer.WriteKey("Skin");
            writer.WriteUTF(this.Skin);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Util;
using Sinan.FastJson;

namespace Sinan.Data
{
    /// <summary>
    /// GameConfig:游戏配置
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    sealed public class GameConfig : VariantExternalizable
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonId]
        public string ID
        { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// 主类型
        /// </summary>
        public string MainType
        { get; set; }

        /// <summary>
        /// 子类型
        /// </summary>
        public string SubType
        { get; set; }

        /// <summary>
        /// 用户界面(JSON对象的字符串)
        /// </summary>
        public Variant UI
        { get; set; }

        /// <summary>
        /// 修改者
        /// </summary>
        public string Author
        { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime Modified
        { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Ver
        { get; set; }

        public ConfigEntity CovertToConfigEntity()
        {
            ConfigEntity c = new ConfigEntity();
            c.ID = this.ID;
            c.Name = this.Name;
            c.MainType = this.MainType;
            c.SubType = this.SubType;
            c.Ver = this.Ver;
            if (this.UI != null)
            {
                c.UI = JsonConvert.SerializeObject(this.UI);
            }
            if (this.Value != null)
            {
                c.Value = JsonConvert.SerializeObject(this.Value);
            }
            return c;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("Value");
            writer.WriteIDictionary(m_value);

            writer.WriteKey("ID");
            writer.WriteUTF(ID);

            writer.WriteKey("Name");
            writer.WriteUTF(Name);

            writer.WriteKey("MainType");
            writer.WriteUTF(MainType);

            writer.WriteKey("SubType");
            writer.WriteUTF(SubType);

            writer.WriteKey("UI");
            writer.WriteIDictionary(UI);

            writer.WriteKey("Ver");
            writer.WriteInt(Ver);
        }
    }
}

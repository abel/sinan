using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Util;
using Sinan.GameModule;

namespace Sinan.Entity
{
    /// <summary>
    /// Goods:实体类
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Goods : SmartVariantEntity
    {
        private Goods() { }
        #region Entity
        /// <summary>
        /// 关联的物品ID
        /// </summary>
        public string GoodsID
        {
            get;
            set;
        }

        /// <summary>
        /// 所有者ID
        /// </summary>
        public string PlayerID
        {
            get;
            set;
        }

        /// <summary>
        /// 是否绑定
        /// 0:绑定
        /// 1:非绑定
        /// </summary>
        public int IsBind
        {
            get;
            set;
        }

        /// <summary>
        /// 标识，主要用来标识装备所处状态0正常，1删除,2正在出售
        /// </summary>
        public int Mode
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }

        #endregion Entity

        /// <summary>
        /// 只写用户登录时需要的信息..
        /// </summary>
        /// <param name="writer"></param>
        protected override void WriteAmf3(IExternalWriter writer)
        {
            base.WriteAmf3(writer);

            writer.WriteKey("ID");
            writer.WriteUTF(ID);
            writer.WriteKey("Name");
            writer.WriteUTF(Name);
            writer.WriteKey("PlayerID");
            writer.WriteUTF(PlayerID);
            writer.WriteKey("IsBind");
            writer.WriteInt(IsBind);
        }

        /// <summary>
        ///  写入玩家基本信息.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public override bool Save()
        {
            this.Changed = false;
            return GoodsAccess.Instance.Save(this);
        }

        public static Goods Create()
        {
            Goods g = new Goods();
            g.m_value = new Variant();
            return g;
        }

        public static Goods Create(Variant v)
        {
            Goods g = new Goods();
            g.m_value = v;
            return g;
        }
    }
}


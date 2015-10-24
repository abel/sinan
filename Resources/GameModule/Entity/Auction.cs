using System;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Util;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.GameModule;
using MongoDB.Bson;


namespace Sinan.Entity
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Auction : SmartVariantEntity
    {                
        public override bool Save()
        {
            return AuctionAccess.Instance.Save(this);
        }
    }
}

using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Sinan.Util;

namespace Sinan.Data
{
    public class Int64UnionBsonSerializer : BsonBaseSerializer
    {
        public static readonly Int64UnionBsonSerializer Instance = new Int64UnionBsonSerializer();

        public override object Deserialize(
         BsonReader bsonReader,
         Type nominalType,
         Type actualType,
         IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.CurrentBsonType;
            if (bsonReader.CurrentBsonType == BsonType.Int64)
            {
                return (Int64Union)bsonReader.ReadInt64();
            }
            else if (bsonType == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                var message = string.Format("Can't deserialize a {0} from BsonType {1}", nominalType.FullName, bsonType);
                throw new FileFormatException(message);
            }
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            bsonWriter.WriteInt64(((Int64Union)value).All);
        }

    }
}
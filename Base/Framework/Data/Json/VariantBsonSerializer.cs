using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Sinan.Util;

namespace Sinan.Data
{
    public class VariantBsonSerializer : BsonBaseSerializer
    {
        public static readonly VariantBsonSerializer Instance = new VariantBsonSerializer();

        public override object Deserialize(
         BsonReader bsonReader,
         Type nominalType,
         Type actualType,
         IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else if (bsonType == BsonType.Document)
            {
                var dictionary = CreateInstance(nominalType);
                bsonReader.ReadStartDocument();
                var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    var key = bsonReader.ReadName();
                    //TODO:对键进行字符串留用.Intern.
                    if (key.Length < 16)
                    {
                        key = String.Intern(key);
                    }
                    var valueType = discriminatorConvention.GetActualType(bsonReader, typeof(object));
                    var valueSerializer = BsonSerializer.LookupSerializer(valueType);
                    var value = valueSerializer.Deserialize(bsonReader, typeof(object), valueType, null);
                    //dictionary.Add(key, value);
                    if (dictionary is IDictionary<string, object>)
                    {
                        ((IDictionary<string, object>)dictionary).Add(key, value);
                    }
                    else if (dictionary is IDictionary)
                    {
                        ((IDictionary)dictionary).Add(key, value);
                    }
                }
                bsonReader.ReadEndDocument();
                return dictionary;
            }
            else
            {
                var message = string.Format("Can't deserialize a {0} from BsonType {1}", nominalType.FullName, bsonType);
                throw new FileFormatException(message);
            }
        }

        private object CreateInstance(
            Type nominalType
        )
        {
            if (nominalType == typeof(IDictionary<string, object>))
            {
                return new Variant();
            }
            else if (nominalType == typeof(Hashtable))
            {
                return new Hashtable();
            }
            else if (nominalType == typeof(ListDictionary))
            {
                return new ListDictionary();
            }
            else if (nominalType == typeof(IDictionary))
            {
                return new Hashtable();
            }
            else if (nominalType == typeof(OrderedDictionary))
            {
                return new OrderedDictionary();
            }
            else if (nominalType == typeof(SortedList))
            {
                return new SortedList();
            }
            else if (nominalType == typeof(object))
            {
                return new Variant();
            }
            else if (nominalType.IsClass)
            {
                return Activator.CreateInstance(nominalType);
            }
            else if (nominalType == typeof(IDictionary))
            {
                return new Dictionary<string, object>();
            }
            else
            {
                var message = string.Format("Invalid nominalType for DictionarySerializer: {0}", nominalType.FullName);
                throw new BsonSerializationException(message);
            }
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            IDictionary<string, object> obj = value as IDictionary<string, object>;
            if (obj == null)
            {
                bsonWriter.WriteNull();
                return;
            }

            bsonWriter.WriteStartDocument();
            foreach (var member in obj)
            {
                bsonWriter.WriteName(member.Key);
                object memberValue = member.Value;

                if (memberValue == null)
                {
                    bsonWriter.WriteNull();
                }
                else
                {
                    nominalType = memberValue.GetType();
                    var serializer = BsonSerializer.LookupSerializer(nominalType);
                    serializer.Serialize(bsonWriter, nominalType, memberValue, options);
                }
            }
            bsonWriter.WriteEndDocument();
        }

    }
}
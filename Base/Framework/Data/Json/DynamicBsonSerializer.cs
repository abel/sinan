using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.Data
{
    public class DynamicBsonSerializer : BsonBaseSerializer
    {
        public static readonly DynamicBsonSerializer Instance = new DynamicBsonSerializer();

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
                    var valueType = discriminatorConvention.GetActualType(bsonReader, typeof(object));
                    var valueSerializer = BsonSerializer.LookupSerializer(valueType);
                    var value = valueSerializer.Deserialize(bsonReader, typeof(object), valueType, null);
                    //dictionary.Add(key, value);
                    //TODO: 添加IDictionary<string, object>
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
            if (nominalType == typeof(Hashtable))
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
                return new ExpandoObject();
            }
            else if (nominalType.IsClass)
            {
                return Activator.CreateInstance(nominalType);
            }
            else if (nominalType == typeof(IDictionary<string, object>))
            {
                return new ExpandoObject();
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
            if (value == null)
            {
                bsonWriter.WriteNull();
                return;
            }
            var metaObject = ((IDynamicMetaObjectProvider)value).GetMetaObject(Expression.Constant(value));
            var memberNames = metaObject.GetDynamicMemberNames().ToList();
            if (memberNames.Count == 0)
            {
                bsonWriter.WriteNull();
                return;
            }

            bsonWriter.WriteStartDocument();
            foreach (var memberName in memberNames)
            {
                bsonWriter.WriteName(memberName);
                var memberValue = BinderHelper.GetMemberValue(value, memberName);
                if (memberValue == null)
                    bsonWriter.WriteNull();
                else
                {
                    var serializer = BsonSerializer.LookupSerializer(memberValue.GetType());
                    serializer.Serialize(bsonWriter, nominalType, memberValue, options);
                }
            }
            bsonWriter.WriteEndDocument();
        }

        private static class BinderHelper
        {
            private static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>> _cache = new ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>>();

            public static object GetMemberValue(object owner, string memberName)
            {
                if (owner is IDictionary<string, object>)
                {
                    object o;
                    if (((IDictionary<string, object>)owner).TryGetValue(memberName, out o))
                    {
                        return o;
                    }
                }
                else if (owner is IDictionary)
                {
                    if (((IDictionary)owner).Contains(memberName))
                    {
                        return ((IDictionary)owner)[memberName];
                    }
                }
                var getSite = _cache.GetOrAdd(
                    memberName,
                    key => CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, key, typeof(BinderHelper), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));

                return getSite.Target(getSite, owner);
            }
        }
    }
}
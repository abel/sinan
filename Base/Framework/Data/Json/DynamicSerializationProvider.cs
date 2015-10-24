using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using Sinan.Util;

namespace Sinan.Data
{
    public class DynamicSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
            {
                return VariantBsonSerializer.Instance;
            }
            //if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            //{
            //    return DynamicBsonSerializer.Instance;
            //}
            // TODO:使用了Int64Union则放开注释
            //if (typeof(Int64Union) == type)
            //{
            //    return Int64UnionBsonSerializer.Instance;
            //}
            return null;
            //return BsonSerializer.LookupSerializer(type);
        }
    }
}
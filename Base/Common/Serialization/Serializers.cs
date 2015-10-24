using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace Sinan.Serialization
{
    /// <summary>
    /// 序列化
    /// </summary>
    public static class Serializers
    {
        /// <summary>
        /// 二进制序列化
        /// </summary>
        /// <param name="request">要序列化的对象</param>
        /// <returns>MemoryStream</returns>
        public static System.IO.MemoryStream SerializeBinary(object request)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            serializer.Serialize(memStream, request);
            return memStream;
        }

        /// <summary>
        /// 反序列化二进制对象
        /// </summary>
        /// <param name="memStream"></param>
        /// <returns></returns>
        public static object DeSerializeBinary(System.IO.MemoryStream memStream)
        {
            memStream.Position = 0;
            BinaryFormatter deserializer = new BinaryFormatter();
            object newobj = deserializer.Deserialize(memStream);
            memStream.Close();
            return newobj;
        }

        #region Binary Serializers
        /// <summary>
        /// 对象序列化为Bin
        /// </summary>
        /// <param name="data">要序列化的对象</param>
        public static byte[] BinSerialize(this object data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }

        /// <summary>
        /// Bin反序列化为对象
        /// </summary>
        /// <param name="data">数据缓冲区</param>
        /// <returns>对象</returns>
        public static object BinDeserialize(this byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream(data);
            return formatter.Deserialize(rems);
        }

        #endregion

        #region Json序列化
        /// <summary>
        /// Json序列化,用于发送到客户端
        /// </summary>
        public static string JsonSerialize(this object item)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(item.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, item);
                string result = Encoding.UTF8.GetString(ms.ToArray());
                return result;
            }
        }

        /// <summary>
        /// Json反序列化,用于接收客户端Json后生成对应的对象
        /// </summary>
        public static T JsonDeserialize<T>(this string jsonString)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                T jsonObject = (T)serializer.ReadObject(ms);
                return jsonObject;
            }
        }
        #endregion

        #region XML Serializers
        /// <summary>
        /// XML 序列化
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static System.IO.MemoryStream SerializeSOAP(object request)
        {
            SoapFormatter serializer = new SoapFormatter();
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            serializer.Serialize(memStream, request);
            return memStream;
        }

        /// <summary>
        /// XML 反序列化
        /// </summary>
        /// <param name="memStream"></param>
        /// <returns></returns>
        public static object DeSerializeSOAP(System.IO.MemoryStream memStream)
        {
            object sr;
            SoapFormatter deserializer = new SoapFormatter();
            memStream.Position = 0;
            sr = deserializer.Deserialize(memStream);
            memStream.Close();
            return sr;
        }
        #endregion
      
        #region XML文件写DataSet
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fileName"></param>
        public static void WriteXml(DataSet ds, string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            ds.WriteXml(fileName, XmlWriteMode.WriteSchema);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DataSet ReadXml(string fileName)
        {
            if (File.Exists(fileName))
            {
                //创建XmlTextReader对象
                XmlTextReader xReader = new XmlTextReader(fileName);
                //创建一个新的数据集
                DataSet ds = new DataSet();
                //从filexml中读出数据集
                ds.ReadXml(xReader, XmlReadMode.Auto);
                return ds;
            }
            return null;
        }       
        #endregion
    }
}

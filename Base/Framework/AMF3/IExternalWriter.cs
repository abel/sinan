using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;

namespace Sinan.AMF3
{
    public interface IExternalWriter : IDataOutput
    {
        void WriteKey(string key);

        /// <summary>
        /// 写AMF3对象.不清空三个缓存.用于自定义方式写入对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool WriteValue(object data);
        bool WriteReference(object value);
        bool WriteIDictionary(IDictionary<string, int> value);
        bool WriteIDictionary(IDictionary<string, object> value);
    }
}


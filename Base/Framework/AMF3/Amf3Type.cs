using System;
using System.Collections.Generic;
using System.Text;

namespace Sinan.AMF3
{
    /// <summary>
    /// AMF3 data types.
    /// </summary>
    public class Amf3Type
    {
        internal const long UnixEpochTicks = 621355968000000000;
        /// <summary>
        /// UTC 1970年1月1日(UnixTime起始时间)
        /// </summary>
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public const string TypeName = "$type";

        /// <summary>
        /// AMF Undefined data type.
        /// </summary>
        public const int Undefined = 0;
        /// <summary>
        /// AMF Null data type.
        /// </summary>
        public const int Null = 1;
        /// <summary>
        /// AMF Boolean false data type.
        /// </summary>
        public const int BooleanFalse = 2;
        /// <summary>
        /// AMF Boolean true data type.
        /// </summary>
        public const int BooleanTrue = 3;
        /// <summary>
        /// AMF Integer data type.
        /// </summary>
        public const int Integer = 4;
        /// <summary>
        /// AMF Number data type.
        /// </summary>
        public const int Number = 5;
        /// <summary>
        /// AMF String data type.
        /// </summary>
        public const int String = 6;
        /// <summary>
        /// AMF Xml data type.
        /// </summary>
        public const int XmlDoc = 7;
        /// <summary>
        /// AMF DateTime data type.
        /// </summary>
        public const int DateTime = 8;
        /// <summary>
        /// AMF Array data type.
        /// </summary>
        public const int Array = 9;
        /// <summary>
        /// AMF Object data type.
        /// </summary>
        public const int Object = 10;
        /// <summary>
        /// AMF Xml data type.
        /// </summary>
        public const int Xml = 11;
        /// <summary>
        /// AMF ByteArray data type.
        /// </summary>
        public const int ByteArray = 12;

        /// <summary>
        /// AMF3 Data
        /// </summary>
        public const int Amf3Tag = 17;
    }
}

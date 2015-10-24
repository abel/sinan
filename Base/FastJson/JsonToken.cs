using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastJson
{
    /// <summary>
    /// JSON标识符.
    /// </summary>
    public class JsonToken
    {
        public const char ObjectHead = '{';
        public const char ObjectEnd = '}';
        public const char ArrayHead = '[';
        public const char ArrayEnd = ']';
        public const char Quote = '"';
        public const char KeySplit = ':';
        public const char ValueSplit = ',';
        public const char Escape = '\\';
        public const string Null = "null";
        public const string True = "true";
        public const string False = "false";
    }
}

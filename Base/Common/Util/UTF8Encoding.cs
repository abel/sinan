using System;
using System.Text;

namespace Sinan.Util
{
    public class UTF8Encoding
    {
#if unsafe
        //UTF-8以字节为单位对Unicode进行编码。从Unicode到UTF-8的编码方式如下：
        //Unicode编码(16进制)　║　UTF-8 字节流(二进制) 　　
        //000000 - 00007F　║　0xxxxxxx 　　
        //000080 - 0007FF　║　110xxxxx 10xxxxxx 　　
        //000800 - 00FFFF　║　1110xxxx 10xxxxxx 10xxxxxx 　　
        //010000 - 10FFFF　║　11110xxx 10xxxxxx 10xxxxxx 10xxxxxx 
        unsafe public static int GetBytes(string text, byte[] bin, int offset, int count)
        {
            fixed (char* cptr = text)
            {
                return GetBytes(cptr, text.Length, bin, offset);
            }
        }

        unsafe public static int GetBytes(char* cptr, int count, byte[] bin, int offset)
        {
            int total = 0;
            char* start = cptr;
            char* end = cptr + count;
            while (start < end)
            {
                char ch = *start;
                if (ch < '\u0080')
                {
                    total++;
                    bin[offset++] = (byte)ch;
                }
                else if (ch < '\u0800')
                {
                    total += 2;
                    bin[offset++] = (byte)(0xC0 | (ch >> 6));
                    bin[offset++] = (byte)(0x80 | (ch & 0x3F));
                }
                ////Unicode编码的0xD800-0xDFFF为保留编码
                //else if (ch >= '\uD800' && ch <= '\uDFFF')
                //{
                //    throw new System.Exception("不支持的Unicode编码");
                //}
                else
                {
                    total += 3;
                    bin[offset++] = (byte)(0xE0 | (ch >> 12));
                    bin[offset++] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                    bin[offset++] = (byte)(0x80 | (ch & 0x3F));
                }
                start++;
            }
            return total;
        }

        unsafe public static int GetByteCount(string text)
        {
            fixed (char* cptr = text)
            {
                return GetByteCount(cptr, text.Length);
            }
        }

        unsafe public static int GetByteCount(char* cptr, int count)
        {
            int total = 0;
            char* end = cptr + count;
            while (cptr < end)
            {
                if (*cptr < '\u0080')
                {
                    total++;
                }
                else if (*cptr < '\u0800')
                {
                    total += 2;
                }
                else
                {
                    total += 3;
                }
                cptr++;
            }
            return total;
        }

        private unsafe static int InternalGetBytes(char* chars, int count, byte* bytes, int bcount, ref char leftOver, bool flush)
        {
            char* end = chars + count;
            char* start = chars;
            byte* start_bytes = bytes;
            byte* end_bytes = bytes + bcount;
            while (chars < end)
            {
                if (leftOver == 0)
                {
                    for (; chars < end; chars++)
                    {
                        int ch = *chars;
                        if (ch < '\x80')
                        {
                            if (bytes >= end_bytes)
                                goto fail_no_space;
                            *bytes++ = (byte)ch;
                        }
                        else if (ch < '\x800')
                        {
                            if (bytes + 1 >= end_bytes)
                                goto fail_no_space;
                            bytes[0] = (byte)(0xC0 | (ch >> 6));
                            bytes[1] = (byte)(0x80 | (ch & 0x3F));
                            bytes += 2;
                        }
                        else if (ch < '\uD800' || ch > '\uDFFF')
                        {
                            if (bytes + 2 >= end_bytes)
                                goto fail_no_space;
                            bytes[0] = (byte)(0xE0 | (ch >> 12));
                            bytes[1] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                            bytes[2] = (byte)(0x80 | (ch & 0x3F));
                            bytes += 3;
                        }
                        else if (ch <= '\uDBFF')
                        {
                            // This is a surrogate char, exit the inner loop.
                            leftOver = *chars;
                            chars++;
                            break;
                        }
                        else
                        {
                            leftOver = '\0';
                        }
                    }
                }
                else
                {
                    if (*chars >= '\uDC00' && *chars <= '\uDFFF')
                    {
                        // We have a correct surrogate pair.
                        int ch = 0x10000 + (int)*chars - 0xDC00 + (((int)leftOver - 0xD800) << 10);
                        if (bytes + 3 >= end_bytes)
                            goto fail_no_space;
                        bytes[0] = (byte)(0xF0 | (ch >> 18));
                        bytes[1] = (byte)(0x80 | ((ch >> 12) & 0x3F));
                        bytes[2] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                        bytes[3] = (byte)(0x80 | (ch & 0x3F));
                        bytes += 4;
                        chars++;
                    }
                    leftOver = '\0';
                }
            }
            if (flush)
            {
                // Flush the left-over surrogate pair start.
                if (leftOver != '\0')
                {
                    int ch = leftOver;
                    if (bytes + 2 < end_bytes)
                    {
                        bytes[0] = (byte)(0xE0 | (ch >> 12));
                        bytes[1] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                        bytes[2] = (byte)(0x80 | (ch & 0x3F));
                        bytes += 3;
                    }
                    else
                    {
                        goto fail_no_space;
                    }
                    leftOver = '\0';
                }
            }
            return (int)(bytes - (end_bytes - bcount));
        fail_no_space:
            throw new ArgumentException("Insufficient Space", "bytes");
        }
#endif
    }
}
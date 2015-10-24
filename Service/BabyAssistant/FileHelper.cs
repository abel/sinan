using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sinan.BabyAssistant.Command;

namespace Sinan.BabyAssistant
{
    class FileHelper
    {
        public static bool CopyFile(string sourceFile, string desFile)
        {
            string dir = Path.GetDirectoryName(desFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Exception ex = null;
            int count = 0;
            while (count++ < 300)
            {
                try
                {
                    File.Copy(sourceFile, desFile, true);
                    ex = null;
                    break;
                }
                catch (Exception err)
                {
                    ex = err;
                    System.Threading.Thread.Sleep(1000);
                }
            }
            if (ex != null)
            {
                Log.LogInfo("修改失败:" + desFile + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断两文件是否相同
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <returns>true表示相同,false表示不相同</returns>
        public static bool FileEquals(string fileA, string fileB)
        {
            if (fileA == fileB) return true;
            using (FileStream fs1 = new FileStream(fileA, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream fs2 = new FileStream(fileB, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs1.Length != fs2.Length)
                {
                    return false;
                }
                //每次读4K数据
                const int oneRead = 4 * 1024;
                byte[] bin1 = new byte[oneRead];
                byte[] bin2 = new byte[oneRead];
                int readLen = fs1.Read(bin1, 0, oneRead);
                fs2.Read(bin2, 0, oneRead);
                while (readLen == oneRead)
                {
                    if (ByteArrayEquals(bin1, bin2, oneRead))
                    {
                        readLen = fs1.Read(bin1, 0, oneRead);
                        fs2.Read(bin2, 0, oneRead);
                    }
                    else
                    {
                        return false;
                    }
                }
                return ByteArrayEquals(bin1, bin2, readLen);
            }
        }

        /// <summary>
        /// 比较字符数组内容是否相同
        /// </summary>
        /// <returns></returns>
        static bool ByteArrayEquals(byte[] a, byte[] b, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}

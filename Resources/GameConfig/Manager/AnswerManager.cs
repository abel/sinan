using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Sinan.Data;

namespace Sinan.GameModule
{
    public class AnswerManager
    {
        static Dictionary<string, string> dic = new Dictionary<string, string>();
        /// <summary>
        /// 判断答题是否正确
        /// </summary>
        /// <param name="soleid">题目ID</param>
        /// <param name="answer">选择答案</param>
        /// <returns>0正确,1表示答错,2题目不正确</returns>
        public static int IsAnswer(string soleid, string answer)
        {

            if (dic.Count == 0)
            {
                XmlDocument doc = new XmlDocument();
                string xpath = Path.Combine(ConfigLoader.Config.DirBase, "answerXML.xml");
                doc.Load(xpath);
                XmlNodeList nodes = doc.SelectNodes("//content");
                foreach (XmlNode node in nodes)
                {
                    if (!dic.ContainsKey(node.Attributes["id"].Value))
                    {
                        dic.Add(node.Attributes["id"].Value, node.Attributes["right"].Value);
                    }
                }
            }
            string an;
            if (dic.TryGetValue(soleid, out an))
            {
                //答题正确
                if (an == answer) return 0;
                return 1;
            }
            return 2;
        }
    }
}

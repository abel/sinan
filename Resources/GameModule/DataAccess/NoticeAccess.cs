using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FastConfig;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GameModule
{
    /// <summary>
    /// 公告
    /// </summary>
    public class NoticeAccess : VariantBuilder<Notice>
    {
        readonly static NoticeAccess m_instance = new NoticeAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static NoticeAccess Instance
        {
            get { return m_instance; }
        }

        static ConcurrentDictionary<string, Notice> m_dic = new ConcurrentDictionary<string, Notice>();
        NoticeAccess()
            : base("Notice")
        {
        }

        public override bool Save(Notice model)
        {
            return base.Save(model);
        }


        public void LoadNotices() 
        {
            var v = m_collection.FindAllAs<Notice>();
            foreach (var item in v) 
            {
                m_dic.TryAdd(item.ID, item);
            }
        }


        public bool AddNotice(Notice model) 
        {
            return m_dic.TryAdd(model.ID, model);
        }


        public List<Notice> GetNotices()
        {
            List<Notice> list = new List<Notice>();
            foreach (var item in m_dic) 
            {
                //Console.WriteLine(item.Value.Name);
                list.Add(item.Value);
            }
            return list;
        }

        public bool GetNotice(string id,out Notice model) 
        {
           return m_dic.TryGetValue(id, out model);
        }
    }
}

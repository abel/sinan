using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class RoleManager : IConfigManager
    {
        readonly static RoleManager m_instance = new RoleManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static RoleManager Instance
        {
            get { return m_instance; }
        }
        private RoleManager() { }

        /// <summary>
        /// 角色升级获得技能的配置
        /// </summary>
        static Variant m_roleConfig;

        /// <summary>
        /// 角色升级所需经验
        /// </summary>
        List<int> m_roleExp;

        int m_maxLev = 0;

        /// <summary>
        /// 最大等级
        /// </summary>
        public int MaxLev
        {
            get { return m_maxLev; }
        }

        static public Variant RoleConfig
        {
            get { return m_roleConfig; }
        }

        public void Load(string path)
        {
            Variant roleConfig = VariantWapper.LoadVariant(path);
            List<int> roleExp = new List<int>(200);
            roleExp.Add(0);
            foreach (int i in roleConfig.GetValueOrDefault<IList>("RoleExp"))
            {
                if (i > 0)
                {
                    roleExp.Add(i);
                    m_maxLev = Math.Max(i, m_maxLev);
                }
            }
            m_roleConfig = roleConfig;
            m_roleExp = roleExp;
        }

        public string GetNewSkill(string roleID, int level)
        {
            return GetRoleConfig(roleID, level.ToString());
        }

        /// <summary>
        /// 获取角色配置
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetRoleConfig(string roleID, string key)
        {
            Variant v = m_roleConfig.GetVariantOrDefault(roleID);
            if (v != null)
            {
                return v.GetStringOrDefault(key);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取角色配置
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetRoleConfig<T>(string roleID, string key)
        {
            Variant v = m_roleConfig.GetVariantOrDefault(roleID);
            T t;
            if (v != null)
            {
                v.TryGetValueT(key, out t);
            }
            else
            {
                t = default(T);
            }
            return t;
        }

        /// <summary>
        /// 获取角色配置
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public Variant GetAllRoleConfig(string roleID)
        {
            return m_roleConfig.GetVariantOrDefault(roleID);
        }

        /// <summary>
        /// 获取玩家升级所需经验值
        /// </summary>
        /// <param name="level">当前等级</param>
        /// <returns></returns>
        public int GetRoleExp(int level)
        {
            if (level < m_roleExp.Count)
            {
                return m_roleExp[level];
            }
            return Int32.MaxValue;
        }

        public T GetValue<T>(string name)
        {
            T t;
            m_roleConfig.TryGetValueT<T>(name, out t);
            return t;
        }

        public void Unload(string fullPath)
        {
        }
    }
}

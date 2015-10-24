using System;
using System.Collections;
using System.Net.Sockets;
using Sinan.FastSocket;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif


namespace Sinan.GMServer
{
    static public class SessionManager<T> where T : class,ISession
    {
        /// <summary>
        /// Key:用户ID(字符串), 游戏服务器(服务器分区号)
        /// Value:T
        /// </summary>
        static readonly ConcurrentDictionary<string, T> m_users = new ConcurrentDictionary<string, T>();

        static public bool TryGetValue(string id, out T t)
        {
            return m_users.TryGetValue(id, out t);
        }


        /// <summary>
        ///  添加并获取旧值
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        static public T AddAndGetOld(T user)
        {
            T oldUser = null;
            m_users.AddOrUpdate(user.UserID, user, (x, y) =>
            {
                oldUser = y;
                return user;
            });
            return oldUser;
        }

        /// <summary>
        /// 移出用户
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="connectID">连接ID</param>
        /// <returns></returns>
        static public bool RemoveUser(string userID, int connectID)
        {
            T user = null;
            if (m_users.TryGetValue(userID, out user))
            {
                if (user.ConnectID == connectID)
                {
                    return m_users.TryRemove(userID, out user);
                }
            }
            return false;
        }

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="buffer">已编码的方法调用</param>
        static public bool Call(string userID, Sinan.Collections.BytesSegment buffer)
        {
            if (userID != null)
            {
                T user;
                if (m_users.TryGetValue(userID, out user))
                {
                    if (user.SendAsync(buffer) == SocketError.Success)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

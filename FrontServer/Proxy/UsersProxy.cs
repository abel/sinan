using System;
using System.Collections;
using System.Net.Sockets;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    static public class UsersProxy
    {
        /// <summary>
        /// Key:用户ID,Value:UserSession
        /// </summary>
        static readonly ConcurrentDictionary<string, UserSession> m_users = new ConcurrentDictionary<string, UserSession>();

        public static int UserCount
        {
            get { return m_users.Count; }
        }

        /// <summary>
        ///  添加并获取旧值
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        static public UserSession AddAndGetOld(UserSession user)
        {
            UserSession oldUser = null;
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
            UserSession user = null;
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
        ///  调用客户端的方法
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="name">方法名</param>
        /// <param name="objs">方法参数</param>
        /// <returns></returns>
        static public bool Call(string userID, string name, IList objs)
        {
            if (userID != null && name != null)
            {
                UserSession user;
                if (m_users.TryGetValue(userID, out user))
                {
                    if (user.CallArray(name, objs) == SocketError.Success)
                    {
                        return true;
                    }
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
                UserSession user;
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

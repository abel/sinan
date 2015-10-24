
using System;
using System.Collections.Generic;
using Sinan.AMF3;
using Sinan.FastSocket;
using Sinan.Log;
using Sinan.FrontServer;
using Sinan.Observer;
using Sinan.Command;

namespace Sinan.GMServer
{
    public class CommandProcessor : IBufferProcessor, ICommandProcessor<Tuple<int, List<object>>>
    {
        //接收客户端消息的最大容量
        readonly protected int m_capacity;
        readonly protected AmfStringZip m_zip;
        public CommandProcessor(int capacity, AmfStringZip zip)
        {
            m_zip = zip;
            m_capacity = capacity;
        }

        public int Execute(ISession session, Sinan.Collections.BytesSegment data)
        {
            List<Tuple<int, List<object>>> results = new List<Tuple<int, List<object>>>();
            int offset = data.Offset;
            int maxIndex = offset + data.Count;
            byte[] buffer = data.Array;
            while (maxIndex >= offset + 2)
            {
                // 取包长,前两字节表示
                int packetLen = buffer[offset] + (buffer[offset + 1] << 8);
                if (packetLen < m_capacity)
                {
                    if (maxIndex < offset + packetLen)
                    {
                        break;
                    }
                    try
                    {
                        int uid;
                        if (!int.TryParse(session.UserID, out uid))
                        {
                            //GM客户端.需要解密
                            if (!session.Decode(buffer, offset, packetLen))
                            {
                                session.Close();
                                return -1;
                            }
                        }
                        // 取命令,第3/4字节
                        int command = buffer[offset + 2] + (buffer[offset + 3] << 8);
                        var param = AmfCodec.Decode(buffer, offset, packetLen);
                        results.Add(new Tuple<int, List<object>>(command, param));
                    }
                    catch (AmfException ex)
                    {
                        LogWrapper.Warn(session.ToString(), ex);
                        session.Close();
                        return -1;
                    }
                }
                foreach (var v in results)
                {
                    if (!this.Execute(session, v))
                    {
                        break;
                    }
                }
                offset += packetLen;
            }
            return maxIndex - offset;
        }

        public bool Execute(ISession token, Tuple<int, List<object>> bin)
        {
            UserSession user = token as UserSession;
            try
            {
                int command = bin.Item1;// - user.CommandMask;
                if (command == 103)
                {
                    return true; //心跳包
                }
                string name = m_zip.ReadString(command);
                if (name != null)
                {
                    return Execute(user, name, bin.Item2);
                }
                //命令不存在时.则断开.
                user.Close();
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(user.ToString(), ex);
            }
            return false;
        }


        protected bool Execute(UserSession user, string command, List<object> parm)
        {
            if (!string.IsNullOrEmpty(user.UserID))
            {
                //已登录用户操作指令
                //TODO:记日志..检查权限
                UserNote<UserSession> note = new UserNote<UserSession>(user, command, parm);
                Notifier.Instance.Publish(note);
                return true;
            }

            //GM登录命令
            if (command != LoginCommand.UserLogin || parm == null || parm.Count != 2)
            {
                user.Close();
                return false;
            }
            return UserLogin(user, parm[0].ToString(), parm[1].ToString());
        }

        private static bool UserLogin(UserSession user, string userID, string pwd)
        {
            int zoneid;
            if (int.TryParse(userID, out zoneid))
            {
                // TODO: 游戏服务器登录
                if (FrontManager.Instance.GetValue(zoneid) != pwd
                    && user.IP.ToString() != pwd)
                {
                    user.Close();
                    return false;
                }
                user.zoneid = zoneid;
            }
            else
            {
                // TODO: GM用户登录
                if (!GMManager.Instance.Login(userID, pwd))
                {
                    user.Close();
                    return false;
                }
            }

            user.UserID = userID;
            UserSession oldUser = SessionManager<UserSession>.AddAndGetOld(user);
            if (oldUser != null && oldUser != user)
            {
                ProcessOldUser(oldUser);
            }
            //登录成功
            user.Call(LoginCommand.UserLoginR, "登录成功");
            return true;
        }

        /// <summary>
        /// 处理重复登录用户
        /// </summary>
        /// <param name="oldUser"></param>
        private static void ProcessOldUser(UserSession oldUser)
        {
            try
            {
                //TODO: 需添加发送重复登录信息 
                oldUser.Close();
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(ex);
            }
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using MongoDB.Bson;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Log;
using Sinan.Command;
using Sinan.Observer;
using Sinan.Extensions;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 用户Session
    /// </summary>
    sealed public class UserSession : SocketToken
    {
        private int m_zoneid;
        private PlayerBusiness m_player;

        #region 属性
        public UserBuffer Buffer;

        /// <summary>
        /// 最后的操作命令
        /// </summary>
        public Tuple<int, List<object>> LastCommand;

        /// <summary>
        /// 创建连接时间
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }

        public PlayerBusiness Player
        {
            get { return m_player; }
        }

        /// <summary>
        /// 用于区分从哪个业务平台进入应用 ：
        /// Qzone 为 1 ；
        /// 腾讯朋友为 2 ；
        /// 腾讯微博为 3 ；
        /// Q+ 平台为 4 ；
        /// 财付通开放平台为 5 ；
        /// QQGame 为 10 ；
        /// </summary>
        public Domain domain
        {
            get;
            set;
        }

        /// <summary>
        /// 应用分区分服时大区的ID. 如果分区分服，
        /// 则该 ID 为新建服务器时自动分配的域名中的serverid 。
        /// </summary>
        public int zoneid
        {
            get { return m_zoneid; }
            set { m_zoneid = value; }
        }

        /// <summary>
        /// 用户登录的 session key 
        /// （即 openkey ）
        /// </summary>
        public string key
        {
            get;
            set;
        }

        /// <summary>
        /// 安全key校验结果
        /// </summary>
        public int keycheckret
        {
            get;
            set;
        }

        public Variant QQToken
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">唯一ID</param>
        /// <param name="receiveBuffer">接收缓冲区</param>
        /// <param name="sendBuffer">发送缓冲区</param>
        /// <param name="socket">连接的Socket</param>
        /// <param name="process"></param>
        public UserSession(int id, UserBuffer buffer, Socket socket, IBufferProcessor process)
            : base(id, buffer.ReceiveBuffer, buffer.SendBuffer, process, buffer.SendPool)
        {
            m_socket = socket;
            Buffer = buffer;
            m_address = ((System.Net.IPEndPoint)(socket.RemoteEndPoint)).Address;
            Created = DateTime.UtcNow;
        }

        /// <summary>
        /// 调用客户端方法.
        /// </summary>
        /// <param name="name">客户端的方法名</param>
        /// <param name="objs">方法参数数组</param>
        /// <returns></returns>
        public SocketError CallArray(string name, IList objs)
        {
            var buffer = AmfCodec.Encode(name, objs);
            return base.SendAsync(buffer);
        }

        /// <summary>
        /// 调用客户端方法.
        /// </summary>
        /// <param name="name">客户端的方法名</param>
        /// <param name="objs">方法参数数组</param>
        /// <returns></returns>
        public SocketError Call(string name, params object[] objs)
        {
            var buffer = AmfCodec.Encode(name, objs);
            return base.SendAsync(buffer);
        }

        public void InitPlayer(PlayerBusiness player)
        {
            m_player = player;
            if (player != null)
            {
                DateTime now = DateTime.UtcNow;
                player.LoginTime = now;
                player.WriteLog(new RoleLogin());
            }
        }

        public void UserExit()
        {
            if (m_player != null)
            {
                DateTime now = DateTime.UtcNow;
                int second = (int)(now - m_player.LoginTime).TotalSeconds;
                RoleLogout log = new RoleLogout();
                log.onlinetime = second;
                m_player.WriteLog(log);
            }
        }

        public override void Close()
        {
            UserBuffer buffer = null;
            PlayerBusiness player = null;
            lock (m_locker)
            {
                if (Buffer == null)
                {
                    return;
                }
                player = m_player;
                buffer = Buffer;
                Buffer = null;
            }
            try
            {
                //已登录用户断开连接
                if (m_userID != null)
                {
                    UserExit();
                    UserNote note = new UserNote(this, ClientCommand.UserDisconnected, new object[] { this });
                    Notifier.Instance.Publish(note);
                }
            }
            finally
            {
                base.Close();
                buffer.Free(this);
                m_player = null;
                if (player != null)
                {
                    player.DisConnection(this);
                }
            }
        }

        public override string ToString()
        {
            return ToString(string.Empty);
        }

        public string ToString(string head)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
            sb.Append(head);
            sb.Append("{");
            sb.Append("IP:");
            sb.Append(m_address);
            sb.Append(",UID:");
            sb.Append(m_userID);
            var temp = m_player;
            if (temp != null)
            {
                sb.Append(",PID:");
                sb.Append(temp.ID);
                sb.Append(",N:");
                sb.Append(temp.Name);
                sb.Append(",L:");
                sb.Append(temp.Level.ToString());
                sb.Append(",S:");
                sb.Append(temp.SceneID);
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override bool Decode(byte[] buffer, int offset, int count)
        {
            int seed = this.m_connectID;
            //前两字节为包头.表示包长度
            seed = (seed << 3) - seed + buffer[offset];
            seed = (seed << 3) - seed + buffer[offset + 1];
            int max = offset + count;
            for (int i = offset + 2; i < max; i++)
            {
                byte b = buffer[i];
                byte newb = (byte)(b - seed);
                buffer[i] = newb;
                seed = (seed << 3) - seed + newb;
            }
            return true;
        }
    }
}

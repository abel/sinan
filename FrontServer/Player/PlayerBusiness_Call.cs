using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        public void CallBig(string command, params object[] msgs)
        {
            UserSession user = m_session;
            if (user != null)
            {
                var buffer = AmfCodec.EncodeBig(command, msgs);
                user.SendAsync(buffer);
            }
        }

        /// <summary>
        /// 发送消息给玩家
        /// </summary>
        /// <param name="command"></param>
        /// <param name="msgs"></param>
        public void Call(string command, params object[] msgs)
        {
            UserSession user = m_session;
            if (user != null)
            {
                user.CallArray(command, msgs);
            }
        }

        /// <summary>
        /// 调用客户端的方法
        /// </summary>
        /// <param name="buffer">已编码的方法调用</param>
        public bool Call(Sinan.Collections.BytesSegment buffer)
        {
            UserSession user = m_session;
            if (user != null)
            {
                if (user.SendAsync(buffer) == SocketError.Success)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 发送给场景上所有用户
        /// </summary>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        public void CallAll(string command, params object[] objs)
        {
            SceneBusiness scene = m_scene;
            if (scene != null)
            {
                scene.CallAll(this.ShowID, command, objs);
            }
        }

        /// <summary>
        /// 发送给场景上所有用户
        /// </summary>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        public void CallAll(Sinan.Collections.BytesSegment buffer)
        {
            SceneBusiness scene = m_scene;
            if (scene != null)
            {
                scene.CallAll(this.ShowID, buffer);
            }
        }

        /// <summary>
        /// 发送给场景上其它的用户
        /// </summary>
        /// <param name="excludeID"></param>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        public void CallAllExcludeOne(PlayerBusiness exclude, string command, params object[] objs)
        {
            SceneBusiness scene = m_scene;
            if (scene != null)
            {
                var buffer = AmfCodec.Encode(command, objs);
                scene.CallAllExcludeOne(exclude, buffer);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;
using Sinan.Command;
using Sinan.FastJson;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Security.Cryptography;
using Sinan.Util;

namespace Sinan.GMServerModule
{
    /// <summary>
    /// GM服务
    /// </summary>
    class GMServer : HttpServer
    {
        private static ILog logger = LogManager.GetLogger("GMLog");

        static long f = Stopwatch.Frequency / 1000;
        static Stopwatch watch = Stopwatch.StartNew();

        byte[] sendEnds;
        string m_path;
        bool m_checkSig;

        IPAddress callAddr;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">路径验证</param>
        /// <param name="checkSig">启用签名验证</param>
        public GMServer(string path, bool checkSig)
        {
            sendEnds = Encoding.UTF8.GetBytes(" HTTP/1.1\r\nHost: " + "sinan.com" + "\r\nConnection: Close\r\n\r\n");
            m_path = path;
            m_checkSig = checkSig;
            IPAddress.TryParse(ConfigLoader.Config.GMKey, out callAddr);
        }

        Dictionary<string, Func<Notification, object>> m_command;
        Dictionary<string, Func<Notification, object>> m_adminCommand;

        public override CompactResponse Process(Socket client, CompactRequest request)
        {
            CompactResponse response = new CompactResponse();
            response.Ret = 4;
            try
            {
                if (request.Queries == null || (!request.Url.StartsWith(m_path)))
                {
                    response.Msg = "404";
                    return response;
                }
                string sig, cmd, par, time;
                request.Queries.TryGetValue("sig", out sig);
                request.Queries.TryGetValue("cmd", out cmd);
                request.Queries.TryGetValue("par", out par);
                request.Queries.TryGetValue("time", out time);

                object ps = JsonDecoder<Variant>.DeserializeObject(par);

                if (callAddr == null)
                {
                    string test = MD5Helper.MD5Encrypt(cmd + par + time + ConfigLoader.Config.GMKey);
                    if (test != sig.ToUpper())
                    {
                        response.Msg = "验证错误";
                        return response;
                    }
                }
                else if (callAddr.Address != ((System.Net.IPEndPoint)(client.RemoteEndPoint)).Address.Address)
                {
                    response.Msg = "验证错误";
                    return response;
                }


                Notification note = new Notification(cmd, ps as IList);

                System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
                sb.Append("IP:");
                sb.Append(client.RemoteEndPoint);
                sb.Append(" cmd:");
                sb.Append(note.Name);
                sb.Append(" par:");
                sb.Append(par);

                //写日志
                if (note.Name != GMCommand.Online)
                {
                    logger.Info(sb.ToString());
                }

                Func<Notification, object> fun;
                if (m_command.TryGetValue(cmd, out fun) ||
                    (ServerManager.AdminGM && m_adminCommand.TryGetValue(cmd, out fun)))
                {
                    if (fun != null)
                    {
                        object result = fun(note);
                        if (client.Connected)
                        {
                            string resultStr = JsonConvert.SerializeObject(result);
                            byte[] data = Encoding.UTF8.GetBytes(resultStr);
                            //client.Send(data);
                            //寫入資料本體
                            client.Send(new ArraySegment<byte>[]{
                                    new ArraySegment<byte>(CompactResponse.HttpOK),
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.Length + "\r\n\r\n")),
                                    new ArraySegment<byte>(data)}
                                   );
                            client.Shutdown(SocketShutdown.Send);
                        }
                        return null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                response.Msg = ex.Message;
                LogWrapper.Error(ex);
            }
            return response;
        }

        /// <summary>
        /// GM基本指令
        /// </summary>
        public void Init()
        {
            m_command = new Dictionary<string, Func<Notification, object>>();
            m_command.Add(GMCommand.KickUser, GMBusiness.KickUser);
            m_command.Add(GMCommand.Online, GMBusiness.Online);
            m_command.Add(GMCommand.SetPlayerState, GMBusiness.SetPlayerState);
            m_command.Add(GMCommand.ViewPlayer, GMBusiness.ViewPlayer);
            m_command.Add(GMCommand.SetTalk, GMBusiness.SetTalk);
            m_command.Add(GMCommand.DoubleExp, GMBusiness.DoubleExp);
            m_command.Add(GMCommand.Part, GMBusiness.OpenPart);
            m_command.Add(GMCommand.Bond, GMBusiness.AddBond);
            m_command.Add(GMCommand.Notice, GMBusiness.Notice);
            m_command.Add(GMCommand.NoticeList, GMBusiness.NoticeList);
            m_command.Add(GMCommand.UpdateNotice, GMBusiness.UpdateNotice);
            m_command.Add(GMCommand.UpdatePart, GMBusiness.UpdatePart);
            //if (ServerManager.AdminGM)
            {
                m_adminCommand = new Dictionary<string, Func<Notification, object>>();
                m_adminCommand.Add(GMCommand.Exitall, ManageBusiness.ExitAll);
                m_adminCommand.Add(GMCommand.Coin, ManageBusiness.AddCoin);
                m_adminCommand.Add(GMCommand.Score, ManageBusiness.AddScore);
                m_adminCommand.Add(GMCommand.Bond, ManageBusiness.AddBond);
                m_adminCommand.Add(GMCommand.Exp, ManageBusiness.Exp);
                m_adminCommand.Add(GMCommand.Power, ManageBusiness.AddPower);

                m_adminCommand.Add(GMCommand.TaskRemove, ManageBusiness.TaskRemove);
                m_adminCommand.Add(GMCommand.TaskAct, ManageBusiness.TaskAct);
                m_adminCommand.Add(GMCommand.TaskId, ManageBusiness.GetTaskID);
                m_adminCommand.Add(GMCommand.Goodsid, ManageBusiness.GoodID);
                m_adminCommand.Add(GMCommand.Getgood, ManageBusiness.GetGood);
                m_adminCommand.Add(GMCommand.GoodRemove, ManageBusiness.GoodRemove);
                m_adminCommand.Add(GMCommand.PetExp, ManageBusiness.PetExp);
                m_adminCommand.Add(GMCommand.Skill, ManageBusiness.Skill);
                m_adminCommand.Add(GMCommand.Pskill, ManageBusiness.PSkill);
                m_adminCommand.Add(GMCommand.Restart, ManageBusiness.ReStart);
                m_adminCommand.Add(GMCommand.SelectEmail, ManageBusiness.SelectEmail);
                m_adminCommand.Add(GMCommand.GMDelEmail, ManageBusiness.GMDelEmail);
                m_adminCommand.Add(GMCommand.TaskReset, ManageBusiness.TaskReset);
                m_adminCommand.Add(GMCommand.FamilySite, ManageBusiness.FamilySite);

                m_adminCommand.Add(GMCommand.GMAuctionList, ManageAuction.GMAuctionList);
                m_adminCommand.Add(GMCommand.GMAuctionDel, ManageAuction.GMAuctionDel);
                m_adminCommand.Add(GMCommand.GMBurdenClear, ManageAuction.GMBurdenClear);
                m_adminCommand.Add(GMCommand.GMMallInfo, ManageAuction.GMMallInfo);
                m_adminCommand.Add(GMCommand.EmailSend, ManageAuction.GMEmailSend);
                
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 前端服务基础配置文件
    /// 不支持动态修改
    /// </summary>
    public class BaseConfig
    {
        readonly public string BaseDirectory;

        /// <summary>
        /// 运营平台
        /// 腾讯:tencent
        /// 趣游:gamewave
        /// 新干线:gameflier
        /// </summary>
        readonly public string Platform;

        /// <summary>
        /// Log4配置文件
        /// </summary>
        readonly public string Log4Config;

        /// <summary>
        /// 插件目录
        /// </summary>
        readonly public string DirPlugin;

        /// <summary>
        /// 默认配置目录
        /// </summary>
        readonly public string DirConfig;

        /// <summary>
        /// 数据库目录
        /// </summary>
        readonly public string DirDB;

        /// <summary>
        /// 游戏配置目录
        /// </summary>
        readonly public string DirGame;

        /// <summary>
        /// 基本配置目录
        /// </summary>
        readonly public string DirBase;

        /// <summary>
        /// 分区ID
        /// </summary>
        readonly public int Zoneid;

        /// <summary>
        /// 游戏服务监听的IP和端口
        /// </summary>
        readonly public IPEndPoint EpGame;

        /// <summary>
        /// 发货服务监听的IP和端口
        /// </summary>
        readonly public IPEndPoint EpShip;

        /// <summary>
        /// GM服务监听的IP和端口(AMF直接调用方式)
        /// </summary>
        readonly public IPEndPoint EpGM;

        /// <summary>
        /// GM服务监听的IP和端口(Http间接调用方式)
        /// </summary>
        readonly public IPEndPoint EpGMM;

        /// <summary>
        /// 最大允许连接的客户端
        /// </summary>
        readonly public int MaxClient;

        /// <summary>
        /// 发送环形队列大小
        /// </summary>
        readonly public int SendQueueSize;

        /// <summary>
        /// 最高等级
        /// </summary>
        readonly public int MaxLevel;

        /// <summary>
        /// 名字的最大长度,默认16
        /// </summary>
        readonly public int MaxNameLen;

        /// <summary>
        /// 游戏服务的数据库连接串
        /// </summary>
        readonly public string DbPlayer;

        /// <summary>
        /// 配置服务的数据库连接串
        /// </summary>
        readonly public string DbBase;

        /// <summary>
        /// 日志服务的数据库连接串
        /// </summary>
        readonly public string DbLog;

        /// <summary>
        /// 开服时间
        /// </summary>
        readonly public DateTime ZoneEpoch;

        /// <summary>
        /// 47baby登录密匙
        /// </summary>
        readonly public string DesKey;

        /// <summary>
        /// 登录密匙
        /// </summary>
        readonly public string LoginKey;

        /// <summary>
        /// 充值密匙
        /// </summary>
        readonly public string RechargeKey;

        /// <summary>
        /// GM工具密匙
        /// </summary>
        readonly public string GMKey;

        /// <summary>
        /// 服务端报告的IP地址
        /// </summary>
        readonly public string ReportSIP;

        /// <summary>
        /// 跨域文件
        /// </summary>
        readonly public string Crossdomain;

        /// <summary>
        /// WebService地址
        /// </summary>
        readonly public string WebAddress;
        /// <summary>
        /// 语言目录(文件使用UTF8格式保存)
        /// en-US:英语美国
        /// zh-CN:中文中国
        /// zh-TW:中文台湾
        /// zh-SG:中文新加坡
        /// th-TH:泰语泰国
        /// </summary>
        readonly public string Language;

        /// <summary>
        /// 是否开始道具金币
        /// </summary>
        readonly public bool FreeCoin;

        public BaseConfig(Variant config)
        {
            this.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            this.Platform = config.GetStringOrDefault("Platform", Sinan.Entity.Platform.Tencent);
            this.DirConfig = Path.Combine(this.BaseDirectory, fixPath(config.GetStringOrDefault("DirConfig", "Config")));

            this.Crossdomain = Path.Combine(DirConfig, fixPath(config.GetStringOrDefault("Crossdomain", "crossdomain.txt")));
            this.DirBase = Path.Combine(DirConfig, fixPath(config.GetStringOrDefault("DirBase", "base")));
            this.DirGame = Path.Combine(DirConfig, fixPath(config.GetStringOrDefault("DirGame", "game")));
            this.DirDB = Path.Combine(DirConfig, fixPath(config.GetStringOrDefault("DirDB", "db")));

            this.Language = config.GetStringOrDefault("Language");
            if (!string.IsNullOrWhiteSpace(Language))
            {
                string path = Path.Combine(DirConfig, fixPath(Language));
                if (Directory.Exists(path))
                {
                    this.DirBase = path;
                }
            }

            this.DirPlugin = Path.Combine(BaseDirectory, fixPath(config.GetStringOrDefault("DirPlugin", "Plugins")));
            this.Log4Config = Path.Combine(BaseDirectory, fixPath(config.GetStringOrDefault("Log4Config", "log4net.config")));

            this.DbBase = config.GetStringOrDefault("DbBase");
            this.DbLog = config.GetStringOrDefault("DbLog");
            this.DbPlayer = config.GetStringOrDefault("DbPlayer");

            string epGame = config.GetStringOrDefault("EpGame");
            this.EpGame = CreateEndPoint(epGame);

            string epGM = config.GetStringOrDefault("EpGM");
            this.EpGM = CreateEndPoint(epGM);

            string epGMM = config.GetStringOrDefault("EpGMM");
            this.EpGMM = CreateEndPoint(epGMM);

            string epShip = config.GetStringOrDefault("EpShip");
            this.EpShip = CreateEndPoint(epShip);

            this.MaxClient = config.GetIntOrDefault("MaxClient", 500);
            this.SendQueueSize = config.GetIntOrDefault("SendQueueSize", 100);
            this.MaxLevel = config.GetIntOrDefault("MaxLevel", 100);
            this.MaxNameLen = config.GetIntOrDefault("MaxNameLen", 16);

            this.Zoneid = config.GetIntOrDefault("Zoneid");
            this.ZoneEpoch = config.GetUtcTimeOrDefault("ZoneEpoch");

            this.DesKey = config.GetStringOrDefault("DESKey");
            this.LoginKey = config.GetStringOrDefault("LoginKey") ?? config.GetStringOrDefault("MD5Key");
            this.RechargeKey = config.GetStringOrDefault("RechargeKey");
            this.GMKey = config.GetStringOrEmpty("GMKey");
            this.ReportSIP = config.GetStringOrDefault("ReportSIP");

            this.WebAddress = config.GetStringOrDefault("WebAddress");

            this.FreeCoin = config.GetBooleanOrDefault("FreeCoin", false);
        }

        static string fixPath(string x)
        {
            if (!string.IsNullOrEmpty(x))
            {
                if (System.IO.Path.DirectorySeparatorChar == '/')
                {
                    return x.Replace('\\', '/');
                }
                else
                {
                    return x.Replace('/', '\\');
                }
            }
            return x;
        }

        public static BaseConfig Create(string file)
        {
            Variant config = VariantWapper.LoadVariant(file);
            return new BaseConfig(config);
        }

        static IPEndPoint CreateEndPoint(string address, int port)
        {
            IPAddress hostAddress;
            if (!IPAddress.TryParse(address, out hostAddress))
            {
                hostAddress = Dns.Resolve(address).AddressList[0];
            }
            return new IPEndPoint(hostAddress, port);
        }

        static IPEndPoint CreateEndPoint(string address)
        {
            if (address != null)
            {
                string[] ap = address.Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (ap.Length == 2)
                {
                    int port;
                    if (int.TryParse(ap[1], out port))
                    {
                        return CreateEndPoint(ap[0], port);
                    }
                }
            }
            return null;
        }
    }
}

using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text;
using Sinan.FastConfig;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Log;

namespace Sinan.ShipmentService
{
    [System.ComponentModel.DesignerCategory("Class")]
    public class RouteService : ServiceBase
    {
        static string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config");
        ConfigFacade m_configFacade = new ConfigFacade(path);
        ShipmentRouter router;
        Replenishment repleni;

        public RouteService()
        {
            this.ServiceName = "RouteService";
            //注册认处理器
            m_configFacade.RegistConfigProcessor(FrontManager.Instance, "FrontIP.txt");
            m_configFacade.RegistConfigProcessor(GoodsManager.Instance, "GoodsInfo.txt");
            m_configFacade.RegistConfigProcessor(OrderTypeManager.Instance, "OrderType.txt");
        }

        public void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            string msg = Encoding.UTF8.GetString(CompactResponse.RetOK);
            LogWrapper.Warn("服务正在启动:" + System.Environment.NewLine + msg);
            try
            {
                m_configFacade.LoadAll(null);
                m_configFacade.Enable = true;

                string path = ConfigurationManager.AppSettings["Path"];
                string ip = ConfigurationManager.AppSettings["IP"];
                int port = int.Parse(ConfigurationManager.AppSettings["RoutePort"]);
                bool checkSig;
                bool.TryParse(ConfigurationManager.AppSettings["CheckSig"], out checkSig);
                string connectionString = ConfigurationManager.AppSettings["OrderDB"];
                OrderAccess.Instance.Connect(connectionString);
                string key = ConfigurationManager.AppSettings["RechargeKey"];
                if (string.IsNullOrEmpty(key))
                {
                    router = new ShipmentRouter(path, checkSig);
                }
                else
                {
                    router = new ShipmentRouterQY(path, key);
                }

                repleni = new Replenishment(router);
                router.Start(ip, port);
                repleni.Start();
                LogWrapper.Warn("服务启动成功");
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error("服务启动失败", ex);
            }
        }

        protected override void OnStop()
        {
            LogWrapper.Warn("服务正在停止");
            try
            {
                m_configFacade.Enable = false;
                repleni.Close();
                router.Stop();
                router = null;
                LogWrapper.Warn("服务停止成功");
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error("服务停止失败", ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.ServiceProcess;
using Sinan.FastConfig;

[System.ComponentModel.DesignerCategory("Class")]
partial class BabyService : ServiceBase, IConfigManager
{
    /// <summary>
    /// 配置文件目录
    /// </summary>
    string serverPath;

    /// <summary>
    /// 配置文件监听
    /// </summary>
    ConfigFacade m_baseFacade;

    /// <summary>
    /// 所有加载的应用程序域
    /// </summary>
    Dictionary<string, AppDomain> domains = new Dictionary<string, AppDomain>();

    public BabyService()
    {
        this.ServiceName = "BabyService";
        this.serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Server");
    }

    public void Start(string[] args)
    {
        this.OnStart(args);
    }

    protected override void OnStart(string[] args)
    {
        FileInfo info = new System.IO.FileInfo(Path.Combine(serverPath, "log4net.config"));
        log4net.Config.XmlConfigurator.Configure(info);

        m_baseFacade = new ConfigFacade(serverPath, "*.txt", false);
        m_baseFacade.RegistConfigProcessor(this, "*");
        m_baseFacade.LoadAll();
        m_baseFacade.Enable = true;
    }

    protected override void OnStop()
    {
        m_baseFacade.Enable = false;
        lock (domains)
        {
            foreach (var item in domains)
            {
                try
                {
                    AppDomain.Unload(item.Value);
                    LogWrapper.Warn("OnStop.UnloadSuccess:" + item.Key);
                }
                catch (System.Exception ex)
                {
                    Exception inner = ex.InnerException;
                    if (inner != null)
                    {
                        LogWrapper.Error("OnStop.UnloadFail InnerException", inner);
                    }
                    LogWrapper.Error("OnStop.UnloadFail", ex);
                }
            }
            domains.Clear();
        }
    }

    void IConfigManager.Load(string path)
    {
        try
        {
            AppDomain current = AppDomain.CurrentDomain;
            string name = Path.GetFileNameWithoutExtension(path);
            AppDomainSetup info = new AppDomainSetup();
            info.ApplicationBase = current.BaseDirectory;
            info.ApplicationName = name;
            info.DynamicBase = serverPath;
            info.CachePath = serverPath;
            info.ShadowCopyFiles = "true";

            Evidence securityInfo = new Evidence(current.Evidence);
            var app = AppDomain.CreateDomain(name, securityInfo, info);

            if (app != null)
            {
                lock (domains)
                {
                    AppDomain old;
                    if (domains.TryGetValue(path, out old))
                    {
                        if (domains.Remove(path))
                        {
                            AppDomain.Unload(old);
                            LogWrapper.Warn("Load.UnloadSuccess:" + path);
                        }
                    }
                    domains.Add(path, app);
                    string exeFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FrontServer.exe");
                    app.ExecuteAssembly(exeFile, new string[] { path });
                }
                LogWrapper.Warn("Load.LoadSuccess:" + path);
            }
        }
        catch (System.Exception ex)
        {
            LogWrapper.Error("Load.LoadFail", ex);
        }
    }

    void IConfigManager.Unload(string path)
    {
        try
        {
            AppDomain app;
            lock (domains)
            {
                if (domains.TryGetValue(path, out app))
                {
                    if (domains.Remove(path))
                    {
                        AppDomain.Unload(app);
                        LogWrapper.Warn("Unload.UnloadSuccess:" + path);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            LogWrapper.Error("Unload.UnloadFail", ex);
        }
    }

}

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;

#if !mono

/// <summary>
/// 此类只能将代码添加到项目中使用
/// </summary>
[RunInstaller(true)]
[System.ComponentModel.DesignerCategory("Class")]
public class InstallHelper : Installer
{
    private System.ServiceProcess.ServiceProcessInstaller processInstaller;
    private System.ServiceProcess.ServiceInstaller serviceInstaller;

    #region 组件设计器生成的代码
    /// <summary>
    /// 设计器支持所需的方法 - 不要
    /// 使用代码编辑器修改此方法的内容。
    /// </summary>
    private void InitializeComponent()
    {
        this.processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
        this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();

        // ProjectInstaller
        this.processInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
        this.processInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceInstaller});
        this.processInstaller.Password = null;
        this.processInstaller.Username = null;

        // serviceInstaller
        this.serviceInstaller.Description = InstallHelper.Description ?? InstallHelper.ServiceName;
        this.serviceInstaller.DisplayName = InstallHelper.ServiceName;
        this.serviceInstaller.ServiceName = InstallHelper.ServiceName;
        this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.processInstaller});
    }
    #endregion

    public InstallHelper()
    {
        InitializeComponent();
    }

    static public string ServiceName;
    static public string Description;

    public static void Install(string[] args)
    {
        bool flag = false;
        bool flag2 = false;
        TransactedInstaller installerWithHelp = new TransactedInstaller();
        bool flag3 = false;
        try
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("/", StringComparison.Ordinal) || args[i].StartsWith("-", StringComparison.Ordinal))
                {
                    string strA = args[i].Substring(1);
                    if ((string.Compare(strA, "u", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "uninstall", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        flag = true;
                    }
                    else if ((string.Compare(strA, "?", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "help", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        flag3 = true;
                    }
                    else if (string.Compare(strA, "AssemblyName", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        list.Add(args[i]);
                    }
                }
                else
                {
                    string name = args[i];
                    ServiceName = name;
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    AssemblyInstaller installer2 = new AssemblyInstaller(assembly, (string[])list.ToArray(typeof(string)));
                    installerWithHelp.Installers.Add(installer2);
                }
            }
            if (flag3 || (installerWithHelp.Installers.Count == 0))
            {
                flag3 = true;
                installerWithHelp.Installers.Add(new AssemblyInstaller());
                throw new InvalidOperationException("GetHelp(installerWithHelp)");
            }
            installerWithHelp.Context = new InstallContext("InstallUtil.InstallLog", (string[])list.ToArray(typeof(string)));
        }
        catch (Exception exception2)
        {
            if (flag3)
            {
                throw exception2;
            }
            throw new InvalidOperationException("InstallInitializeException");
        }
        try
        {
            string str2 = installerWithHelp.Context.Parameters["installtype"];
            if ((str2 != null) && (string.Compare(str2, "notransaction", StringComparison.OrdinalIgnoreCase) == 0))
            {
                string str3 = installerWithHelp.Context.Parameters["action"];
                if ((str3 != null) && (string.Compare(str3, "rollback", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    installerWithHelp.Context.LogMessage("InstallRollbackNtRun");
                    for (int j = 0; j < installerWithHelp.Installers.Count; j++)
                    {
                        installerWithHelp.Installers[j].Rollback(null);
                    }
                }
                else if ((str3 != null) && (string.Compare(str3, "commit", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    installerWithHelp.Context.LogMessage("InstallCommitNtRun");
                    for (int k = 0; k < installerWithHelp.Installers.Count; k++)
                    {
                        installerWithHelp.Installers[k].Commit(null);
                    }
                }
                else if ((str3 != null) && (string.Compare(str3, "uninstall", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    installerWithHelp.Context.LogMessage("InstallUninstallNtRun");
                    for (int m = 0; m < installerWithHelp.Installers.Count; m++)
                    {
                        installerWithHelp.Installers[m].Uninstall(null);
                    }
                }
                else
                {
                    installerWithHelp.Context.LogMessage("InstallInstallNtRun");
                    for (int n = 0; n < installerWithHelp.Installers.Count; n++)
                    {
                        installerWithHelp.Installers[n].Install(null);
                    }
                }
            }
            else if (!flag)
            {
                IDictionary stateSaver = new Hashtable();
                installerWithHelp.Install(stateSaver);
            }
            else
            {
                installerWithHelp.Uninstall(null);
            }
        }
        catch (Exception exception3)
        {
            throw exception3;
        }
    }
}
#else
[System.ComponentModel.DesignerCategory("Class")]
public class InstallHelper
{
}
#endif
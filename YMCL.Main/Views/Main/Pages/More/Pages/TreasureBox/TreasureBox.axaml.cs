using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.More.Pages.TreasureBox;

public partial class TreasureBox : UserControl
{
    public TreasureBox()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
        ActivateBtn.Click += async (sender, e) =>
        {
            if (Const.Platform != Platform.Windows)
                Method.Ui.Toast(MainLang.ThisFeatureOnlySupportsWindowsPlatform, type: NotificationType.Error);
            else
                try
                {
                    if (Environment.OSVersion.Version.Major < 10)
                        Method.Ui.Toast(MainLang.ThisFeatureOnlySupportsWindows10AndAbove,
                            type: NotificationType.Warning);
                    if (await Method.Ui.UpgradeToAdministratorPrivilegesAsync())
                    {
                        var process = new Process();
                        process.StartInfo.FileName = @"powershell.exe";
                        process.StartInfo.Arguments = "irm https://get.activated.win | iex";
                        process.StartInfo.Verb = "runas";
                        process.Start();
                    }
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowLongException(MainLang.RunCommandFail, ex);
                }
        };
        CancelActivateWinBtn.Click += async (sender, e) =>
        {
            if (Const.Platform != Platform.Windows)
                Method.Ui.Toast(MainLang.ThisFeatureOnlySupportsWindowsPlatform, type: NotificationType.Error);
            else
                try
                {
                    var tip = await Method.Ui.ShowDialogAsync(MainLang.CancelActivateWin, MainLang.CancelActivateWinTip,
                        b_primary: MainLang.Ok, b_secondary: MainLang.Cancel);
                    if (tip == ContentDialogResult.Primary && await Method.Ui.UpgradeToAdministratorPrivilegesAsync())
                    {
                        var str = "slmgr -upk";
                        Process p = new();
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.UseShellExecute = false; //�Ƿ�ʹ�ò���ϵͳshell����
                        p.StartInfo.Verb = "runas";
                        p.StartInfo.RedirectStandardInput = true; //�������Ե��ó����������Ϣ
                        p.StartInfo.RedirectStandardOutput = true; //�ɵ��ó����ȡ�����Ϣ
                        p.StartInfo.RedirectStandardError = true; //�ض����׼�������
                        p.StartInfo.CreateNoWindow = true; //����ʾ���򴰿�
                        p.Start(); //��������
                        p.StandardInput.WriteLine(str + "&exit");
                        p.StandardInput.AutoFlush = true;
                        var output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        p.Close();
                    }
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowLongException(MainLang.RunCommandFail, ex);
                }
        };
    }

    private void ControlProperty()
    {
    }
}
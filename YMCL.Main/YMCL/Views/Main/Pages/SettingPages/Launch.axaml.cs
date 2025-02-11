using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using YMCL.Public.Classes;
using YMCL.Public.Module.IO;
using YMCL.Public.Module.Op;
using MinecraftFolder = YMCL.Public.Module.Op.MinecraftFolder;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Launch : UserControl
{
    public Launch()
    {
        InitializeComponent();
        BindingEvent();
        _ = ControlProperty();
    }

    private async System.Threading.Tasks.Task ControlProperty()
    {
        DataContext = Data.Instance;
        while (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                using var managementClass = new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory");
                using var instances = managementClass.GetInstances();
                foreach (var mo in instances)
                {
                    var size = long.Parse(mo.Properties["AvailableMBytes"].Value.ToString()!);
                    var used = Math.Round(100 - size / UiProperty.Instance.SystemMaxMem * 100, 2);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var free = Math.Round(100 - size / UiProperty.Instance.SystemMaxMem * 100 -
                                              MaxMemSlider.Value / MaxMemSlider.Maximum * 100, 2);
                        UsedMemText.Text = $"{used}%";
                        UsedMemProgressBar.Value = used;
                        CanUseMemText.Text = $"{free}%";
                    });
                }
            });
            await System.Threading.Tasks.Task.Delay(100);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            UsedMemRoot.IsVisible = false;
            CanUseMemText.IsVisible = false;
        }
    }

    private void BindingEvent()
    {
        AddMinecraftFolderBtn.Click += (_, _) => { _ = MinecraftFolder.AddByUi(this); };
        DelSelectedMinecraftFolderBtn.Click += (_, _) => { MinecraftFolder.RemoveSelected(); };
        RemoveJavaBtn.Click += (_, _) => { JavaRuntime.RemoveSelected(); };
        AutoScanJavaBtn.Click += (_, _) => { JavaRuntime.AddByAutoScan(); };
        ManualAddJavaBtn.Click += (_, _) => { _ = JavaRuntime.AddByUi(this); };
        JavaComboBox.SelectionChanged += (_, _) =>
        {
            JavaComboBox.IsVisible = false;
            JavaComboBox.IsVisible = true;
        };
    }
}
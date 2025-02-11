using System.IO;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Langs;

namespace YMCL.Views.Initialize.Pages;

public partial class JavaRuntime : UserControl
{
    public JavaRuntime()
    {
        InitializeComponent();
        BindingEvent();
        JavaRuntimeListBox.ItemsSource = Data.JavaRuntimes;
    }

    private void BindingEvent()
    {
        AutoScanJavaRuntimeBtn.Click += (_, _) =>
        {
            Public.Module.Op.JavaRuntime.AddByAutoScan();
        };
        ManualAddJavaRuntimeBtn.Click += async (_, _) =>
        {
            await Public.Module.Op.JavaRuntime.AddByUi(this);
        };
    }
}
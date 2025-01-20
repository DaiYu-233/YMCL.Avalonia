using System.IO;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

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
            Public.Module.Operate.JavaRuntime.AddByAutoScan();
        };
        ManualAddJavaRuntimeBtn.Click += async (_, _) =>
        {
            await Public.Module.Operate.JavaRuntime.AddByUi(this);
        };
    }
}
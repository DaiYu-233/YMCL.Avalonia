using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Skin;
using MinecraftLaunch.Skin.Class.Fetchers;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;
using YMCL.Public.Module.Ui;
using YMCL.Public.Module.Value;
using Setting = YMCL.Public.Enum.Setting;
using String = YMCL.Public.Const.String;

namespace YMCL.Views.Initialize.Pages;

public partial class Account : UserControl
{
    public Account()
    {
        InitializeComponent();
        BindingEvent();
        AccountListBox.ItemsSource = Data.Accounts;
    }
    
    private void BindingEvent()
    {
        AddAccountBtn.Click += async (_, _) =>
        {
            await Public.Module.Operate.Account.AddByUi(this);
        };
    }
}
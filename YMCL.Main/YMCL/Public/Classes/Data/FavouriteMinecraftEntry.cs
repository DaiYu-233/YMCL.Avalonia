using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Components.Parser;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc;
using YMCL.Public.Module.Mc.Launcher;
using YMCL.Public.Module.Value;
using String = System.String;

namespace YMCL.Public.Classes.Data;

public sealed class FavouriteMinecraftEntry : INotifyPropertyChanged
{
    private string _iconBase64;
    private string _minecraftPath;
    private string _minecraftId;
    private string _displayName;
    private string _serverUrl;

    [JsonIgnore] public Bitmap Bitmap => Converter.Base64ToBitmap(IconBase64);

    [JsonProperty]
    public string IconBase64
    {
        get => _iconBase64;
        set
        {
            if (SetField(ref _iconBase64, value)) OnPropertyChanged(nameof(Bitmap));
        }
    }

    [JsonProperty]
    public string MinecraftPath
    {
        get => _minecraftPath;
        set => SetField(ref _minecraftPath, value);
    }

    [JsonProperty]
    public string MinecraftId
    {
        get => _minecraftId;
        set => SetField(ref _minecraftId, value);
    }

    [JsonProperty]
    public string DisplayName
    {
        get => _displayName;
        set => SetField(ref _displayName, value);
    }

    [JsonProperty]
    public string ServerUrl
    {
        get => _serverUrl;
        set => SetField(ref _serverUrl, value);
    }

    public bool Equals(FavouriteMinecraftEntry? other)
    {
        return other != null && MinecraftPath == other?.MinecraftPath && MinecraftId == other.MinecraftId &&
               DisplayName == other.DisplayName && ServerUrl == other.ServerUrl;
    }

    public async void Rename()
    {
        var textBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Text = DisplayName, HorizontalAlignment = HorizontalAlignment.Stretch, Width = 500
        };
        var cr = await ShowDialogAsync(MainLang.Rename, p_content: textBox, b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok);
        if (cr != ContentDialogResult.Primary) return;
        DisplayName = textBox.Text ?? MinecraftId;
    }

    public void Del()
    {
        Const.Data.FavouriteMinecraft.Remove(this);
    }

    public async void SetIcon()
    {
        var list = await TopLevel.GetTopLevel(App.UiRoot).StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
                { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypes.ImageAll] });
        if (list.Count == 0) return;
        try
        {
            var bitmap = new Bitmap(list[0].Path.LocalPath);
            IconBase64 = Converter.BitmapToBase64(bitmap);
            Notice(MainLang.OperateSuccess);
        }
        catch
        {
            Notice(MainLang.OperateFailed);
        }

        App.UiRoot.ViewModel.Launch.InvalidateVisual();
        App.UiRoot.ViewModel.Launch.InvalidateArrange();
        App.UiRoot.ViewModel.Launch.InvalidateMeasure();
    }

    public async void SetServer()
    {
        var textBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Text = ServerUrl, HorizontalAlignment = HorizontalAlignment.Stretch, Width = 500,
            Watermark = "example.com:25565"
        };
        var cr = await ShowDialogAsync(MainLang.Rename, p_content: textBox, b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok);
        if (cr != ContentDialogResult.Primary) return;
        ServerUrl = textBox.Text ?? string.Empty;
    }

    public void Launch()
    {
        if (!Directory.Exists(MinecraftPath))
        {
            Notice(MainLang.FolderNotExist, NotificationType.Error);
            return;
        }

        var parser = new MinecraftParser(MinecraftPath);
        var entry = parser.GetMinecraft(MinecraftId);
        if (entry == null)
        {
            Notice(MainLang.CreateGameEntryFail, NotificationType.Error);
            return;
        }

        var setting = MinecraftSetting.GetGameSetting(entry);
        MinecraftSetting.HandleGameSetting(setting);
        if (setting.Java.JavaStringVersion == "Auto")
        {
            setting.Java =
                YMCL.Public.Module.Value.Calculator.GetCurrentJava(Const.Data.JavaRuntimes.ToList(), entry);
        }

        if (setting.Java.JavaPath == "Error")
        {
            Notice($"{MainLang.CannotFandRightJava}\n{setting.Java.JavaStringVersion}", NotificationType.Error);
            return;
        }
        
        if (setting.Java == null)
        {
            Notice(MainLang.JavaRuntimeError, NotificationType.Error);
            return;
        }

        _ = JavaClient.Launch(MinecraftId, entry.MinecraftFolderPath, setting.MaxMem,
            JavaEntry.YmclToMl(setting.Java),
            p_fullUrl: !string.IsNullOrWhiteSpace(ServerUrl) ? ServerUrl : setting.AutoJoinServerIp);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public FavouriteMinecraftEntry()
    {
        PropertyChanged += (_, _) =>
        {
            File.WriteAllText(ConfigPath.FavouriteMinecraftDataPath,
                JsonConvert.SerializeObject(Const.Data.FavouriteMinecraft, Formatting.Indented));
        };
    }
}
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Module.App.Init.SubModule;

namespace YMCL.Public.Module.App;

public class Setting
{
    public static string Export(bool ui, bool net, bool launch, bool other, bool accounts)
    {
        var s = Data.SettingEntry;
        var data = new ExchangeSettingEntry.Data()
        {
            NetworkSettings = net
                ? new ExchangeSettingEntry.NetworkSettings()
                {
                    EnableAutoCheckUpdate = s.EnableAutoCheckUpdate,
                    MaxDownloadThread = s.MaxDownloadThread,
                    CustomUpdateUrl = s.CustomUpdateUrl,
                    EnableCustomUpdateUrl = s.EnableCustomUpdateUrl,
                    MusicApiWithIPAddress = s.MusicApiWithIPAddress,
                    DownloadSource = s.DownloadSource,
                    MusicApi = s.MusicApi,
                    MaxFileFragmentation = s.MaxFileFragmentation
                }
                : null,

            LaunchSettings = launch
                ? new ExchangeSettingEntry.LaunchSettings()
                {
                    MaxMem = s.MaxMem,
                    EnableIndependencyCore = s.EnableIndependencyCore,
                    EnableAutoAllocateMem = s.EnableAutoAllocateMem
                }
                : null,

            UiSettings = ui
                ? new ExchangeSettingEntry.UiSettings()
                {
                    TranslucentBackgroundOpacity = s.TranslucentBackgroundOpacity, Theme = s.Theme,
                    LauncherVisibility = s.LauncherVisibility,
                    SpecialControlEnableTranslucent = s.SpecialControlEnableTranslucent,
                    CornerRadius = s.CornerRadius,
                    CustomHomePageUrl = s.CustomHomePageUrl,
                    DeskLyricColor = s.DeskLyricColor,
                    NoticeWay = s.NoticeWay,
                    AccentColor = s.AccentColor,
                    Language = s.Language,
                    CustomBackGround = s.CustomBackGround,
                    DeskLyricSize = s.DeskLyricSize,
                    DeskLyricAlignment = s.DeskLyricAlignment,
                    WindowBackGroundImgData = s.WindowBackGroundImgData
                }
                : null,

            OtherSettings = other
                ? new ExchangeSettingEntry.OtherSettings() { Volume = s.Volume, Repeat = s.Repeat }
                : null,
            AccountSettings = accounts
                ? Data.Accounts.Where(x => x.AccountType != Enum.Setting.AccountType.Microsoft)
                : null
        };
        var json = JsonConvert.SerializeObject(data, Formatting.None);
        var bytes = Encoding.UTF8.GetBytes(json);
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public static (bool success, ExchangeSettingEntry.Data? data) Import(string hex)
    {
        if (hex.Length % 2 != 0)
            return (false, null);

        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        var json = Encoding.UTF8.GetString(bytes);
        return (true, JsonConvert.DeserializeObject<ExchangeSettingEntry.Data>(json));
    }

    public static void Replace(SettingEntry s, ExchangeSettingEntry.Data data)
    {
        if (data.NetworkSettings != null)
        {
            var networkSettingsType = typeof(ExchangeSettingEntry.NetworkSettings);
            var networkProperties = networkSettingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var sType = s.GetType();
            foreach (var prop in networkProperties)
            {
                var sProp = sType.GetProperty(prop.Name);
                if (sProp != null && sProp.CanWrite)
                {
                    sProp.SetValue(s, prop.GetValue(data.NetworkSettings));
                }
            }
        }

        // 处理 LaunchSettings
        if (data.LaunchSettings != null)
        {
            var launchSettingsType = typeof(ExchangeSettingEntry.LaunchSettings);
            var launchProperties = launchSettingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var sType = s.GetType();
            foreach (var prop in launchProperties)
            {
                var sProp = sType.GetProperty(prop.Name);
                if (sProp != null && sProp.CanWrite)
                {
                    sProp.SetValue(s, prop.GetValue(data.LaunchSettings));
                }
            }
        }

        // 处理 UiSettings
        if (data.UiSettings != null)
        {
            var uiSettingsType = typeof(ExchangeSettingEntry.UiSettings);
            var uiProperties = uiSettingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var sType = s.GetType();
            foreach (var prop in uiProperties)
            {
                var sProp = sType.GetProperty(prop.Name);
                if (sProp != null && sProp.CanWrite)
                {
                    sProp.SetValue(s, prop.GetValue(data.UiSettings));
                }
            }
        }

        // 处理 OtherSettings
        if (data.OtherSettings != null)
        {
            var otherSettingsType = typeof(ExchangeSettingEntry.OtherSettings);
            var otherProperties = otherSettingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var sType = s.GetType();
            foreach (var prop in otherProperties)
            {
                var sProp = sType.GetProperty(prop.Name);
                if (sProp != null && sProp.CanWrite)
                {
                    sProp.SetValue(s, prop.GetValue(data.OtherSettings));
                }
            }
        }

        if (data.AccountSettings != null)
        {
            data.AccountSettings.ToList().ForEach(x =>
            {
                if (!Data.Accounts.Contains(x))
                {
                    Data.Accounts.Add(x);
                }
            });
            File.WriteAllText(ConfigPath.AccountDataPath,
                JsonConvert.SerializeObject(Data.Accounts, Formatting.Indented));
            Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
        }

        _ = InitUi.Dispatch();
    }
}
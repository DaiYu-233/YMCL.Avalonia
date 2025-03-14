﻿using System.IO;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc;
using YMCL.Public.Module.Mc.Launcher;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Public.Classes.Operate;

public sealed record MinecraftDataEntry
{
    [JsonIgnore] public bool IsFavourite { get; }
    [JsonIgnore] public Action? LaunchAction { get; set; }
    [JsonIgnore] public bool IsSettingVisible { get; set; } = true;
    [JsonIgnore] public string FavouriteIcon { get; set; }
    public string ListTip { get; set; }
    public string Loaders { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
    [JsonIgnore] public Bitmap Icon => Public.Module.Mc.Icon.GetMinecraftIcon(this);
    public MinecraftEntry MinecraftEntry { get; set; }

    public async Task SettingCommand()
    {
        await YMCL.App.UiRoot.ViewModel.Launch.CloseGameList(false);
        _ = YMCL.App.UiRoot.ViewModel.Launch.OpenGameSetting(MinecraftEntry);
    }

    public void LaunchCommand()
    {
        LaunchAction?.Invoke();
    }

    public void FavouriteCommand()
    {
        if (string.IsNullOrWhiteSpace(MinecraftEntry.ClientJarPath)) return;
        var path = Path.Combine(MinecraftEntry.MinecraftFolderPath, "versions", MinecraftEntry.Id, "YMCL.Favourite");
        YMCL.App.UiRoot.ViewModel.Launch._gameList.CanCloseGameList = false;
        var favourite = File.Exists(path);
        try
        {
            if (!favourite)
            {
                File.WriteAllText(path, string.Empty);
            }
            else
            {
                File.Delete(path);
            }
        }
        catch
        {
        }

        LaunchUi.LoadGames();
        YMCL.App.UiRoot.ViewModel.Launch._gameList.CanCloseGameList = true;
    }

    public MinecraftDataEntry(MinecraftEntry minecraftEntry, bool favourite = false, bool isBedrock = false)
    {
        if (minecraftEntry == null && !isBedrock) return;
        if (isBedrock)
            Type = "bedrock";
        IsFavourite = favourite;
        if (Type != "bedrock")
        {
            Id = minecraftEntry.Id;
            ListTip = minecraftEntry.Version.VersionId;
            MinecraftEntry = minecraftEntry;
            Loaders = MinecraftEntry.IsVanilla
                ? "Vanilla"
                : string.Join(" , ",
                    (MinecraftEntry as ModifiedMinecraftEntry)?.ModLoaders.Select(a => $"{a.Type} {a.Version}")!);
            FavouriteIcon = favourite
                ? "F1 M 4.21875 19.53125 C 4.049479 19.53125 3.902995 19.467773 3.779297 19.34082 C 3.655599 19.213867 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 L 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.571939 0.633139 9.672852 0.571289 C 9.773763 0.509441 9.879557 0.478516 9.990234 0.478516 C 10.107422 0.478516 10.218099 0.507812 10.322266 0.566406 C 10.426432 0.625 10.504557 0.709637 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.082683 19.918619 8.232422 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.420573 19.505209 4.322917 19.53125 4.21875 19.53125 Z "
                : "F1 M 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.573567 0.633139 9.677734 0.571289 C 9.7819 0.509441 9.889322 0.478516 10 0.478516 C 10.110677 0.478516 10.218099 0.509441 10.322266 0.571289 C 10.426432 0.633139 10.504557 0.716146 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.076172 19.918619 8.225912 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.427083 19.498697 4.329427 19.521484 4.21875 19.521484 C 4.049479 19.521484 3.902995 19.459635 3.779297 19.335938 C 3.655599 19.21224 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 Z M 5.048828 17.753906 L 10 15.146484 L 14.951172 17.753906 C 14.794922 16.829428 14.640299 15.909831 14.487305 14.995117 C 14.33431 14.080404 14.173177 13.160808 14.003906 12.236328 L 18.017578 8.330078 L 12.480469 7.529297 C 12.057291 6.689453 11.642252 5.852865 11.235352 5.019531 C 10.82845 4.186199 10.416666 3.349609 10 2.509766 C 9.583333 3.349609 9.171549 4.186199 8.764648 5.019531 C 8.357747 5.852865 7.942708 6.689453 7.519531 7.529297 L 1.982422 8.330078 L 5.996094 12.236328 C 5.826823 13.160808 5.66569 14.080404 5.512695 14.995117 C 5.3597 15.909831 5.205078 16.829428 5.048828 17.753906 Z ";
        }
        else
        {
            Id = MainLang.BedRockVersion;
            ListTip = "如未安装基岩版则无法启动";
            IsFavourite = true;
        }

        InitLaunchAction(minecraftEntry);
    }

    private void InitLaunchAction(MinecraftEntry? entry)
    {
        if (Type == "bedrock")
        {
            LaunchAction = () =>
            {
                if (YMCL.App.UiRoot != null)
                    YMCL.Public.Module.Mc.Launcher.BedRock.Launch(YMCL.App.UiRoot);
            };
        }
        else
        {
            if (entry == null) return;
            LaunchAction = () =>
            {
                var setting = MinecraftSetting.GetGameSetting(entry);
                MinecraftSetting.HandleGameSetting(setting);
                if (setting.Java.JavaStringVersion == "Auto")
                {
                    setting.Java =
                        YMCL.Public.Module.Value.Calculator.GetCurrentJava(Const.Data.JavaRuntimes.ToList<JavaEntry>(), entry);
                }

                if (setting.Java == null)
                {
                    Notice(MainLang.CannotFandRightJava, NotificationType.Error);
                    return;
                }
                _ = JavaClient.Launch(Id, entry.MinecraftFolderPath, setting.MaxMem,
                    JavaEntry.YmclToMl(setting.Java), p_fullUrl: setting.AutoJoinServerIp);
            };
        }
    }

    public bool Equals(MinecraftDataEntry? other)
    {
        return other != null && Id == other.Id;
    }
}
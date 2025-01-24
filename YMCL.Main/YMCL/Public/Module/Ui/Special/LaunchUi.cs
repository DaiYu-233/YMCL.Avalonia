using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Data;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;
using YMCL.Public.Classes;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Ui.Special;

public class LaunchUi
{
    public static void LoadGames()
    {
        if (Data.Setting == null) return;
        if (Data.Setting.MinecraftFolder == null) return;
        GameDataEntry? selected = null;
        if (Data.CurrentFolderGames.Count > 0)
        {
            selected = Data.Setting.SelectedGame;
        }

        List<GameDataEntry> games = [];
        var resolver = new GameResolver(Data.Setting.MinecraftFolder.Path);
        resolver.GetGameEntitys().ToList().ForEach(a =>
        {
            try
            {
                var favourite = File.Exists(Path.Combine(a.GameFolderPath, "versions", a.Id, "YMCL.Favourite"));
                games.Add(new GameDataEntry(a, favourite));
            }
            catch
            {
            }
        });
        Data.CurrentFolderGames.Clear();
        games.OrderBy(entry => !entry.IsFavourite).ToList().ForEach(a => { Data.CurrentFolderGames.Add(a); });
        var bedrock = new GameDataEntry(new GameEntry { Id = "基岩版", MainLoaderType = LoaderType.Vanilla, Type = "bedrock", Version = "如未安装基岩版则无法启动" }, true)
            { IsSettingVisible = false };
        Data.CurrentFolderGames.Insert(0, bedrock);
        if (Data.Setting.SelectedGame == null || !Data.CurrentFolderGames.Contains(Data.Setting.SelectedGame))
        {
            Data.Setting.SelectedGame = Data.CurrentFolderGames.FirstOrDefault();
        }
        else
        {
            var index = Data.CurrentFolderGames.IndexOf(Data.Setting.SelectedGame);
            Data.Setting.SelectedGame = null;
            Data.Setting.SelectedGame = Data.CurrentFolderGames[index];
        }

        if (selected != null && Data.CurrentFolderGames.Contains(selected))
        {
            Data.Setting.SelectedGame = selected;
        }
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Data;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Ui.Special;

public class LaunchUi
{
    public static void LoadGames()
    {
        if (Data.SettingEntry == null) return;
        if (Data.SettingEntry.MinecraftFolder == null) return;
        MinecraftDataEntry? selected = null;
        if (Data.CurrentFolderGames.Count > 0)
        {
            selected = Data.UiProperty.SelectedMinecraft;
        }

        List<MinecraftDataEntry> games = [];
        var resolver = new MinecraftParser(Data.SettingEntry.MinecraftFolder.Path);
        resolver.GetMinecrafts().ToList().ForEach(a =>
        {
            try
            {
                var favourite = File.Exists(Path.Combine(a.MinecraftFolderPath, "versions", a.Id, "YMCL.Favourite"));
                games.Add(new MinecraftDataEntry(a, favourite));
            }
            catch
            {
            }
        });
        Data.CurrentFolderGames.Clear();
        games.OrderBy(entry => !entry.IsFavourite).ToList().ForEach(a => { Data.CurrentFolderGames.Add(a); });
        var bedrock = new MinecraftDataEntry(null, true, true) { IsSettingVisible = false, Type = "bedrock" };
        Data.CurrentFolderGames.Insert(0, bedrock);
        if (Data.UiProperty.SelectedMinecraft == null ||
            !Data.CurrentFolderGames.Contains(Data.UiProperty.SelectedMinecraft))
        {
            Data.UiProperty.SelectedMinecraft = Data.CurrentFolderGames.FirstOrDefault();
        }
        else
        {
            var index = Data.CurrentFolderGames.IndexOf(Data.UiProperty.SelectedMinecraft);
            Data.UiProperty.SelectedMinecraft = null;
            Data.UiProperty.SelectedMinecraft = Data.CurrentFolderGames[index];
        }

        if (selected != null && Data.CurrentFolderGames.Contains(selected))
        {
            Data.UiProperty.SelectedMinecraft = selected;
        }
    }
}
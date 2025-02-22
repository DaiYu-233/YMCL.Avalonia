using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;

namespace YMCL.Views.Initialize.Pages;

public partial class MinecraftFolder : UserControl
{
    public MinecraftFolder()
    {
        InitializeComponent();
        MinecraftFolderListBox.ItemsSource = Data.MinecraftFolders;
        ManualAddMinecraftFolderBtn.Click += async (_, _) =>
        {
            await Public.Module.Op.MinecraftFolder.AddByUi(this);
        };
        MinecraftFolderListBox.SelectionChanged += (_, _) =>
        {
            Data.Setting.MinecraftFolder = MinecraftFolderListBox.SelectedItem as Public.Classes.Data.MinecraftFolder;
        };
    }
}
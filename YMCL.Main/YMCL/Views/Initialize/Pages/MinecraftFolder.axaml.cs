﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

namespace YMCL.Views.Initialize.Pages;

public partial class MinecraftFolder : UserControl
{
    public MinecraftFolder()
    {
        InitializeComponent();
        MinecraftFolderListBox.ItemsSource = Data.Instance.MinecraftFolders;
        ManualAddMinecraftFolderBtn.Click += async (_, _) =>
        {
            await Public.Module.Operate.MinecraftFolder.AddByUi(this);
        };
        MinecraftFolderListBox.SelectionChanged += (_, _) =>
        {
            Data.Instance.Setting.MinecraftFolder = MinecraftFolderListBox.SelectedItem as Public.Classes.MinecraftFolder;
        };
    }
}
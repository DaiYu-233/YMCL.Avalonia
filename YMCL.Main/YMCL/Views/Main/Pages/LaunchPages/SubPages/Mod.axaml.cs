using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json.Linq;
using Tomlyn;
using Tomlyn.Model;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using SearchOption = System.IO.SearchOption;
using String = System.String;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public sealed partial class Mod : UserControl, INotifyPropertyChanged
{
    private readonly ObservableCollection<LocalModEntry> _mods = [];
    public ObservableCollection<LocalModEntry> FilteredMods { get; set; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    private readonly MinecraftEntry _entry;
    private string _filter = string.Empty;
    private bool _isLoading;

    public Mod(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        LoadMods();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter))
            {
                FilterMods();
            }
        };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadMods(); };
        // Loaded += (_, _) => { LoadMods(); };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        DisableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as LocalModEntry;
                if (mod.FileName.Length <= 0) continue;
                if (Path.GetExtension(mod.Path) == ".jar")
                    File.Move(mod.Path, mod.Path + ".disabled");
            }

            LoadMods();
        };
        EnableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as LocalModEntry;
                if (mod.FileName.Length <= 0) continue;
                if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(mod.Path))) continue;
                if (Path.GetExtension(mod.Path) == ".disabled")
                    File.Move(mod.Path, Path.Combine(Path.GetDirectoryName(mod.Path)!, $"{mod.FileName}.jar"));
            }

            LoadMods();
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            if (mods is null) return;
            var text = (from object? item in mods select item as LocalModEntry).Aggregate(string.Empty,
                (current, mod) => current + $"• {Path.GetFileName(mod.FileName)}\n");

            var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in mods)
            {
                var mod = item as LocalModEntry;
                if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
                {
                    FileSystem.DeleteFile(mod.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                }
                else
                {
                    File.Delete(mod.Path);
                }
            }

            LoadMods();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";
    }

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    private async void LoadMods()
    {
        _mods.Clear();
        IsLoading = true;
        FilterMods();

        var mods = Directory.GetFiles(
            Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_entry, GameSpecialFolder.ModsFolder)
            , "*.*", SearchOption.AllDirectories);
        foreach (var mod in mods)
        {
            LocalModEntry? localModEntry = null;
            if (Path.GetExtension(mod) == ".jar")
            {
                localModEntry = new LocalModEntry
                {
                    FileName = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 4)],
                    IsEnable = true, Path = mod, Callback = LoadMods,
                    DisplayText = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 4)]
                };
            }

            if (Path.GetExtension(mod) == ".disabled")
                localModEntry = new LocalModEntry
                {
                    FileName = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 13)],
                    IsEnable = false, Path = mod, Callback = LoadMods,
                    DisplayText = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 13)],
                };

            if (localModEntry == null) continue;

            var (displayName, description) = await GetModInfo(mod);

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                localModEntry.DisplayText = $"{displayName.Trim()} ({localModEntry.FileName})";
                localModEntry.ModInfoName = $"{displayName.Trim()}";
                localModEntry.ShouldTranslateInfoName = true;
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                localModEntry.Description = description.Trim();
                localModEntry.ShouldTranslateDescription = true;
            }
            else
                localModEntry.Description = MainLang.NoDescription;

            if (_mods.All(item => item.Path != localModEntry.Path))
            {
                _mods.Add(localModEntry);
            }

            // if (localModEntry.DisplayText.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            // {
            //     if (FilteredMods.All(item => item.Path != localModEntry.Path))
            //     {
            //         FilteredMods.Add(localModEntry);
            //     }
            // }

            Translate(localModEntry);
        }

        IsLoading = false;
        FilterMods();
    }

    private async Task<(string? displayName, string? description)> GetModInfo(string path)
    {
        var result = await System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                if (!File.Exists(path)) return (null, null);
                using var archive = ZipFile.OpenRead(path);
                if (archive.Entries.Count <= 0) return (null, null);
                var type1 = archive.GetEntry("META-INF/mods.toml");
                if (type1 != null)
                {
                    try
                    {
                        await using var entryStream = type1.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var result = Toml.ToModel(text);

                        if (result.TryGetValue("mods", out var modsObj))
                        {
                            if (modsObj is TomlTableArray modsList)
                                foreach (var modTable in modsList.OfType<TomlTable>())
                                {
                                    var displayName = modTable.TryGetValue("displayName", out var nameObj)
                                        ? !string.IsNullOrWhiteSpace(nameObj.ToString()) ? nameObj.ToString() : null
                                        : null;
                                    var description = modTable.TryGetValue("description", out var descObj)
                                        ? !string.IsNullOrWhiteSpace(descObj.ToString()) ? descObj.ToString() : null
                                        : null;
                                    if (!string.IsNullOrWhiteSpace(displayName) ||
                                        !string.IsNullOrWhiteSpace(description))
                                    {
                                        return (displayName, description);
                                    }
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                var type2 = archive.GetEntry("fabric.mod.json");
                if (type2 != null)
                {
                    try
                    {
                        await using var entryStream = type2.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var json = JObject.Parse(text);

                        var displayName = !string.IsNullOrWhiteSpace(json["name"]?.ToString())
                            ? json["name"]?.ToString()
                            : null;
                        var description = !string.IsNullOrWhiteSpace(json["description"]?.ToString())
                            ? json["description"]?.ToString()
                            : null;
                        if (!string.IsNullOrWhiteSpace(displayName) ||
                            !string.IsNullOrWhiteSpace(description))
                        {
                            return (displayName, description);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                var type3 = archive.GetEntry("mcmod.info");
                if (type3 != null)
                {
                    try
                    {
                        await using var entryStream = type3.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var obj = JArray.Parse(text).FirstOrDefault();
                        if (obj is not JObject o)
                            throw new Exception("mcmod.info is not a valid json array");

                        var displayName = !string.IsNullOrWhiteSpace(o["name"]?.ToString())
                            ? o["name"]?.ToString()
                            : null;
                        var description = !string.IsNullOrWhiteSpace(o["description"]?.ToString())
                            ? o["description"]?.ToString()
                            : null;
                        if (!string.IsNullOrWhiteSpace(displayName) ||
                            !string.IsNullOrWhiteSpace(description))
                        {
                            return (displayName, description);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                var type4 = archive.GetEntry("META-INF/neoforge.mods.toml");
                if (type4 != null)
                {
                    try
                    {
                        await using var entryStream = type4.Open();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var text = await reader.ReadToEndAsync();
                        var result = Toml.ToModel(text);

                        if (result.TryGetValue("mods", out var modsObj))
                        {
                            if (modsObj is TomlTableArray modsList)
                                foreach (var modTable in modsList.OfType<TomlTable>())
                                {
                                    var displayName = modTable.TryGetValue("displayName", out var nameObj)
                                        ? !string.IsNullOrWhiteSpace(nameObj.ToString()) ? nameObj.ToString() : null
                                        : null;
                                    var description = modTable.TryGetValue("description", out var descObj)
                                        ? !string.IsNullOrWhiteSpace(descObj.ToString()) ? descObj.ToString() : null
                                        : null;
                                    if (!string.IsNullOrWhiteSpace(displayName) ||
                                        !string.IsNullOrWhiteSpace(description))
                                    {
                                        return (displayName, description);
                                    }
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                return (null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return (null, null);
            }
        });
        return (result.displayName, result.description);
    }

    private void FilterMods()
    {
        FilteredMods.Clear();
        _mods.Where(item => item.DisplayText.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.IsEnable).ToList().ForEach(mod =>
            {
                if (FilteredMods.All(item => item.Path != mod.Path))
                {
                    FilteredMods.Add(mod);
                }
            });
        NoMatchResultTip.IsVisible = FilteredMods.Count == 0 && !IsLoading;
        SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
    }


    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    private void Translate(LocalModEntry entry)
    {
        if (string.IsNullOrWhiteSpace(Data.TranslateToken)) return;
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            if (!entry.ShouldTranslateInfoName) return;
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (_, _, _, _) => true;
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Authorization", Data.TranslateToken);
                var response =
                    await client.PostAsync(
                        $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={Data.Setting.Language.Code}&textType=plain",
                        new StringContent($"[{{\"Text\": \"{entry.ModInfoName}\"}}]", Encoding.UTF8,
                            "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                var translatedText =
                    ((JObject)JArray.Parse(responseContent)[0]["translations"][0])["text"].ToString();
                entry.DisplayText = $"{translatedText} ({entry.ModInfoName}) ({entry.FileName})";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            if (!entry.ShouldTranslateDescription) return;
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (_, _, _, _) => true;
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Authorization", Data.TranslateToken);
                var response =
                    await client.PostAsync(
                        $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={Data.Setting.Language.Code}&textType=plain",
                        new StringContent($"[{{\"Text\": \"{entry.ModInfoName}\"}}]", Encoding.UTF8,
                            "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                var translatedText =
                    ((JObject)JArray.Parse(responseContent)[0]["translations"][0])["text"].ToString();
                entry.Description = translatedText.Trim();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }

    public Mod()
    {
    }
}
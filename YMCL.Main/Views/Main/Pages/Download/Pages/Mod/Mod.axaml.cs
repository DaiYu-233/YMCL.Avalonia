using Avalonia.Controls;
using CurseForge.APIClient;
using CurseForge.APIClient.Models.Mods;
using CurseForge.APIClient.Models;
using System;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;
using System.Collections.Generic;
using YMCL.Main.Public.Classes;
using System.Globalization;
using CurseForge.APIClient.Models.Games;
using Avalonia;
using System.Linq;
using YMCL.Main.Public.Controls;
using static YMCL.Main.Public.Classes.SearchModListViewItemEntry;
using CurseForge.APIClient.Models.Files;
using System.IO;
using System.Net.Http;

namespace YMCL.Main.Views.Main.Pages.Download.Pages.Mod
{
    public partial class Mod : UserControl
    {
        readonly ApiClient cfApiClient = new(Const.CurseForgeApiKey);
        readonly int _gameId = 432;
        int _page = 0;
        bool _firstOpenModInfo = true;
        string _keyword;
        ModLoaderType _loaderType;
        string _gameVersion;

        public class VersionComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                string[] versionPartsX = x.Split('.');
                string[] versionPartsY = y.Split('.');

                int minLength = Math.Min(versionPartsX.Length, versionPartsY.Length);

                for (int i = 0; i < minLength; i++)
                {
                    int partX = int.Parse(versionPartsX[i]);
                    int partY = int.Parse(versionPartsY[i]);

                    if (partX != partY)
                    {
                        return partX.CompareTo(partY);
                    }
                }
                // 如果所有相同位置的版本号都相同，但长度不同，则较长的版本号应该更大  
                return versionPartsX.Length.CompareTo(versionPartsY.Length);
            }
        }

        Dictionary<DaiYuLoaderType, ModLoaderType> mapping = new()
        {
            { DaiYuLoaderType.Any, ModLoaderType.Any },
            { DaiYuLoaderType.Forge, ModLoaderType.Forge },
            { DaiYuLoaderType.NeoForge, ModLoaderType.NeoForge },
            { DaiYuLoaderType.Fabric, ModLoaderType.Fabric },
            { DaiYuLoaderType.Quilt, ModLoaderType.Quilt },
            { DaiYuLoaderType.LiteLoader, ModLoaderType.LiteLoader }
        };
        public Mod()
        {
            InitializeComponent();
            ControlProperty();
            BindingEvent();
            Loaded += (_, _) =>
            {
            };
        }
        private void BindingEvent()
        {
            Loaded += (_, _) =>
            {
                Method.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            };
            ModNameTextBox.KeyDown += (_, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    _page = 0;
                    ModNameTextBox.IsEnabled = false;
                    SearchBtn.IsEnabled = false;
                    Loading.IsVisible = true;
                    LoadMoreBtn.IsVisible = false;
                    ModListView.Items.Clear();
                    SearchModFromCurseForge();
                }
            };
            ModVersionTextBox.KeyDown += (_, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    _page = 0;
                    ModNameTextBox.IsEnabled = false;
                    SearchBtn.IsEnabled = false;
                    Loading.IsVisible = true;
                    LoadMoreBtn.IsVisible = false;
                    ModListView.Items.Clear();
                    SearchModFromCurseForge();
                }
            };
            CloseModInfoBtn.Click += (_, _) =>
            {
                ModInfoRoot.Margin = new Avalonia.Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
                ModListView.IsVisible = true;
            };
            ModListView.SelectionChanged += async (_, e) =>
            {
                if (ModListView.SelectedIndex == -1) return;
                if (_firstOpenModInfo)
                {
                    ModInfoRoot.Margin = new Avalonia.Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
                    ModInfoRoot.IsVisible = true;
                    _firstOpenModInfo = false;
                    ModInfoRoot.Margin = new Avalonia.Thickness(10);
                }
                else
                {
                    ModInfoRoot.Margin = new Avalonia.Thickness(10);
                }
                ModListView.IsVisible = false;
                ModFileLoading.IsVisible = true;
                var entry = ModListView.SelectedItem as SearchModListViewItemEntry;
                ModFileVersionPanel.Children.Clear();
                var topChildPanel = new StackPanel();
                ModFileVersionPanel.Children.Add(topChildPanel);
                ModInfoName.Text = entry.Name;
                ModInfoModSource.Text = entry.ModSource.ToString();
                ModInfoStringDateTime.Text = entry.StringDateTime;
                ModInfoSummary.Text = entry.Summary;
                ModInfoIcon.Url = entry.Logo.Url;
                ModInfoStringDownloadCount.Text = entry.StringDownloadCount;
                ModListView.SelectedIndex = -1;
                _ = ModInfoIcon.RefreshImgAsync();
                var modFiles = new List<CurseForge.APIClient.Models.Files.File>();
                var index = 0;
                var shouldReturn = false;
                while (true)
                {
                    GenericListResponse<CurseForge.APIClient.Models.Files.File> files = new();
                    try
                    {
                        files = await cfApiClient.GetModFilesAsync(entry.Id, pageSize: 40, index: index * 40);
                    }
                    catch (Exception ex)
                    {
                        Method.ShowShortException(MainLang.ErrorCallingApi, ex);
                        shouldReturn = true;
                        break;
                    }
                    if (files.Data.Count == 0) break;
                    else
                    {
                        files.Data.ForEach(file =>
                        {
                            modFiles.Add(file);
                        });
                        index++;
                    }
                }
                ModFileLoading.IsVisible = false;
                if (shouldReturn) return;
                List<string> mcVersions = new();
                foreach (var file in modFiles)
                {
                    if (!mcVersions.Contains(file.SortableGameVersions[0].GameVersion!))
                    {
                        mcVersions.Add(file.SortableGameVersions[0].GameVersion!);
                    }
                }
                mcVersions = mcVersions.Where(s => !string.IsNullOrEmpty(s)).ToList();
                mcVersions.Sort(new VersionComparer());
                mcVersions.Reverse();
                mcVersions.ForEach(mcVersion =>
                {
                    var expander = new ModFileView(mcVersion);
                    expander.ListView.SelectionChanged += ModFileSelectionChanged;
                    expander.Name = $"mod_{mcVersion.Replace(".", "_")}";
                    foreach (var file in modFiles)
                    {
                        if (file.GameVersions[0].ToString() == mcVersion)
                        {
                            var item = new ModFileListViewItemEntry(file);
                            item.StringDownloadCount = Method.ConvertToWanOrYi(file.DownloadCount);
                            DateTime localDateTime = file.FileDate.DateTime.ToLocalTime();
                            string formattedDateTime = localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            item.StringDateTime = formattedDateTime;
                            var loader = string.Empty;
                            try
                            {
                                loader = file.SortableGameVersions[1].GameVersionName;
                            }
                            catch
                            {
                                loader = "Null";
                            }
                            item.Loader = loader;
                            expander.ListView.Items.Add(item);
                        }
                    }
                    ModFileVersionPanel.Children.Add(expander);
                    if (mcVersion == _gameVersion && !string.IsNullOrEmpty(_gameVersion))
                    {
                        var matchedExpander = new ModFileView($"{MainLang.MatchingVersion} - {mcVersion}");
                        matchedExpander.Margin = new Thickness(0, 10, 0, 0);
                        matchedExpander.ListView.SelectionChanged += ModFileSelectionChanged;
                        foreach (var file in modFiles)
                        {
                            if (file.GameVersions[0].ToString() == mcVersion)
                            {
                                var item = new ModFileListViewItemEntry(file);
                                item.StringDownloadCount = Method.ConvertToWanOrYi(file.DownloadCount);
                                DateTime localDateTime = file.FileDate.DateTime.ToLocalTime();
                                string formattedDateTime = localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                                item.StringDateTime = formattedDateTime;
                                var loader = string.Empty;
                                try
                                {
                                    loader = file.SortableGameVersions[1].GameVersionName;
                                }
                                catch
                                {
                                    loader = "Null";
                                }
                                item.Loader = loader;
                                matchedExpander.ListView.Items.Add(item);
                            }
                        }
                        topChildPanel.Children.Add(matchedExpander);
                    }
                });
            };
            SearchBtn.Click += (_, _) =>
            {
                _page = 0;
                ModNameTextBox.IsEnabled = false;
                SearchBtn.IsEnabled = false;
                Loading.IsVisible = true;
                LoadMoreBtn.IsVisible = false;
                ModListView.Items.Clear();
                SearchModFromCurseForge();
            };
            LoadMoreBtn.Click += async (_, _) =>
            {
                _page++;
                Loading.IsVisible = true;
                LoadMoreBtn.IsVisible = false;
                ModListViewScroll.ScrollToEnd();
                try
                {
                    GenericListResponse<CurseForge.APIClient.Models.Mods.Mod> mods;
                    if (_loaderType == ModLoaderType.Any)
                    {
                        mods = await cfApiClient.SearchModsAsync(gameId: _gameId, searchFilter: _keyword, gameVersion: _gameVersion, index: _page * 25, pageSize: 25);
                    }
                    else
                    {
                        mods = await cfApiClient.SearchModsAsync(gameId: _gameId, searchFilter: _keyword, modLoaderType: _loaderType, gameVersion: _gameVersion, index: _page * 25, pageSize: 25);
                    }
                    mods.Data.ForEach(mod =>
                    {
                        var entry = new SearchModListViewItemEntry(mod);
                        entry.StringDownloadCount = Method.ConvertToWanOrYi(mod.DownloadCount);
                        DateTime localDateTime = mod.DateReleased.DateTime.ToLocalTime();
                        string formattedDateTime = localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        entry.StringDateTime = formattedDateTime;
                        entry.ModSource = ModSource.CurseForge;
                        ModListView.Items.Add(entry);
                    });
                    ModNameTextBox.IsEnabled = true;
                    SearchBtn.IsEnabled = true;
                    Loading.IsVisible = false;
                    LoadMoreBtn.IsVisible = true;
                    if (mods.Data.Count == 0)
                    {
                        Method.Toast(MainLang.SearchNoResult);
                        LoadMoreBtn.IsVisible = false;
                    }
                }
                catch (Exception ex)
                {
                    ModNameTextBox.IsEnabled = true;
                    SearchBtn.IsEnabled = true;
                    Loading.IsVisible = false;
                    LoadMoreBtn.IsVisible = false;
                    Method.ShowShortException(MainLang.ErrorCallingApi, ex);
                    return;
                }
            };
        }

        private async void ModFileSelectionChanged(object? s, SelectionChangedEventArgs e)
        {
            var sender = s as ListBox;
            if (sender.SelectedIndex >= 0)
            {
                var item = sender.SelectedItem as ModFileListViewItemEntry;
                sender.SelectedIndex = -1;
                var path = await Method.SaveFilePicker(TopLevel.GetTopLevel(this)!, new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    SuggestedFileName = item.DisplayName,
                    DefaultExtension = "jar",
                    ShowOverwritePrompt = true,
                    Title = MainLang.SaveFile
                });
                if (path == null) return;
                Method.Toast($"{MainLang.BeginDownload}：{item.DisplayName}");
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage response = await client.GetAsync(item.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();

                            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                   fileStream = System.IO.File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                    Method.Toast($"{MainLang.DownloadFinish}：{item.DisplayName}",type:Avalonia.Controls.Notifications.NotificationType.Success);
                }
                catch (HttpRequestException ex)
                {
                    Method.ShowShortException($"{MainLang.DownloadFail}：{item.DisplayName}", ex);
                }
            }
        }

        private void ControlProperty()
        {

        }
        async void SearchModFromCurseForge()
        {
            var keyword = ModNameTextBox.Text;
            _keyword = keyword;
            var gameVersion = ModVersionTextBox.Text;
            _gameVersion = gameVersion;
            ModLoaderType loaderType;

            if (mapping.TryGetValue((DaiYuLoaderType)LoaderTypeComboBox.SelectedIndex, out ModLoaderType modLoaderType))
            {
                loaderType = modLoaderType;
            }
            else
            {
                loaderType = ModLoaderType.Any;
            }
            _loaderType = loaderType;

            try
            {
                GenericListResponse<CurseForge.APIClient.Models.Mods.Mod> mods;
                if (loaderType == ModLoaderType.Any)
                {
                    mods = await cfApiClient.SearchModsAsync(gameId: _gameId, searchFilter: keyword, gameVersion: gameVersion, index: _page * 25, pageSize: 25);
                }
                else
                {
                    mods = await cfApiClient.SearchModsAsync(gameId: _gameId, searchFilter: keyword, modLoaderType: loaderType, gameVersion: gameVersion, index: _page * 25, pageSize: 25);
                }
                mods.Data.ForEach(mod =>
                {
                    var entry = new SearchModListViewItemEntry(mod);
                    entry.StringDownloadCount = Method.ConvertToWanOrYi(mod.DownloadCount);
                    DateTime localDateTime = mod.DateReleased.DateTime.ToLocalTime();
                    string formattedDateTime = localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    entry.StringDateTime = formattedDateTime;
                    entry.ModSource = ModSource.CurseForge;
                    ModListView.Items.Add(entry);
                });
                ModNameTextBox.IsEnabled = true;
                SearchBtn.IsEnabled = true;
                Loading.IsVisible = false;
                LoadMoreBtn.IsVisible = true;
                if (mods.Data.Count == 0)
                {
                    Method.Toast(MainLang.SearchNoResult);
                    LoadMoreBtn.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                ModNameTextBox.IsEnabled = true;
                SearchBtn.IsEnabled = true;
                Loading.IsVisible = false;
                LoadMoreBtn.IsVisible = false;
                Method.ShowShortException(MainLang.ErrorCallingApi, ex);
                return;
            }
        }
    }
}

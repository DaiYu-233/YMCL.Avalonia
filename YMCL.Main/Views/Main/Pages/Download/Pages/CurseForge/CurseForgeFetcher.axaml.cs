using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CurseForge.APIClient;
using CurseForge.APIClient.Models;
using CurseForge.APIClient.Models.Mods;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls;
using YMCL.Main.Public.Langs;
using File = CurseForge.APIClient.Models.Files.File;

namespace YMCL.Main.Views.Main.Pages.Download.Pages.CurseForgeFetcher;

public partial class CurseForgeFetcher : UserControl
{
    private readonly int _gameId = 432;
    private readonly ApiClient cfApiClient = new(Const.CurseForgeApiKey);

    private readonly Dictionary<DaiYuLoaderType, ModLoaderType> mapping = new()
    {
        { DaiYuLoaderType.Any, ModLoaderType.Any },
        { DaiYuLoaderType.Forge, ModLoaderType.Forge },
        { DaiYuLoaderType.NeoForge, ModLoaderType.NeoForge },
        { DaiYuLoaderType.Fabric, ModLoaderType.Fabric },
        { DaiYuLoaderType.Quilt, ModLoaderType.Quilt },
        { DaiYuLoaderType.LiteLoader, ModLoaderType.LiteLoader }
    };

    private bool _firstOpenModInfo = true;
    private string _gameVersion;
    private string _keyword;
    private ModLoaderType _loaderType;
    private int _page;

    public CurseForgeFetcher()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
        Loaded += (_, _) => { };
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
        ModNameTextBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                _page = 0;
                ModNameTextBox.IsEnabled = false;
                ModVersionTextBox.IsEnabled = false;
                SearchBtn.IsEnabled = false;
                Loading.IsVisible = true;
                LoadMoreBtn.IsVisible = false;
                ModListView.Items.Clear();
                SearchModFromCurseForge();
            }
        };
        ModVersionTextBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                _page = 0;
                ModNameTextBox.IsEnabled = false;
                ModVersionTextBox.IsEnabled = false;
                SearchBtn.IsEnabled = false;
                Loading.IsVisible = true;
                LoadMoreBtn.IsVisible = false;
                ModListView.Items.Clear();
                SearchModFromCurseForge();
            }
        };
        CloseModInfoBtn.Click += (_, _) =>
        {
            ModInfoRoot.Margin = new Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
            ModListView.IsVisible = true;
        };
        ModListView.SelectionChanged += async (_, e) =>
        {
            if (ModListView.SelectedIndex == -1) return;
            if (_firstOpenModInfo)
            {
                ModInfoRoot.Margin = new Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
                ModInfoRoot.IsVisible = true;
                _firstOpenModInfo = false;
                ModInfoRoot.Margin = new Thickness(10);
            }
            else
            {
                ModInfoRoot.Margin = new Thickness(10);
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
            _ = ModInfoIcon.LoadImgAsync();
            var modFiles = new List<File>();
            var index = 0;
            var shouldReturn = false;
            while (true)
            {
                GenericListResponse<File> files = new();
                try
                {
                    files = await cfApiClient.GetModFilesAsync(entry.Id, pageSize: 40, index: index * 40);
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowShortException(MainLang.ErrorCallingApi, ex);
                    shouldReturn = true;
                    break;
                }

                if (files.Data.Count == 0) break;

                files.Data.ForEach(file => { modFiles.Add(file); });
                index++;
            }

            ModFileLoading.IsVisible = false;
            if (shouldReturn) return;
            List<string> mcVersions = new();
            foreach (var file in modFiles)
                if (!mcVersions.Contains(file.SortableGameVersions[0].GameVersion!))
                    mcVersions.Add(file.SortableGameVersions[0].GameVersion!);

            mcVersions = mcVersions.Where(s => !string.IsNullOrEmpty(s)).ToList();
            mcVersions.Sort(new VersionComparer());
            mcVersions.Reverse();
            mcVersions.ForEach(mcVersion =>
            {
                var expander = new ModFileView(mcVersion);
                expander.ListView.SelectionChanged += ModFileSelectionChanged;
                expander.Name = $"mod_{mcVersion.Replace(".", "_")}";
                var classId = 0;
                if (ResultTypeComboBox.SelectedIndex == 0)
                    classId = 0;
                else if (ResultTypeComboBox.SelectedIndex == 1)
                    classId = 6;
                else if (ResultTypeComboBox.SelectedIndex == 2)
                    classId = 12;
                else if (ResultTypeComboBox.SelectedIndex == 3) classId = 17;

                foreach (var file in modFiles)
                    if (file.GameVersions[0] == mcVersion)
                    {
                        var item = new ModFileListViewItemEntry(file);
                        item.ClassId = classId;
                        item.StringDownloadCount = Method.Value.ConvertToWanOrYi(file.DownloadCount);
                        var localDateTime = file.FileDate.DateTime.ToLocalTime();
                        var formattedDateTime =
                            localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
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

                ModFileVersionPanel.Children.Add(expander);
                if (mcVersion == _gameVersion && !string.IsNullOrEmpty(_gameVersion))
                {
                    var matchedExpander = new ModFileView($"{MainLang.MatchingVersion} - {mcVersion}");
                    matchedExpander.Margin = new Thickness(0, 10, 0, 0);
                    matchedExpander.ListView.SelectionChanged += ModFileSelectionChanged;
                    foreach (var file in modFiles)
                        if (file.GameVersions[0] == mcVersion)
                        {
                            var item = new ModFileListViewItemEntry(file);
                            item.ClassId = classId;
                            item.StringDownloadCount = Method.Value.ConvertToWanOrYi(file.DownloadCount);
                            var localDateTime = file.FileDate.DateTime.ToLocalTime();
                            var formattedDateTime =
                                localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
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

                    topChildPanel.Children.Add(matchedExpander);
                }
            });
        };
        SearchBtn.Click += (_, _) =>
        {
            _page = 0;
            ModNameTextBox.IsEnabled = false;
            ModVersionTextBox.IsEnabled = false;
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
                var classId = 0;
                if (ResultTypeComboBox.SelectedIndex == 0)
                    classId = 0;
                else if (ResultTypeComboBox.SelectedIndex == 1)
                    classId = 6;
                else if (ResultTypeComboBox.SelectedIndex == 2)
                    classId = 12;
                else if (ResultTypeComboBox.SelectedIndex == 3) classId = 17;

                GenericListResponse<Mod> mods;
                if (_loaderType == ModLoaderType.Any)
                    mods = await cfApiClient.SearchModsAsync(_gameId, gameVersion: _gameVersion,
                        searchFilter: _keyword, index: _page * 25, pageSize: 25, categoryId: -1, classId: classId);
                else
                    mods = await cfApiClient.SearchModsAsync(_gameId, gameVersion: _gameVersion,
                        searchFilter: _keyword, modLoaderType: _loaderType, index: _page * 25, pageSize: 25,
                        categoryId: -1, classId: classId);

                mods.Data.ForEach(mod =>
                {
                    var entry = new SearchModListViewItemEntry(mod);
                    entry.StringDownloadCount = Method.Value.ConvertToWanOrYi(mod.DownloadCount);
                    var localDateTime = mod.DateReleased.DateTime.ToLocalTime();
                    var formattedDateTime =
                        localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    entry.StringDateTime = formattedDateTime;
                    entry.ModSource = ModSource.CurseForge;
                    ModListView.Items.Add(entry);
                });
                ModNameTextBox.IsEnabled = true;
                ModVersionTextBox.IsEnabled = true;
                SearchBtn.IsEnabled = true;
                Loading.IsVisible = false;
                LoadMoreBtn.IsVisible = true;
                if (mods.Data.Count == 0)
                {
                    Method.Ui.Toast(MainLang.SearchNoResult);
                    LoadMoreBtn.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                ModNameTextBox.IsEnabled = true;
                ModVersionTextBox.IsEnabled = true;
                SearchBtn.IsEnabled = true;
                Loading.IsVisible = false;
                LoadMoreBtn.IsVisible = false;
                Method.Ui.ShowShortException(MainLang.ErrorCallingApi, ex);
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
            var path = await Method.IO.SaveFilePicker(TopLevel.GetTopLevel(this)!,
                new FilePickerSaveOptions
                {
                    SuggestedFileName = item.DisplayName,
                    ShowOverwritePrompt = true,
                    Title = MainLang.SaveFile
                });
            if (path == null) return;
            var classId = item.ClassId;
            var extension = Path.GetExtension(path);
            if (classId == 6 && extension != ".jar") path += ".jar";

            if ((classId == 12 || classId == 17) && extension != ".zip") path += ".zip";

            Method.Ui.Toast($"{MainLang.BeginDownload}: {item.DisplayName}");
            var task = new TaskEntry($"{MainLang.Download} - {Path.GetFileName(path)}", true, false);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(
                               item.DownloadUrl.Replace("edge.forgecdn.net", "mediafilez.forgecdn.net"),
                               HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode(); // ȷ��HTTP�ɹ�״ֵ̬  

                        var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                                   FileShare.None, 4096, true))
                        {
                            var buffer = new byte[4096];
                            var totalBytesRead = 0L;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                var progressPercentage = (int)(totalBytesRead * 100 / totalBytes);
                                task.UpdateValueProgress(progressPercentage);
                            }

                            Method.Ui.Toast($"{MainLang.DownloadFinish}: {item.DisplayName}");
                            task.Destory();
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(item.DownloadUrl,
                                   HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode(); // ȷ��HTTP�ɹ�״ֵ̬  

                            var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                                       FileShare.None, 4096, true))
                            {
                                var buffer = new byte[4096];
                                var totalBytesRead = 0L;
                                int bytesRead;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;
                                    var progressPercentage = (int)(totalBytesRead * 100 / totalBytes);
                                    task.UpdateValueProgress(progressPercentage);
                                }

                                Method.Ui.Toast($"{MainLang.DownloadFinish}: {item.DisplayName}");
                                task.Destory();
                            }
                        }
                    }
                }
                catch
                {
                    Method.Ui.Toast($"{MainLang.DownloadFail}: {item.DisplayName}");
                    task.Destory();
                }
            }
        }
    }

    private void ControlProperty()
    {
    }

    public async void SearchModFromCurseForge()
    {
        var keyword = ModNameTextBox.Text;
        _keyword = keyword;
        var gameVersion = ModVersionTextBox.Text;
        _gameVersion = gameVersion;
        ModLoaderType loaderType;

        if (mapping.TryGetValue((DaiYuLoaderType)LoaderTypeComboBox.SelectedIndex, out var modLoaderType))
            loaderType = modLoaderType;
        else
            loaderType = ModLoaderType.Any;

        _loaderType = loaderType;

        try
        {
            var classId = 0;
            if (ResultTypeComboBox.SelectedIndex == 0)
                classId = 0;
            else if (ResultTypeComboBox.SelectedIndex == 1)
                classId = 6;
            else if (ResultTypeComboBox.SelectedIndex == 2)
                classId = 12;
            else if (ResultTypeComboBox.SelectedIndex == 3) classId = 17;

            GenericListResponse<Mod> mods;
            if (loaderType == ModLoaderType.Any)
                mods = await cfApiClient.SearchModsAsync(_gameId, gameVersion: gameVersion,
                    searchFilter: keyword, index: _page * 25, pageSize: 25, categoryId: -1, classId: classId);
            else
                mods = await cfApiClient.SearchModsAsync(_gameId, gameVersion: gameVersion,
                    searchFilter: keyword, modLoaderType: loaderType, index: _page * 25, pageSize: 25,
                    categoryId: -1, classId: classId);

            mods.Data.ForEach(mod =>
            {
                var entry = new SearchModListViewItemEntry(mod);
                entry.StringDownloadCount = Method.Value.ConvertToWanOrYi(mod.DownloadCount);
                var localDateTime = mod.DateReleased.DateTime.ToLocalTime();
                var formattedDateTime = localDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                entry.StringDateTime = formattedDateTime;
                entry.ModSource = ModSource.CurseForge;
                ModListView.Items.Add(entry);
            });
            ModNameTextBox.IsEnabled = true;
            ModVersionTextBox.IsEnabled = true;
            SearchBtn.IsEnabled = true;
            Loading.IsVisible = false;
            LoadMoreBtn.IsVisible = true;
            if (mods.Data.Count == 0)
            {
                Method.Ui.Toast(MainLang.SearchNoResult);
                LoadMoreBtn.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            ModNameTextBox.IsEnabled = true;
            ModVersionTextBox.IsEnabled = true;
            SearchBtn.IsEnabled = true;
            Loading.IsVisible = false;
            LoadMoreBtn.IsVisible = false;
            Method.Ui.ShowShortException(MainLang.ErrorCallingApi, ex);
        }
    }

    public class VersionComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var versionPartsX = x.Split('.');
            var versionPartsY = y.Split('.');

            var minLength = Math.Min(versionPartsX.Length, versionPartsY.Length);

            for (var i = 0; i < minLength; i++)
            {
                var partX = int.Parse(versionPartsX[i]);
                var partY = int.Parse(versionPartsY[i]);

                if (partX != partY) return partX.CompareTo(partY);
            }

            // ���������ͬλ�õİ汾�Ŷ���ͬ�������Ȳ�ͬ����ϳ��İ汾��Ӧ�ø���  
            return versionPartsX.Length.CompareTo(versionPartsY.Length);
        }
    }
}
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

namespace YMCL.Main.Views.Main.Pages.Download.Pages.Mod
{
    public partial class Mod : UserControl
    {
        readonly ApiClient cfApiClient = new(Const.CurseForgeApiKey);
        readonly int _gameId = 432;
        int _page = 0;
        string _keyword;
        ModLoaderType _loaderType;
        string _gameVersion;
        

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
            ModListView.SelectionChanged += (_, e) =>
            {
                if (ModListView.SelectedIndex == -1) return;
                var mod = ModListView.SelectedItem as SearchModListViewItemEntry;

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

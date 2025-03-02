using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CurseForge.APIClient;
using CurseForge.APIClient.Models.Mods;
using DynamicData;
using FluentAvalonia.Core;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Classes.Data.ResourceFetcher.CurseForgeModFileUiEntry;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;
using YMCL.Public.Module.Util;
using ModLoaderType = YMCL.Public.Enum.ModLoaderType;

namespace YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;

public partial class ModFileResult : UserControl, INotifyPropertyChanged
{
    private CurseForgeResourceEntry _entry;
    private ShortVersionEntry _selectedShortVersion;
    public ObservableCollection<ShortVersionEntry> Versions { get; set; } = [];

    public ShortVersionEntry SelectedShortVersion
    {
        get
        {
            ItemsControl.ItemsSource = null;
            ItemsControl.Items.Clear();
            ItemsControl.ItemsSource = _selectedShortVersion?.VersionEntries;
            return _selectedShortVersion;
        }
        set => SetField(ref _selectedShortVersion, value);
    }

    public ModFileResult(string id, (string name, string icon, string summary) tuple)
    {
        InitializeComponent();
        InfoBorder.DataContext = this;
        ItemsControl.DataContext = this;
        FileInfo.DataContext = new CurseForgeResourceEntry()
        {
            Name = tuple.name,
            Logo = new ModAsset() { Url = tuple.icon },
            Summary = tuple.summary,
        };
        Ring.IsVisible = true;
        _ = GetProject(id);
    }

    private async System.Threading.Tasks.Task GetProject(string id)
    {
        ApiClient apiClient = new(Public.Const.String.CurseForgeApiKey);
        var res = await apiClient.GetModAsync(Convert.ToInt32(id));
        Ring.IsVisible = false;
        var item = res.Data;
        var entry = new CurseForgeResourceEntry()
        {
            Id = item.Id,
            GameId = item.GameId,
            Name = item.Name,
            Slug = item.Slug,
            Summary = item.Summary,
            Status = item.Status,
            DownloadCount = item.DownloadCount,
            IsFeatured = item.IsFeatured,
            PrimaryCategoryId = item.PrimaryCategoryId,
            ClassId = item.ClassId,
            Authors = item.Authors,
            Logo = item.Logo,
            LatestFiles = item.LatestFiles,
            LatestFilesIndexes = item.LatestFilesIndexes,
            DateCreated = item.DateCreated,
            DateModified = item.DateModified,
            DateReleased = item.DateReleased,
            AllowModDistribution = item.AllowModDistribution,
            GamePopularityRank = item.GamePopularityRank,
            IsAvailable = item.IsAvailable,
            ThumbsUpCount = item.ThumbsUpCount,
            Rating = item.Rating
        };
        entry.Type = item.ClassId switch
        {
            6 => ResourceType.Mod,
            12 => ResourceType.ResourcePack,
            17 => ResourceType.Map,
            6552 => ResourceType.ShaderPack,
            6945 => ResourceType.DataPack,
            4471 => ResourceType.ModPack,
            _ => ResourceType.Unknown
        };
        _entry = entry;
        Init(entry);
    }

    public ModFileResult(CurseForgeResourceEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        Init(entry);
        InfoBorder.DataContext = this;
        ItemsControl.DataContext = this;
    }

    private void Init(CurseForgeResourceEntry entry)
    {
        var favourite = new FavouriteResource()
        {
            Id = entry.Id.ToString(),
            Source = ResourceSource.CurseForge,
            Icon = entry.Logo.Url,
            Summary = entry.Summary,
            Title = entry.Name,
            Type = entry.Type
        };
        Favourite.IsEnabled = true;
        if (!Data.FavouriteResources.Contains(favourite))
        {
            Icon.Data = PathGeometry.Parse(
                "F1 M 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.573567 0.633139 9.677734 0.571289 C 9.7819 0.509441 9.889322 0.478516 10 0.478516 C 10.110677 0.478516 10.218099 0.509441 10.322266 0.571289 C 10.426432 0.633139 10.504557 0.716146 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.076172 19.918619 8.225912 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.427083 19.498697 4.329427 19.521484 4.21875 19.521484 C 4.049479 19.521484 3.902995 19.459635 3.779297 19.335938 C 3.655599 19.21224 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 Z M 5.048828 17.753906 L 10 15.146484 L 14.951172 17.753906 C 14.794922 16.829428 14.640299 15.909831 14.487305 14.995117 C 14.33431 14.080404 14.173177 13.160808 14.003906 12.236328 L 18.017578 8.330078 L 12.480469 7.529297 C 12.057291 6.689453 11.642252 5.852865 11.235352 5.019531 C 10.82845 4.186199 10.416666 3.349609 10 2.509766 C 9.583333 3.349609 9.171549 4.186199 8.764648 5.019531 C 8.357747 5.852865 7.942708 6.689453 7.519531 7.529297 L 1.982422 8.330078 L 5.996094 12.236328 C 5.826823 13.160808 5.66569 14.080404 5.512695 14.995117 C 5.3597 15.909831 5.205078 16.829428 5.048828 17.753906 Z ");
            Text.Text = MainLang.Favourite;
        }
        else
        {
            Icon.Data = PathGeometry.Parse(
                "F1 M 4.21875 19.53125 C 4.049479 19.53125 3.902995 19.467773 3.779297 19.34082 C 3.655599 19.213867 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 L 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.571939 0.633139 9.672852 0.571289 C 9.773763 0.509441 9.879557 0.478516 9.990234 0.478516 C 10.107422 0.478516 10.218099 0.507812 10.322266 0.566406 C 10.426432 0.625 10.504557 0.709637 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.082683 19.918619 8.232422 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.420573 19.505209 4.322917 19.53125 4.21875 19.53125 Z ");
            Text.Text = MainLang.Favourted;
        }

        Favourite.Click += (_, _) =>
        {
            if (Data.FavouriteResources.Contains(favourite))
            {
                Data.FavouriteResources.Remove(favourite);
                Icon.Data = PathGeometry.Parse(
                    "F1 M 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.573567 0.633139 9.677734 0.571289 C 9.7819 0.509441 9.889322 0.478516 10 0.478516 C 10.110677 0.478516 10.218099 0.509441 10.322266 0.571289 C 10.426432 0.633139 10.504557 0.716146 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.076172 19.918619 8.225912 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.427083 19.498697 4.329427 19.521484 4.21875 19.521484 C 4.049479 19.521484 3.902995 19.459635 3.779297 19.335938 C 3.655599 19.21224 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 Z M 5.048828 17.753906 L 10 15.146484 L 14.951172 17.753906 C 14.794922 16.829428 14.640299 15.909831 14.487305 14.995117 C 14.33431 14.080404 14.173177 13.160808 14.003906 12.236328 L 18.017578 8.330078 L 12.480469 7.529297 C 12.057291 6.689453 11.642252 5.852865 11.235352 5.019531 C 10.82845 4.186199 10.416666 3.349609 10 2.509766 C 9.583333 3.349609 9.171549 4.186199 8.764648 5.019531 C 8.357747 5.852865 7.942708 6.689453 7.519531 7.529297 L 1.982422 8.330078 L 5.996094 12.236328 C 5.826823 13.160808 5.66569 14.080404 5.512695 14.995117 C 5.3597 15.909831 5.205078 16.829428 5.048828 17.753906 Z ");
                Text.Text = MainLang.Favourite;
            }
            else
            {
                Data.FavouriteResources.Add(favourite);
                Icon.Data = PathGeometry.Parse(
                    "F1 M 4.21875 19.53125 C 4.049479 19.53125 3.902995 19.467773 3.779297 19.34082 C 3.655599 19.213867 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 L 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.571939 0.633139 9.672852 0.571289 C 9.773763 0.509441 9.879557 0.478516 9.990234 0.478516 C 10.107422 0.478516 10.218099 0.507812 10.322266 0.566406 C 10.426432 0.625 10.504557 0.709637 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.082683 19.918619 8.232422 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.420573 19.505209 4.322917 19.53125 4.21875 19.53125 Z ");
                Text.Text = MainLang.Favourted;
            }
        };

        FileInfo.DataContext = entry;
        Loaded += (_, _) =>
        {
            _ = Animator.PageLoading.LevelTwoPage(this);
            if (!Data.FavouriteResources.Contains(favourite))
            {
                Icon.Data = PathGeometry.Parse(
                    "F1 M 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.573567 0.633139 9.677734 0.571289 C 9.7819 0.509441 9.889322 0.478516 10 0.478516 C 10.110677 0.478516 10.218099 0.509441 10.322266 0.571289 C 10.426432 0.633139 10.504557 0.716146 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.076172 19.918619 8.225912 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.427083 19.498697 4.329427 19.521484 4.21875 19.521484 C 4.049479 19.521484 3.902995 19.459635 3.779297 19.335938 C 3.655599 19.21224 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 Z M 5.048828 17.753906 L 10 15.146484 L 14.951172 17.753906 C 14.794922 16.829428 14.640299 15.909831 14.487305 14.995117 C 14.33431 14.080404 14.173177 13.160808 14.003906 12.236328 L 18.017578 8.330078 L 12.480469 7.529297 C 12.057291 6.689453 11.642252 5.852865 11.235352 5.019531 C 10.82845 4.186199 10.416666 3.349609 10 2.509766 C 9.583333 3.349609 9.171549 4.186199 8.764648 5.019531 C 8.357747 5.852865 7.942708 6.689453 7.519531 7.529297 L 1.982422 8.330078 L 5.996094 12.236328 C 5.826823 13.160808 5.66569 14.080404 5.512695 14.995117 C 5.3597 15.909831 5.205078 16.829428 5.048828 17.753906 Z ");
                Text.Text = MainLang.Favourite;
            }
            else
            {
                Icon.Data = PathGeometry.Parse(
                    "F1 M 4.21875 19.53125 C 4.049479 19.53125 3.902995 19.467773 3.779297 19.34082 C 3.655599 19.213867 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 L 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.571939 0.633139 9.672852 0.571289 C 9.773763 0.509441 9.879557 0.478516 9.990234 0.478516 C 10.107422 0.478516 10.218099 0.507812 10.322266 0.566406 C 10.426432 0.625 10.504557 0.709637 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.082683 19.918619 8.232422 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.420573 19.505209 4.322917 19.53125 4.21875 19.53125 Z ");
                Text.Text = MainLang.Favourted;
            }
        };
        Versions.Add(new ShortVersionEntry { Version = "All", DisplayVersion = MainLang.All });
        entry.LatestFilesIndexes.ForEach(x =>
        {
            var parts = x.GameVersion.Split('.');
            if (parts.Length < 2) return;
            var shortVersion = $"{parts[0]}.{parts[1]}";
            var loader = x.ModLoader != null ? (ModLoaderType)x.ModLoader : ModLoaderType.Any;
            var fileGroup = new VersionEntry
            {
                Version = x.GameVersion,
                Loader = loader.ToString(),
                Expander = new CurseForgeFileExpander(x.GameVersion, entry.Id,
                    entry.Type is ResourceType.Mod or ResourceType.ModPack
                        ? $"{x.GameVersion} {loader}"
                        : x.GameVersion,
                    x.ModLoader, entry.Type)
            };
            if (!Versions[0].VersionEntries.Any(z => z.Version == x.GameVersion && z.Loader == loader.ToString()))
            {
                Versions[0].VersionEntries.Add(fileGroup);
            }

            if (Versions.Any(y => y.Version == shortVersion)) return;
            var shortEntry = new ShortVersionEntry { Version = shortVersion, DisplayVersion = shortVersion };
            Versions.Add(shortEntry);
        });
        Versions.ToList().ForEach(x =>
        {
            entry.LatestFilesIndexes.ForEach(y =>
            {
                var parts = y.GameVersion.Split('.');
                if (parts.Length < 2) return;
                var shortVersion = $"{parts[0]}.{parts[1]}";
                var loader = y.ModLoader != null ? (ModLoaderType)y.ModLoader : ModLoaderType.Any;
                var fileGroup = new VersionEntry
                {
                    Version = y.GameVersion,
                    Loader = loader.ToString(),
                    Expander = new CurseForgeFileExpander(y.GameVersion, entry.Id,
                        entry.Type is ResourceType.Mod or ResourceType.ModPack
                            ? $"{y.GameVersion} {loader}"
                            : y.GameVersion,
                        y.ModLoader, entry.Type)
                };
                if (x.Version != shortVersion) return;
                if (x.VersionEntries.Any(z =>
                        z.Version == y.GameVersion && z.Loader == loader.ToString())) return;
                x.VersionEntries.Add(fileGroup);
            });
            var list = x.VersionEntries.OrderByDescending(v => new Version(v.Version))
                .ThenByDescending(v => v.Loader).ToList();
            x.VersionEntries = new ObservableCollection<VersionEntry>(list);
        });
        var shortList = Versions
            .OrderByDescending(s => s.Version == "All" ? 1 : 0)
            .ThenByDescending(s => Version.TryParse(s.Version, out var version) ? version : new Version(0, 0))
            .ToList();
        Versions.Clear();
        Versions.AddRange(shortList);
        SelectedShortVersion = Versions[0];
    }

    public ModFileResult()
    {
        InitializeComponent();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
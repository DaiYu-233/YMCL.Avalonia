using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurseForge.APIClient;
using CurseForge.APIClient.Models.Files;
using CurseForge.APIClient.Models.Mods;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using ModLoaderType = CurseForge.APIClient.Models.Mods.ModLoaderType;
using String = System.String;

namespace YMCL.Public.Module.IO.Network;

public class CurseForge
{
    private static readonly ApiClient apiClient = new(Const.String.CurseForgeApiKey);

    public static async Task<(List<IResourceFileEntry> data, bool success)> GetFiles(int id, int page = 1)
    {
        List<File> files;
        List<IResourceFileEntry> items = [];
        try
        {
            files = (await apiClient.GetModFilesAsync(id, pageSize: 100, index: (page - 1) * 100)).Data.ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e);
            return ([], false);
        }

        files.ForEach(x =>
        {
            items.Add(new CurseForgeResourceFileEntry()
            {
                Id = x.Id,
                GameId = x.GameId,
                ModId = x.ModId,
                IsAvailable = x.IsAvailable,
                DisplayName = x.DisplayName,
                FileName = x.FileName,
                ReleaseType = x.ReleaseType,
                FileStatus = x.FileStatus,
                Hashes = x.Hashes,
                FileDate = x.FileDate,
                FileLength = x.FileLength,
                FileSizeOnDisk = x.FileSizeOnDisk,
                Name = x.FileName,
                McVersions = x.SortableGameVersions.Where(y => !string.IsNullOrWhiteSpace(y.GameVersion))
                    .Select(y => y.GameVersion).ToList(),
                Dependency = x.Dependencies.Select(y => y.ModId).ToList(),
                Loader = x.SortableGameVersions
                    .FirstOrDefault(y => string.IsNullOrWhiteSpace(y.GameVersion) && y.GameVersionPadded == "0")
                    .GameVersionName,
                DownloadCount = x.DownloadCount,
                DownloadUrl = x.DownloadUrl
            });
        });
        return (items, true);
    }

    public static async Task<(List<CurseForgeResourceEntry> data, bool success)> Search(string? keyword,
        int dataType, int page = 1,
        string? mcVersion = "",
        Enum.ModLoaderType loaderType = Enum.ModLoaderType.Any)
    {
        var type = (ModLoaderType)loaderType;
        List<Mod> items;
        try
        {
            if (type == ModLoaderType.Any)
                items = (await apiClient.SearchModsAsync(432, gameVersion: mcVersion,
                    searchFilter: keyword ?? string.Empty, index: page * 25, pageSize: 25, categoryId: -1,
                    classId: dataType)).Data;
            else
                items = (await apiClient.SearchModsAsync(432, gameVersion: mcVersion,
                    searchFilter: keyword ?? string.Empty, modLoaderType: type, index: page * 25, pageSize: 25,
                    categoryId: -1, classId: dataType)).Data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ([], false);
        }

        var result = items.Select(item =>
            {
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
                return entry;
            })
            .ToList();
        return (result, true);
    }
}
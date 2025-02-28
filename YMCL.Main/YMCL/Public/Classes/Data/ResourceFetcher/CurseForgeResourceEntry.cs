using System.Collections.Generic;
using CurseForge.APIClient.Models;
using CurseForge.APIClient.Models.Files;
using CurseForge.APIClient.Models.Mods;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Classes.Data.ResourceFetcher;

public class CurseForgeResourceEntry : IResourceEntry
{
    string IResourceEntry.Summary => Summary;
    string IResourceEntry.Name => Name;
    ResourceSource IResourceEntry.Source => ResourceSource.CurseForge;
    DateTime IResourceEntry.LastUpdateTime => DateModified.DateTime;

    public string DisplayType => Type switch
    {
        ResourceType.Mod => MainLang.Mod,
        ResourceType.Map => MainLang.Map,
        ResourceType.DataPack => MainLang.DataPack,
        ResourceType.ResourcePack => MainLang.MaterialPack,
        ResourceType.ShaderPack => MainLang.ShaderPack,
        ResourceType.ModPack => MainLang.ModPack,
        _ => MainLang.Unknown,
    };

    public ResourceType Type { get; set; } = ResourceType.Unknown;
    ulong IResourceEntry.DownloadCount => Convert.ToUInt64(DownloadCount);
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public ModLinks Links { get; set; }
    public string Summary { get; set; }
    string IResourceEntry.Pic => Logo.Url;
    public ModStatus Status { get; set; }
    public double DownloadCount { get; set; }
    public bool IsFeatured { get; set; }
    public int PrimaryCategoryId { get; set; }
    public List<Category> Categories { get; set; } = [];
    public int? ClassId { get; set; }
    public List<ModAuthor> Authors { get; set; } = [];
    public ModAsset Logo { get; set; }
    public List<ModAsset> Screenshots { get; set; } = [];
    public int MainFileId { get; set; }
    public List<File> LatestFiles { get; set; } = [];
    public List<FileIndex> LatestFilesIndexes { get; set; } = [];
    public DateTimeOffset DateCreated { get; set; }
    public DateTimeOffset DateModified { get; set; }
    public DateTimeOffset DateReleased { get; set; }
    public bool? AllowModDistribution { get; set; }
    public int GamePopularityRank { get; set; }
    public bool IsAvailable { get; set; }
    public int ThumbsUpCount { get; set; }
    public double? Rating { get; set; }
}
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Modrinth.Extensions;
using Modrinth.Models;
using Modrinth.Models.Enums;
using Modrinth.Models.Enums.Project;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Util.Extension;

namespace YMCL.Public.Classes.Data.ResourceFetcher;

public class ModrinthResourceEntry: IResourceEntry
{
    public string Url => this.GetDirectUrl();
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[] Categories { get; set; }
    public Side? ClientSide { get; set; }
    public Side? ServerSide { get; set; }
    public ProjectType ProjectType { get; set; }
    public int Downloads { get; set; }
    public string? IconUrl { get; set; }
    public string ProjectId { get; set; }
    public string Author { get; set; }
    public string[] DisplayCategories { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public int Followers { get; set; }
    public string? LatestVersion { get; set; }
    public string License { get; set; }
    public string[] Versions { get; set; }
    public string[] Gallery { get; set; }
    public System.Drawing.Color? Color { get; set; }
    public string? FeaturedGallery { get; set; }
    public string[] Dependencies { get; set; }
    public string Name => Title;
    public string Summary => Description;
    public string Pic => IconUrl;
    public string DisplayType => ProjectType switch
    {
        ProjectType.Mod => MainLang.Mod,
        ProjectType.Datapack => MainLang.DataPack,
        ProjectType.Resourcepack => MainLang.MaterialPack,
        ProjectType.Shader => MainLang.ShaderPack,
        ProjectType.Modpack => MainLang.ModPack,
        ProjectType.Plugin => MainLang.Plugin,
        _ => MainLang.Unknown,
    };

    public ulong DownloadCount => Convert.ToUInt64(Downloads);
    public DateTime LastUpdateTime => DateModified;
    public ResourceSource Source => ResourceSource.Modrinth;
}
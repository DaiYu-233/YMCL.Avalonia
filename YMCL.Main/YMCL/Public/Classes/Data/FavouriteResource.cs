using Newtonsoft.Json;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Classes.Data;

public record FavouriteResource
{
    public ResourceSource Source { get; set; } = ResourceSource.Unknown;
    public ResourceType Type { get; set; } = ResourceType.Unknown;

    [JsonIgnore]
    public string DisplayType => Type switch
    {
        ResourceType.Mod => MainLang.Mod,
        ResourceType.DataPack => MainLang.DataPack,
        ResourceType.ResourcePack => MainLang.MaterialPack,
        ResourceType.ShaderPack => MainLang.ShaderPack,
        ResourceType.ModPack => MainLang.ModPack,
        ResourceType.Plugin => MainLang.Plugin,
        _ => MainLang.Unknown,
    };

    public string Id { get; set; }
    public string Summary { get; set; }
    public string Icon { get; set; }
    public string Title { get; set; }

    public virtual bool Equals(FavouriteResource? other)
    {
        return Source == other?.Source && Id == other?.Id;
    }
}
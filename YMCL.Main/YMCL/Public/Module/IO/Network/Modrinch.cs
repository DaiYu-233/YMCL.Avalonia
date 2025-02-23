using System.Collections.Generic;
using System.Threading.Tasks;
using Modrinth;
using Index = Modrinth.Models.Enums.Index;

namespace YMCL.Public.Module.IO.Network;

public class Modrinch
{
    private static readonly ModrinthClient apiClient = new();

    public static async Task SearchForAny(string? keyword,
        int page = 1, string? mcVersion = "",
        Enum.ModLoaderType loaderType = Enum.ModLoaderType.Any)
    {
        var res = await apiClient.Project.SearchAsync(keyword!, Index.Downloads, (page - 1) * 25, 25);
    }
}
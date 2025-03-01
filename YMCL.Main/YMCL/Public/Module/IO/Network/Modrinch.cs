using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Modrinth;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Newtonsoft.Json;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Classes.Json;
using YMCL.Public.Enum;
using Index = Modrinth.Models.Enums.Index;

namespace YMCL.Public.Module.IO.Network;

public class Modrinth
{
    private static readonly ModrinthClient apiClient = new();

    public static async Task<(List<ModrinthResourceEntry> data, bool success)> Search(string? keyword,
        int page = 1, string? mcVersion = "", int type = 0)
    {
        try
        {
            var list = new List<ModrinthResourceEntry>();
            var facets = new FacetCollection();
            if (!string.IsNullOrWhiteSpace(mcVersion))
                facets.Add(Facet.Version(mcVersion));
            if (type != 0)
                facets.Add(Facet.ProjectType((ProjectType)(type - 1)));
            var res = await apiClient.Project.SearchAsync(keyword!, Index.Downloads, (page - 1) * 25, 25, facets);

            res.Hits.ToList().ForEach(x =>
            {
                list.Add(new ModrinthResourceEntry()
                {
                    Slug = x.Slug,
                    ProjectId = x.ProjectId,
                    Author = x.Author,
                    Dependencies = x.Dependencies,
                    Categories = x.Categories,
                    ClientSide = x.ClientSide,
                    ServerSide = x.ServerSide,
                    Description = x.Description,
                    ProjectType = x.ProjectType,
                    Title = x.Title,
                    DateCreated = x.DateCreated,
                    DateModified = x.DateModified,
                    Followers = x.Followers,
                    LatestVersion = x.LatestVersion,
                    Downloads = x.Downloads,
                    IconUrl = x.IconUrl,
                    License = x.License,
                    Versions = x.Versions,
                    Gallery = x.Gallery,
                    Color = x.Color,
                    FeaturedGallery = x.FeaturedGallery,
                    DisplayCategories = x.DisplayCategories
                });
            });

            return (list, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ([], false);
        }
    }

    public static async Task<(List<ModrinthVersionEntry.Root>? data, bool success)> GetVersionsById(string id)
    {
        try
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync($"https://api.modrinth.com/v2/project/{id}/version");
            var obj = JsonConvert.DeserializeObject<List<ModrinthVersionEntry.Root>>(json);
            return (obj, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ([], false);
        }
    }
}
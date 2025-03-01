using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YMCL.Public.Classes.Json;

public class ModrinthVersionEntry
{
    public class Root
    {
        public List<string> game_versions { get; set; }
        [JsonPropertyName("loaders")] public List<string> Loaders { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("project_id")] public string ProjectId { get; set; }

        [JsonPropertyName("author_id")] public string AuthorId { get; set; }

        [JsonPropertyName("featured")] public bool Featured { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("version_number")] public string VersionNumber { get; set; }

        [JsonPropertyName("changelog")] public string Changelog { get; set; }

        [JsonPropertyName("changelog_url")] public string ChangelogUrl { get; set; }

        public DateTime date_published { get; set; }

        [JsonPropertyName("downloads")] public int Downloads { get; set; }

        [JsonPropertyName("version_type")] public string VersionType { get; set; }

        [JsonPropertyName("status")] public string Status { get; set; }

        [JsonPropertyName("requested_status")] public object RequestedStatus { get; set; }

        [JsonPropertyName("files")] public List<File> Files { get; set; }

        [JsonPropertyName("dependencies")] public List<Dependency> Dependencies { get; set; }
    }

    public class File
    {
        [JsonPropertyName("hashes")] public Dictionary<string, string> Hashes { get; set; }

        [JsonPropertyName("url")] public string Url { get; set; }

        [JsonPropertyName("filename")] public string Filename { get; set; }

        [JsonPropertyName("primary")] public bool Primary { get; set; }

        [JsonPropertyName("size")] public int Size { get; set; }

        [JsonPropertyName("file_type")] public object FileType { get; set; }
    }

    public class Dependency
    {
        [JsonPropertyName("version_id")] public object VersionId { get; set; }

        [JsonPropertyName("project_id")] public string ProjectId { get; set; }

        [JsonPropertyName("file_name")] public object FileName { get; set; }

        [JsonPropertyName("dependency_type")] public string DependencyType { get; set; }
    }
}
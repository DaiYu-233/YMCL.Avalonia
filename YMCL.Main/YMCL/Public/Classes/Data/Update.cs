namespace YMCL.Public.Classes.Data;

public record CheckUpdateInfo
{
    public bool Success { get; init; }
    public bool IsNeedUpdate { get; init; }
    public string NewVersion { get; init; }
    public string GithubUrl { get; init; }
}
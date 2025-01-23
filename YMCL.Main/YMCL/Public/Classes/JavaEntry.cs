namespace YMCL.Public.Classes;

public sealed record JavaEntry
{
    public bool Is64Bit { get; init; }
    public string JavaPath { get; set; }
    public string JavaVersion { get; init; }

    public int JavaSlugVersion { get; init; }

    public string JavaDirectoryPath { get; init; }

    public bool Equals(JavaEntry? other)
    {
        if (other == null) return false;
        if (other.JavaVersion == "Auto" && JavaVersion == "Auto") return true;
        if (other.JavaVersion == "Global" && JavaVersion == "Global") return true;
        return Is64Bit == other.Is64Bit && JavaVersion == other.JavaVersion && JavaPath == other.JavaPath &&
               JavaSlugVersion == other.JavaSlugVersion && JavaDirectoryPath == other.JavaDirectoryPath;
    }

    public static JavaEntry MlToYmcl(MinecraftLaunch.Classes.Models.Game.JavaEntry entry)
    {
        return new JavaEntry
        {
            Is64Bit = entry.Is64Bit,
            JavaDirectoryPath = entry.JavaDirectoryPath,
            JavaVersion = entry.JavaVersion,
            JavaPath = entry.JavaPath,
            JavaSlugVersion = entry.JavaSlugVersion
        };
    }
    
    public static MinecraftLaunch.Classes.Models.Game.JavaEntry YmclToMl(JavaEntry entry)
    {
        return new MinecraftLaunch.Classes.Models.Game.JavaEntry
        {
            Is64Bit = entry.Is64Bit,
            JavaDirectoryPath = entry.JavaDirectoryPath,
            JavaVersion = entry.JavaVersion,
            JavaPath = entry.JavaPath,
            JavaSlugVersion = entry.JavaSlugVersion
        };
    }
}
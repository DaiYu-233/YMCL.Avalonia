namespace YMCL.Public.Classes.Data;

public sealed record JavaEntry
{
    public bool Is64bit { get; init; }
    public string JavaPath { get; set; }
    public string JavaType { get; init; }
    public string JavaVersion { get; init; }
    public string JavaFolder { get; init; }
    public int MajorVersion { get; init; }

    public bool Equals(JavaEntry? other)
    {
        if (other == null) return false;
        if (other.JavaVersion == "Auto" && JavaVersion == "Auto") return true;
        if (other.JavaVersion == "Global" && JavaVersion == "Global") return true;
        return Is64bit == other.Is64bit && JavaPath == other.JavaPath && JavaType == other.JavaType;
    }

    public static JavaEntry MlToYmcl(MinecraftLaunch.Base.Models.Game.JavaEntry entry)
    {
        return new JavaEntry
        {
            Is64bit = entry.Is64bit,
            JavaVersion = entry.JavaVersion,
            JavaPath = entry.JavaPath,
            JavaType = entry.JavaType,
            JavaFolder = entry.JavaFolder,
            MajorVersion = entry.MajorVersion
        };
    }

    public static MinecraftLaunch.Base.Models.Game.JavaEntry YmclToMl(JavaEntry entry)
    {
        return new MinecraftLaunch.Base.Models.Game.JavaEntry
        {
            Is64bit = entry.Is64bit,
            JavaVersion = entry.JavaVersion,
            JavaPath = entry.JavaPath,
            JavaType = entry.JavaType,
            MajorVersion = entry.MajorVersion,
        };
    }
}
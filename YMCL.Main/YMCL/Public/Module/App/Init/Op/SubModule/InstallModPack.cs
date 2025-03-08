namespace YMCL.Public.Module.App.Init.Op.SubModule;

public class InstallModPack
{
    public static void Invoke(string text)
    {
        var url = text.Split(' ')[1];
        var extension = url.Contains("mrpack") ? ".mrpack" : ".zip";
        _ = Public.Module.Op.DownloadResource.ModPack(url, extension);
    }
}
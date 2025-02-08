using System.Linq;
using Avalonia.Media.Imaging;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Game;
using YMCL.Public.Classes;

namespace YMCL.Public.Module.Mc;

public class Icon
{
    public static Bitmap GetMinecraftIcon(MinecraftDataEntry entry)
    {
        if (entry.Type == "bedrock")
            return Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.dirt_path_side.png");
        if (entry.MinecraftEntry.IsVanilla)
        {
            return entry.MinecraftEntry.Version.Type switch
            {
                MinecraftVersionType.Release => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.grass_block_side.png"),
                MinecraftVersionType.Snapshot => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.crafting_table_front.png"),
                _ => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.grass_block_side.png")
            };
        }

        if (entry.MinecraftEntry is not ModifiedMinecraftEntry e)
            return Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.grass_block_side.png");
        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Forge))
        {
            return Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.furnace_front.png");
        }

        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Fabric))
        {
            return Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.FabricIcon.png");
        }

        if (e.ModLoaders.Any(a => a.Type == ModLoaderType.Quilt))
        {
            return Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.QuiltIcon.png");
        }

        return Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
            e.ModLoaders.Any(a => a.Type == ModLoaderType.OptiFine)
                ? "YMCL.Public.Assets.McIcons.OptiFineIcon.png"
                : "YMCL.Public.Assets.McIcons.grass_block_side.png");
    }
}
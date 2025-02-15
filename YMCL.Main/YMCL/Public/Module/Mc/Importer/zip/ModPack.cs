using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Installer.Modpack;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc.Installer.ModPack;

namespace YMCL.Public.Module.Mc.Importer.zip;

public class ModPack
{
    public static async Task Import(string path)
    {
        CurseforgeModpackInstallEntry? entry = null;
        try
        {
            entry = CurseforgeModpackInstaller.ParseModpackInstallEntry(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Notice($"{MainLang.Unrecognized}: {Path.GetFileName(path)}");
        }

        if (entry is null)
        {
            Notice($"{MainLang.Unrecognized}: {Path.GetFileName(path)}");
            return;
        }

        string? id = null;
        var result = await ShowDialog(entry.Id);
        
        if (result)
        {
            YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
            _ = CurseForge.Install(path, entry, id ?? entry.Id);
        }

        


        return;

        async Task<bool> ShowDialog(string text)
        {
            var textbox = new TextBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                Text = text, HorizontalAlignment = HorizontalAlignment.Stretch, Width = 500
            };
            var dialog = await ShowDialogAsync(MainLang.ImportModPack, p_content: textbox, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary)
                return false;
            var regex = new Regex(@"[\\/:*?""<>|]");
            var matches = regex.Matches(textbox.Text);
            if (matches.Count > 0)
            {
                var str = string.Empty;
                foreach (Match match in matches) str += match.Value;
                Notice($"{MainLang.IncludeSpecialWord}: {str}", NotificationType.Error);
                var dr = await ShowDialog(textbox.Text);
                if (!dr)
                    return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textbox.Text))
                {
                    Notice($"{MainLang.VersionIdCannotBeEmpty}", NotificationType.Error);
                    var dr = await ShowDialog(string.Empty);
                    if (!dr)
                        return false;
                }

                if (Directory.Exists(Path.Combine(Data.Setting.MinecraftFolder.Path, "versions", textbox.Text)))
                {
                    Notice($"{MainLang.FolderAlreadyExists}: {textbox.Text}",
                        NotificationType.Error);
                    var dr = await ShowDialog(textbox.Text);
                    if (!dr)
                        return false;
                }

                id = textbox.Text;
                return true;
            }

            return false;
        }
    }
}
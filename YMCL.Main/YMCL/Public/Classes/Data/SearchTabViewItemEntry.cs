using System.Linq;

namespace YMCL.Public.Classes.Data;

public record SearchTabViewItemEntry
{
    public UserControl Content { get; set; }
    public string Title { get; set; }
    public string Tag { get; set; }
    public string Host { get; set; }
    public bool CanClose { get; set; }

    public void Close()
    {
        if (Host == nameof(Views.Main.Pages.DownloadPages.CurseForge))
        {
            if (App.UiRoot.ViewModel.Download._curseForge.SelectedItem == this)
            {
                App.UiRoot.ViewModel.Download._curseForge.Items.Remove(this);

                App.UiRoot.ViewModel.Download._curseForge.SelectedItem =
                    App.UiRoot.ViewModel.Download._curseForge.Items.LastOrDefault();
            }
            else
            {
                App.UiRoot.ViewModel.Download._curseForge.Items.Remove(this);
            }
        }
        else if (Host == nameof(Views.Main.Pages.DownloadPages.Modrinth))
        {
            if (App.UiRoot.ViewModel.Download._modrinth.SelectedItem == this)
            {
                App.UiRoot.ViewModel.Download._modrinth.Items.Remove(this);
                App.UiRoot.ViewModel.Download._modrinth.SelectedItem =
                    App.UiRoot.ViewModel.Download._modrinth.Items.LastOrDefault();
            }
            else
            {
                App.UiRoot.ViewModel.Download._modrinth.Items.Remove(this);
            }
        }
        else
        {
            throw new NotImplementedException();
        }

        GC.Collect(2, GCCollectionMode.Aggressive, true);
    }
}
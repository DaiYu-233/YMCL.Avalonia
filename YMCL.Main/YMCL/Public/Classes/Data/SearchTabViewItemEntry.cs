namespace YMCL.Public.Classes.Data;

public record SearchTabViewItemEntry
{
    public UserControl Content { get; set; }
    public string Title { get; set; }
    public string Tag { get; set; }
    public bool CanClose { get; set; }

    public void Close()
    {
        if (App.UiRoot.ViewModel.Download._curseForge.SelectedItem == this)
        {
            App.UiRoot.ViewModel.Download._curseForge.SelectedItem =
                App.UiRoot.ViewModel.Download._curseForge.Items[^2];
        }

        App.UiRoot.ViewModel.Download._curseForge.Items.Remove(this);
        GC.Collect(2, GCCollectionMode.Aggressive, true);
    }
}
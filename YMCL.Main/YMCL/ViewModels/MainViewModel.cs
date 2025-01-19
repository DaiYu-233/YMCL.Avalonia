using CommunityToolkit.Mvvm.ComponentModel;
using YMCL.Views.Main.Pages;

namespace YMCL.ViewModels;

public class MainViewModel : NotifyPropertyModelBase
{
    private static readonly About _about = new();
    private static readonly Search _search = new();
    private static readonly Download _download = new();
    private static readonly More _more = new();
    private static readonly Music _music = new();
    private static readonly Setting _setting = new();
    private static readonly Task _task = new();
    private static readonly Launch _launch = new();
    private UserControl _currentPage = _launch;

    public UserControl CurrentPage
    {
        get => _currentPage;
        set => SetField(ref _currentPage, value);
    }
    
    public void TogglePage(string page)
    {
        CurrentPage = page switch
        {
            "about" => _about,
            "search" => _search,
            "download" => _download,
            "more" => _more,
            "music" => _music,
            "setting" => _setting,
            "task" => _task,
            "launch" => _launch,
            _ => _currentPage
        };
        _ = Public.Module.Ui.Animator.PageLoading.LevelOnePage(_currentPage);
    }
}
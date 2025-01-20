using CommunityToolkit.Mvvm.ComponentModel;
using YMCL.Views.Main.Pages;

namespace YMCL.ViewModels;

public class MainViewModel : NotifyPropertyModelBase
{
    public static readonly About _about = new();
    public static readonly Search _search = new();
    public static readonly Download _download = new();
    public static readonly More _more = new();
    public static readonly Music _music = new();
    public static readonly Setting _setting = new();
    public static readonly Task _task = new();
    public static readonly Launch _launch = new();
    public UserControl _currentPage = _launch;

    public UserControl CurrentPage
    {
        get => _currentPage;
        set => SetField(ref _currentPage, value);
    }

    public About About => _about;
    public Search Search => _search;
    public Download Download =>_download ;
    public More More =>_more ;
    public Music  Music =>_music ;
    public Setting  Setting =>_setting ;
    public Task  Task =>_task ;
    public Launch Launch =>_launch ;

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
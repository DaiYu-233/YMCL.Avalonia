using FluentAvalonia.UI.Controls;
using YMCL.Public.Module.Ui;

namespace YMCL.Public.Classes.Operate;

public class RegisteredPage
{
    public NavigationView Host { get; init; }
    public Control NavContent { get; init; }
    public string Tag { get; init; }
    public Control Page { get; init; }
    private NavigationViewItem _item;
    private EventHandler<NavigationViewSelectionChangedEventArgs> _handler;

    public void Show()
    {
        Host.MenuItems.Add(_item);
        Host.SelectionChanged += _handler;
    }

    public void Hide()
    {
        Host.MenuItems.Remove(_item);
        Host.SelectionChanged -= _handler;
    }

    public RegisteredPage Build()
    {
        _item = new NavigationViewItem()
        {
            Tag = Tag,
            Content = NavContent != null ? NavContent : Tag
        };
        _handler = (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            if (tag != Tag) return;
            App.UiRoot.Frame.Content = Page;
            _ = Animator.PageLoading.LevelTwoPage(Page);
        };
        return this;
    }
}
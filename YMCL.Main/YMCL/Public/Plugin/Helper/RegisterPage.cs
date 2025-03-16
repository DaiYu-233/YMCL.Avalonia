using FluentAvalonia.UI.Controls;
using YMCL.Public.Classes.Operate;

namespace YMCL.Public.Plugin;

public static class Helper
{
    public static RegisteredPage RegisterPage(this NavigationView nav, Control page, string tag, Control content = null)
    {
        return new RegisteredPage
        {
            Host = nav,
            Page = page,
            Tag = tag,
            NavContent = content
        }.Build();
    }
}
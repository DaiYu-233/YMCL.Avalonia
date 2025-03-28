using System.Reflection;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Plugin.Base;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Module.Ui;
using YMCL.Public.Plugin;
using YMCL.Views.Main;

namespace YMCL.Plugin.Page;

public class ExamplePluginWithDependence : IPlugin
{
    public string Name => "Example Plugin With Page";
    public string Author => "DaiYu";
    public string Description => "A example plugin with page for YMCL.";
    public string Version => "1.0.0";
    private RegisteredPage _registeredPage;
    private bool _registered;

    public int Execute(bool isEnable)
    {
        if (!_registered)
        {
            _registeredPage = App.UiRoot.NavigationView.RegisterPage(new ExamplePage(), "test",
                BuildNavItemContent.Build("Example",
                    //Icon
                    "F1 M 4.951172 20 C 4.625651 20 4.314778 19.933268 4.018555 19.799805 C 3.722331 19.666342 3.461914 19.487305 3.237305 19.262695 C 3.012695 19.038086 2.833659 18.77767 2.700195 18.481445 C 2.566732 18.185221 2.5 17.87435 2.5 17.548828 L 2.5 2.451172 C 2.5 2.125652 2.566732 1.814779 2.700195 1.518555 C 2.833659 1.222332 3.012695 0.961914 3.237305 0.737305 C 3.461914 0.512695 3.722331 0.33366 4.018555 0.200195 C 4.314778 0.066732 4.625651 0 4.951172 0 L 10 0 L 10 5.048828 C 10 5.387371 10.068359 5.704754 10.205078 6.000977 C 10.341797 6.297201 10.524088 6.55599 10.751953 6.777344 C 10.979817 6.998699 11.245117 7.17448 11.547852 7.304688 C 11.850586 7.434896 12.167969 7.5 12.5 7.5 L 17.5 7.5 L 17.5 17.548828 C 17.5 17.87435 17.433268 18.185221 17.299805 18.481445 C 17.16634 18.77767 16.987305 19.038086 16.762695 19.262695 C 16.538086 19.487305 16.277668 19.666342 15.981445 19.799805 C 15.685221 19.933268 15.374349 20 15.048828 20 Z M 12.5 6.25 C 12.324219 6.25 12.15983 6.217448 12.006836 6.152344 C 11.853841 6.08724 11.722005 5.99935 11.611328 5.888672 C 11.50065 5.777995 11.41276 5.646159 11.347656 5.493164 C 11.282552 5.34017 11.25 5.175782 11.25 5 L 11.25 0.361328 L 17.138672 6.25 Z "));
            _registered = true;
        }

        if (isEnable)
        {
            _registeredPage.Show();
        }
        else
        {
            _registeredPage.Hide();
        }

        return 0;
    }
}
## 1. Clone this repository

```bash
git clone https://github.com/DaiYu-233/YMCL.Avalonia.git
```

## 2. Build the main project

```bash
dotnet build YMCL.Main/YMCL.Main.csproj
```

## 3. Write plugins

Redirects to `YMCL Plugin\Plugin.cs` source file

```csharp
using Avalonia.Controls;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Plugin_Test  //Plugin namespace
{
    public class Plugin_DaiYu : IPlugin //"Plugin_DaiYu" is the plugin class name
    {
        public PluginInfo GetPluginInformation()
        {
            //PluginInfo
            return new PluginInfo()
            {
                Author="DaiYu",//plugin author
                Name="Test Plugin",//plugin name
                Version="1.3.0",//plugin version
                Description = "This A Plugin of YMCL.",//Plugin Description
                Time=new DATE (1970, 1, 1, 0, 0, 0)//Plugin release time
            };
        }

        public void Dispose()
        {
            //PluginBehavior
            //In this example, change the display text of the "Version List" button on the main interface to "Plugin Test". The specific method can be found by browsing the source code
            var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
            a.Content = "Plugin Test";
        }
    };
}
```

## 4.  Building plugins

````bash
dotnet build YMCL.Plugin/YMCL.Plugin.csproj
````

Output result: `YMCL.Plugin/bin/Debug/net8.0/YMCL.Plugin.dll`

Rename the plugin binary file to `plugin-namespace.plugin-class-name.dll` (mandatory)
The plugin compiled in the above code should be renamed as `YMCL.Plugin_Test.Plugin_DaiYu.dll`

The plugin has been developed and can be distributed to users for use

## 5. Install plugins

![img](https://pic.daiyu.fun/pic/2024/202407220914001.png)

As shown in the picture, click this button to open the data folder, then find the 'Plugin' folder, put the plugin into this folder, restart YMCL to complete the installation
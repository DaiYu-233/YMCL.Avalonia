## 1. Clone this repository

```bash
git clone https://github.com/DaiYu-233/YMCL.Avalonia.git
```

## 2. Build the main project

```bash
cd YMCL.Avalonia
```

```bash
dotnet build YMCL.Main/YMCL.Main.csproj
```

## 3. Write plugins

Redirects to `YMCL.Plugin\Plugin.cs` source file

```csharp
using Avalonia.Controls;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Plugin  //Do not modify
{
    public class Main : IPlugin //Do not modify
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
                Time= new DateTime(1970, 1, 1, 0, 0, 0)//Plugin release time, in the format of year month day hour minute second
            };
        }

        public void OnLoad()
        {
            //PluginBehavior
            //Triggered when the program is opened
            //In this example, change the display text of the "Version List" button on the main interface to "Plugin Test". The specific method can be found by browsing the source code
            var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
            a.Content = "Plugin Test";
        }
        
        public void OnDisable()
        {
            //When the plugin switch is turned off
            //In this example, a message box pops up to prompt the user
            Method.Ui.Toast("Plugin Off");
        }

        public void OnEnable()
        {
            //When the plugin switch is turned on
            //In this example, a message box pops up to prompt the user
            Method.Ui.Toast("Plugin On");
        }
        
        public void OnLaunch()
        {
            //When the game launched
            //In this example, a message box pops up to prompt the user
            Dispatcher.UIThread.Invoke(() =>
            {
                Method.Ui.Toast("Game Launched");
            });
        }
    };
}
```

## 4. Rename Project

Rename `YMCL.Plugin/YMCL.Plugin.csproj` to `YMCL.Plugin/PluginAuthorName.PluginName.csproj`

Rename `YMCL.Plugin/YMCL.Plugin.csproj.user` to `YMCL.Plugin/PluginAuthorName.PluginName.csproj.user`

## 5. 构建插件

````bash
cd YMCL.Plugin
````

````bash
dotnet build
````

Result File: `YMCL.Plugin/bin/Debug/net8.0/PluginAuthorName.PluginName.dll`

The plugin has been developed and can be distributed to users for use

## 6. Install plugins

![img](https://pic.daiyu.fun/pic/2024/202407220914001.png)

As shown in the picture, click this button to open the data folder, then find the 'Plugin' folder, put the plugin into
this folder, restart YMCL to complete the installation
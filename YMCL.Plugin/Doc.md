## 1. 克隆本仓库

```bash
git clone https://github.com/DaiYu-233/YMCL.Avalonia.git
```

## 2. 构建主项目

```bash
cd YMCL.Avalonia
```

```bash
dotnet build YMCL.Main/YMCL.Main.csproj
```

## 3. 编写插件

重定向至 `YMCL.Plugin\Plugin.cs`  源文件

```csharp
using Avalonia.Controls;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Pulgin  //不要修改
{
    public class Main : IPlugin //不要修改
    {
        public PluginInfo GetPluginInformation()
        {
            //PluginInfo
            //插件信息
            return new PluginInfo()
            {
                Author = "DaiYu", //插件作者
                Name = "Test-Plugin",  //插件名称
                Version = "1.3.0",  //插件版本
                Description = "This A Plugin of YMCL.",  //插件描述
                Time = new DateTime(1970, 1, 1, 0, 0, 0)  //插件发布时间, 格式为年月日时分秒
            };
        }

        public void OnLoad()
        {
            //PluginBehavior
            //插件行为
            //在程序打开时触发
            //在这个例子中把主界面的"版本列表按钮"的显示文本修改为"Plugin Test",具体方法可浏览源代码
            var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
            a.Content = "Plugin Test";
        }

        public void OnDisable()
        {
            //当插件开关被关闭时
            //在这个例子中弹出了一个消息框来提示用户
            Method.Ui.Toast("Plugin Off");
        }

        public void OnEnable()
        {
            //当插件开关被打开时
            //在这个例子中弹出了一个消息框来提示用户
            Method.Ui.Toast("Plugin On");
        }

        public void OnLaunch()
        {
            //当游戏启动时
            //在这个例子中弹出了一个消息框来提示用户
            Method.Ui.Toast("Game Launched");
        }
    };
}
```

## 4. 重命名项目

重命名 `YMCL.Plugin/YMCL.Plugin.csproj` 为 `YMCL.Plugin/插件作者名称.插件名称.csproj`

重命名 `YMCL.Plugin/YMCL.Plugin.csproj.user` 为 `YMCL.Plugin/插件作者名称.插件名称.csproj.user`

## 5. 构建插件

````bash
cd YMCL.Plugin
````

````bash
dotnet build
````

输出结果: `YMCL.Plugin/bin/Debug/net8.0/插件作者名称.插件名称.dll`

到此插件就开发完成,可以分发给用户使用

## 5. 安装插件

![img](https://pic.daiyu.fun/pic/2024/202407220914001.png)

如图, 点击此按钮打开数据文件夹, 然后找到 `Plugin` 文件夹, 将插件放入此文件夹, 重启YMCL就完成安装了

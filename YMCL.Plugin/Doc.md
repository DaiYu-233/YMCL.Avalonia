## 1. 克隆本仓库

```bash
git clone https://github.com/DaiYu-233/YMCL.Avalonia.git
```

## 2. 构建主项目

```bash
dotnet build YMCL.Main/YMCL.Main.csproj
```

## 3. 编写插件

转至 `YMCL.Plugin\Plugin.cs`  源文件

修改插件信息

```csharp
public PluginInfo GetPluginInformation()
{
    //PluginInfo
    return new PluginInfo()
    {
        Author = "DaiYu",  //插件作者
        Name = "Test Plugin",  //插件名称
        Version = "1.3.0",  //插件版本
        Description = "Plugin Description",  //插件描述
        Time = new DateTime(1970, 1, 1, 0, 0, 0)  //插件的发布时间
    };
}
```

编写插件行为

```csharp
public void Dispose()
{
    //PluginBehavior
    //在这个例子中把主界面的"版本列表按钮"的显示文本修改为"Plugin Test",具体方法可浏览源代码
    var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
    a.Content = "Plugin Test";
}
```

## 4. 构建插件

````bash
dotnet build YMCL.Plugin/YMCL.Plugin.csproj
````

输出结果: `YMCL.Plugin/bin/Debug/net8.0/YMCL.Plugin.dll`

到此插件就开发完成,可以分发给用户使用

## 5. 安装插件

![img](https://pic.daiyu.fun/pic/2024/202407220914001.png)

如图, 点击此按钮打开数据文件夹, 然后找到 `Plugin` 文件夹, 将插件放入此文件夹, 重启YMCL就完成安装了
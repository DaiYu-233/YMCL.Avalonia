<p align="center">
<img height="400" width="400" src="https://ymcl.daiyu.fun/Assets/img/YMCL-Icon.svg"/>
</p>

<div align="center">

# ⛏️ Yu Minecraft Launcher 🐳

简体中文 | [English](https://github.com/DaiYu-233/YMCL.Avalonia/blob/main/README_en.md)

免费、跨平台、完全开源的 Minecraft 启动器

![Downloads](https://img.shields.io/github/downloads/DaiYu-233/YMCL.Avalonia/total?logo=github&label=%E4%B8%8B%E8%BD%BD%E9%87%8F&style=for-the-badge&color=44cc11)
![Star](https://img.shields.io/github/stars/DaiYu-233/YMCL.Avalonia?logo=github&label=Star&style=for-the-badge)
![License](https://img.shields.io/github/license/DaiYu-233/YMCL.Avalonia?logo=github&label=开源协议&style=for-the-badge&color=ff7a35)

</div>

## 介绍

Yu Minecraft Launcher 是一个跨平台 Minecraft 启动器，支持 Windows，MacOS，Linux 等操作系统，兼容 x64、Arm 架构。

Ymcl 支持 Mod 管理, 游戏自定义, 游戏自动安装 (Forge,Fabric, Quilt, OptiFine), 界面自定义等功能。<del>甚至可以激活 Windows !</del>

Ymcl 使用 **Apache License 2.0** 开放源代码，此协议要求**在修改和分发软件时保留原始的版权声明、许可证声明和免责声明**；允许用户自由地使用、修改、复制和分发Apache许可的软件，无论是用于商业用途还是非商业用途；允许用户以不同的许可证发布修改后的产品或衍生品，但未做修改的那部分必须保留Apache许可证；Apache许可证没有提供任何形式的保证，用户对使用软件所造成的任何损害负有全部责任。

## 平台支持情况

|       | Windows | Linux | Mac OS |
| ----- | :------ | :---- | :----- |
| x64   | ✅️     | ✅️   | ✅️❔  |
| x86   | ✅️     | ❌    | ❌     |
| ARM64 | ❌      | ✅️❔ | ✅ ❔  |
| ARM32 | ❌      | ✅️❔ | ❌     |

- ✅: 支持的平台

- ❔: 未测试的平台

- ❌: 不支持的平台

## 如何安装

### 1. 下载 

- [Release](https://github.com/DaiYu-233/YMCL.Avalonia/releases)

### 2. 安装运行时

Ymcl 需要一份 .Net Core 8.0 运行时就可以运行在各种操作系统。

<details>   
<summary>Windows</summary> 

```DiaYu
x64：
	https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-aspnetcore-8.0.6-windows-x64-installer

x86：
	https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-aspnetcore-8.0.6-windows-x86-installer

WinGet：
	winget install Microsoft.DotNet.SDK.8
```

</details>


<details>   
<summary>Linux</summary> 
文档：https://learn.microsoft.com/zh-cn/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website

Debian：
```DaiYu
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

</details>

## 开源项目使用

**[Costura.Fody](https://github.com/Fody/Costura)**  **[MinecraftLaunch](https://github.com/Blessing-Studio/MinecraftLaunch)**  **[Newtonsoft.Json](https://www.newtonsoft.com/json)** 
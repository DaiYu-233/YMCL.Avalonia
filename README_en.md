<p align="center">
<img height="400" width="400" src="https://ymcl.daiyu.fun/Assets/img/YMCL-Icon.svg"/>
</p>

<div align="center">

# ⛏️ Yu Minecraft Launcher 🐳

[简体中文](https://github.com/DaiYu-233/YMCL.Avalonia/blob/main/README.md) | English

Free, Cross-Platform, Fully-Open-Source Minecraft Launcher

![Downloads](https://img.shields.io/github/downloads/DaiYu-233/YMCL.Avalonia/total?logo=github&label=Download&style=for-the-badge&color=44cc11)
![Star](https://img.shields.io/github/stars/DaiYu-233/YMCL.Avalonia?logo=github&label=Star&style=for-the-badge)
![License](https://img.shields.io/github/license/DaiYu-233/YMCL.Avalonia?logo=github&label=LICENSE&style=for-the-badge&color=ff7a35)

</div>

## Introduction  
  
Yu Minecraft Launcher is a cross-platform Minecraft launcher that supports operating systems such as Windows, MacOS, and Linux, with compatibility for x64 and Arm architectures.  
  
Yu Minecraft Launcher offers features like mod management, game customization, automatic game installation (Forge, Fabric, Quilt, OptiFine), and custom interface capabilities. <del>It can even activate Windows !</del>

Ymcl uses **Apache License 2.0** open source code, which requires **to retain the original copyright statement, license statement, and disclaimer when modifying and distributing software**; Allow users to freely use, modify, copy, and distribute Apache licensed software, whether for commercial or non-commercial purposes; Allow users to publish modified products or derivatives under different licenses, but the unmodified portion must retain the Apache license; The Apache license does not provide any form of guarantee, and users are fully responsible for any damage caused by the use of the software.
  
## Plugin repository

 →  https://github.com/DaiYu-233/YMCL.Plugin

## Platform Support  
  
|       | Windows | Linux | Mac OS |  
| ----- | :-----: | :---: | :----: |  
| x64   |   ✅    |   ✅   |   ✅❔   |  
| x86   |   ✅    |   ❌   |   ❌    |  
| ARM64 |   ❌    |   ✅❔   |   ✅❔   |  
| ARM32 |   ❌    |   ✅❔   |   ❌    |  
  
* ✅: Supported platform  
* ❔: Untested platform  
* ❌: Unsupported platform

## How to Install  
  
### 1. Download  
  
- [Release](https://github.com/DaiYu-233/YMCL.Avalonia/releases)  
  
### 2. Install Runtime  
  
Ymcl requires a .NET Core 8.0 runtime to run on various operating systems.  
  
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
Documentation：https://learn.microsoft.com/zh-cn/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website

Debian：
```DaiYu
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

</details>

## Plugin Development Document

简体中文 → https://github.com/DaiYu-233/YMCL.Avalonia/tree/main/YMCL.Plugin/Doc.md

English → https://github.com/DaiYu-233/YMCL.Avalonia/tree/main/YMCL.Plugin/Doc_en.md

## Open-Source Project Usage

**[Costura.Fody](https://github.com/Fody/Costura)**  **[MinecraftLaunch](https://github.com/Blessing-Studio/MinecraftLaunch)**  **[Newtonsoft.Json](https://www.newtonsoft.com/json)** 
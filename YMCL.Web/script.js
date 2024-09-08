/*获取到Url里面的参数*/
(function ($) {
    $.getUrlParam = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) return unescape(r[2]);
        return null;
    };
})(jQuery);
var f = $.getUrlParam("framework");
var w = $.getUrlParam("way");
console.log(f);
console.log(w);
var orUrl = null;
// 使用fetch函数发起GET请求
fetch("https://api.github.com/repos/DaiYu-233/YMCL.Avalonia/releases?per_page=1")
    .then((response) => {
        // 首先检查响应是否成功
        if (!response.ok) {
            alert("加载失败！\n Network response was not ok " + response.statusText);
            console.error("加载失败！\n Network response was not ok " + response.statusText);
            window.close();
        }
        // 解析JSON数据
        return response.json();
    })
    .then((data) => {
        // 处理获取到的数据
        data[0].assets.forEach((element) => {
            console.log(element.name);
            if ((element.name == "YMCL.Main.linux.arm.AppImage" && f == "linux-arm") ||
                (element.name == "YMCL.Main.linux.arm64.AppImage" && f == "linux-arm64") ||
                (element.name == "YMCL.Main.linux.x64.AppImage" && f == "linux-x64") ||
                (element.name == "YMCL.Main.osx.mac.x64.app.zip" && f == "osx-x64") ||
                (element.name == "YMCL.Main.osx.mac.arm64.app.zip" && f == "osx-arm64") ||
                (element.name == "YMCL.Main.win.x64.installer.exe" && f == "win-x64") ||
                (element.name == "YMCL.Main.win.arm64.installer.exe" && f == "win-arm64") ||
                (element.name == "YMCL.Main.win.x86.installer.exe" && f == "win-x86") ||
                (element.name == "YMCL.Main.win7.x64.exe.zip" && f == "win7-x64") ||
                (element.name == "YMCL.Main.win7.arm64.exe.zip" && f == "win7-arm64") ||
                (element.name == "YMCL.Main.win7.x86.exe.zip" && f == "win7-x86")) {
                orUrl = element.browser_download_url;
                console.log(element.name);
                console.log(orUrl);
            }
        });
        if (orUrl == null) {
            console.error("加载失败！");
            alert("加载失败！");
            window.close();
            return;
        }
        var taUrl = null;
        if (w == 0) {
            taUrl = orUrl;
        } else if (w == 1) {
            taUrl = "https://github.moeyy.xyz/" + orUrl;
        } else if (w == 2) {
            taUrl = "https://ghproxy.net/" + orUrl;
        } else if (w == 3) {
            taUrl = "https://mirror.ghproxy.com/" + orUrl;
        }
        if (taUrl == null) {
            console.error("加载失败！");
            alert("加载失败！");
            window.close();
            return;
        }
        window.location = taUrl; // 在新窗口中打开下载链接
        document.getElementById('ma').innerHTML = `已开始下载，若未开始下载请手动跳转 → <a href="${taUrl}" target="_blank" rel="noopener noreferrer"
      style="text-decoration: none;color: skyblue;">${taUrl}</a>`
    })
    .catch((error) => {
        // 处理请求过程中发生的任何错误
        console.error("加载失败！ \nThere has been a problem with your fetch operation:", error);
        alert("加载失败！ \nThere has been a problem with your fetch operation:", error);
        window.close();
    });

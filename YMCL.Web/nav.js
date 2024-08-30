window.addEventListener(
  "scroll",
  function (e) {
    const navbar = document.getElementById("navbar");
    console.log(window.scrollY);

    if (window.scrollY >= 50) {
      // 当滚动超过50像素时
      navbar.style.position = "fixed"; // 变为固定位置
      document.querySelector("#navbar > div > nav > div").style.backgroundColor = "rgba(255, 255, 255, 1)"; // 设置不透明度
      document.querySelector("#navbar > div > nav > div").style.padding = "15px 25px 15px 25px";
      document.querySelector("#navbar > div > nav > div").style.margin = "-15px 0 0 0";
      document.querySelector("#navbar > div > nav > div").style.boxShadow = "0 0 20px 11px rgba(56, 66, 77, 0.1)";
    } else {
      navbar.style.position = "unset";
      document.querySelector("#navbar > div > nav > div").style.backgroundColor = "rgba(0, 0, 0, 0)";
      document.querySelector("#navbar > div > nav > div").style.padding = "unset";
      document.querySelector("#navbar > div > nav > div").style.margin = "unset";
      document.querySelector("#navbar > div > nav > div").style.boxShadow = "unset";
    }
  },
  { passive: true }
);

var menu = 0;
function ToggleMobileMenu() {
  if (menu % 2 == 0) {
    document.getElementById('menu').style.top = "-40px";
    // document.getElementById("menu-btn").innerHTML = "关闭菜单";
  } else {
    document.getElementById('menu').style.top = "-350px";
    // document.getElementById("menu-btn").innerHTML = "打开菜单";
  }
  menu++;
}

function loadPage(page) {
  menu = 0;
  if (page == "download") {
    window.location = "/download";
  } else if (page == "home") {
    window.location = "/";
  }
}

var homeData = null;
var downloadData = null;
fetch("/")
  .then((response) => response.text())
  .then((data) => {
    homeData = data;
  })
  .catch((error) => console.error("Error:", error));
fetch("/download")
  .then((response) => response.text())
  .then((data) => {
    downloadData = data;
  })
  .catch((error) => console.error("Error:", error));

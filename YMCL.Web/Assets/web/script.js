function TrunSideNav() {
  if (num % 2 === 0) {
    document.getElementById("SideNav").style.width = "100vw";
  } else {
    document.getElementById("SideNav").style.width = "0";
  }
  num++;
}
function Load() {
  console.log("Html   By 呆鱼");
  console.log("JavaScript   By 呆鱼");
  LA.init({ id: "KIJVLwHWaiNAjIGG", ck: "KIJVLwHWaiNAjIGG" });
  new LingQue.Monitor().init({ id: "JpvA7imL0NrZzPiQ" });
  (function (c, l, a, r, i, t, y) {
    c[a] =
      c[a] ||
      function () {
        (c[a].q = c[a].q || []).push(arguments);
      };
    t = l.createElement(r);
    t.async = 1;
    t.src = "https://www.clarity.ms/tag/" + i;
    y = l.getElementsByTagName(r)[0];
    y.parentNode.insertBefore(t, y);
  })(window, document, "clarity", "script", "kwkrckpcin");
}
var num = 0;
Load();
window.addEventListener("scroll", function () {
  var pos = window.scrollY;
  document.querySelector("#PlayArea").style.height = height;
  if (pos >= document.querySelector("body > main").clientHeight) {
    document.querySelector("#FunctionContainer").style.marginLeft = (pos - document.querySelector("body > main").clientHeight) * -1 + "px";
  }
});
function Launch() {
  var value = document.querySelector("#Args").value;
  window.open("ymcl://" + value);
}

using UIKit;
using YMCL.Public.Const;
using YMCL.Public.Enum;

namespace YMCL.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        Data.Instance.RunnerType = RunnerType.Android;
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}

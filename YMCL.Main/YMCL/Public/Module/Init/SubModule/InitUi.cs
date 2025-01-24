using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;

namespace YMCL.Public.Module.Init.SubModule;

public class InitUi
{
    public static async Task Dispatch()
    {
        Ui.Setter.SetAccentColor(Data.Setting.AccentColor);
        Application.Current.Resources["MainOpacity"] = 1.0;
        DisplaceDefaultUi();
        Ui.Setter.SetBackGround();
        _ = SetCustomHomePage();
        Application.Current.ActualThemeVariantChanged += (_, _) => { UpdateTheme(); };
        if (Data.Setting.EnableAutoCheckUpdate)
        {
            var result = await IO.Network.Update.CheckUpdateAsync();
            if (result is { Success: true, IsNeedUpdate: true })
            {
                _ = ShowAutoUpdateDialog(result);
            }
        }
    }

    public static void UpdateTheme()
    {
        try
        {
            if (YMCL.App.UiRoot != null)
                (Ui.Getter.FindControlByName(YMCL.App.UiRoot, "PART_PaneRoot") as Panel).Background =
                    Application.Current.ActualThemeVariant == ThemeVariant.Dark
                        ? SolidColorBrush.Parse("#2c2c2c")
                        : SolidColorBrush.Parse("#FFE9F6FF");
            var visuals = new Queue<Visual>();
            if (YMCL.App.UiRoot != null) visuals.Enqueue(YMCL.App.UiRoot);

            while (visuals.Count > 0)
            {
                var current = visuals.Dequeue();

                if (current is not Control control) continue;
                foreach (var child in control.GetVisualChildren())
                {
                    visuals.Enqueue(child);
                }

                if (control is Image { Source: DrawingImage } imageControl)
                {
                    imageControl.InvalidateVisual();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        try
        {
            if (YMCL.App.UiRoot == null) return;
            (Getter.FindControlByName(YMCL.App.UiRoot, "PART_PaneRoot") as Panel).Background =
                Application.Current.ActualThemeVariant == ThemeVariant.Dark
                    ? SolidColorBrush.Parse("#2c2c2c")
                    : SolidColorBrush.Parse("#FFE9F6FF");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }


    public static void DisplaceDefaultUi()
    {
        if (YMCL.App.UiRoot is null) return;
        try
        {
            (Getter.FindControlByName(YMCL.App.UiRoot, "ContentGridBorder") as Border).Background = null;
            (Getter.FindControlByName(YMCL.App.UiRoot, "ContentGridBorder") as Border).BorderThickness =
                new Thickness(0);
            (Getter.FindControlByName(YMCL.App.UiRoot, "PART_PaneRoot") as Panel).Background =
                Application.Current.ActualThemeVariant == ThemeVariant.Dark
                    ? SolidColorBrush.Parse("#2c2c2c")
                    : SolidColorBrush.Parse("#FFE9F6FF");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        FAUISettings.SetAnimationsEnabledAtAppLevel(false); //关闭 FA 动画
    }

    public static async Task SetCustomHomePage()
    {
        if (YMCL.App.UiRoot is null) return;
        if (Data.Setting.CustomHomePage == Setting.CustomHomePageWay.None)
        {
            YMCL.App.UiRoot.ViewModel.Launch.CustomPageRoot.Child = null;
        }

        if (Data.Setting.CustomHomePage == Setting.CustomHomePageWay.Local)
        {
            try
            {
                var c = (Control)AvaloniaRuntimeXamlLoader.Load(
                    await File.ReadAllTextAsync(ConfigPath.CustomHomePageXamlDataPath));
                YMCL.App.UiRoot.ViewModel.Launch.CustomPageRoot.Child = c;
            }
            catch (Exception ex)
            {
                ShowLongException(MainLang.CustomHomePageSourceCodeError, ex);
            }
        }

        if (Data.Setting.CustomHomePage == Setting.CustomHomePageWay.Network)
        {
            try
            {
                using var client = new HttpClient();
                var code = await client.GetStringAsync(Data.Setting.CustomUpdateUrl);
                if (code is null) throw new ArgumentNullException("CustomHomePageSourceCode");
                var c = (Control)AvaloniaRuntimeXamlLoader.Load(code);
                YMCL.App.UiRoot.ViewModel.Launch.CustomPageRoot.Child = c;
            }
            catch (Exception ex)
            {
                ShowLongException(MainLang.CustomHomePageSourceCodeError, ex);
            }
        }
    }
}
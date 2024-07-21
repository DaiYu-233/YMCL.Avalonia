using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = new Views.Initialize.InitializeWindow();
                Const.Window.initialize = window;
                desktop.MainWindow = window;
                window.Hide();

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Method.Ui.ShowLongException(MainLang.UnhandledException, e.Exception);
            }
            finally
            {
                e.Handled = true;
            }
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var textBox = new TextBox() { FontFamily = (FontFamily)Current.Resources["Font"], TextWrapping = TextWrapping.Wrap, Text = $"{MainLang.UnhandledException}\n\n{e.ExceptionObject}", IsReadOnly = true, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                await Method.Ui.ShowDialogAsync(MainLang.GetException, p_content: textBox, b_primary: MainLang.Ok);
            }
            catch { }
        }

    }
}
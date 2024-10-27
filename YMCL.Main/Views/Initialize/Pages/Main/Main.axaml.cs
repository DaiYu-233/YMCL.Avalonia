using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Skin;
using MinecraftLaunch.Skin.Class.Fetchers;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Initialize.Pages.Main;

public partial class Main : UserControl
{
    public int _page = 1;
    public int _currentLang;

    public List<string> _mcFolderList =
        JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath))!;

    public List<JavaEntry> _javaList =
        JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath))!;

    private List<AccountInfo> _accountList =
        JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath));

    public Main()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Next.Click += (sender, e) =>
        {
            _page++;
            UpdatePage();
        };
        Precious.Click += (sender, e) =>
        {
            _page--;
            UpdatePage();
        };
        LanguageListBox.SelectionChanged += (_, e) =>
        {
            if (_currentLang == LanguageListBox.SelectedIndex) return;
            var lang = "zh-CN";
            switch (LanguageListBox.SelectedIndex)
            {
                case 0:
                    lang = "zh-CN";
                    break;
                case 1:
                    lang = "zh-Hant";
                    break;
                case 2:
                    lang = "en-US";
                    break;
                case 3:
                    lang = "ja-JP";
                    break;
                case 4:
                    lang = "ru-RU";
                    break;
            }

            var setting = Const.Data.Setting;
            setting.Language = lang;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            Method.Ui.RestartApp();
        };
        WindowTitleBarStyleListBox.SelectionChanged += (_, e) =>
        {
            var setting = Const.Data.Setting;
            if (WindowTitleBarStyleListBox.SelectedIndex == 0)
            {
                Const.Window.initialize.TitleBar.IsVisible = false;
                Const.Window.initialize.Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                Const.Window.initialize.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                Const.Window.initialize.ExtendClientAreaToDecorationsHint = false;
                setting.WindowTitleBarStyle = WindowTitleBarStyle.System;
            }
            else
            {
                Const.Window.initialize.TitleBar.IsVisible = true;
                Const.Window.initialize.Root.CornerRadius = new CornerRadius(8);
                Const.Window.initialize.WindowState = WindowState.Maximized;
                Const.Window.initialize.WindowState = WindowState.Normal;
                Const.Window.initialize.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                Const.Window.initialize.ExtendClientAreaToDecorationsHint = true;
                setting.WindowTitleBarStyle = WindowTitleBarStyle.Ymcl;
            }

            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        AutoScanMinecraftFolderBtn.Click += (_, _) =>
        {
            if (MinecraftFolderListBox.Items.Contains(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft")) ||
                !Path.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ".minecraft"))) return;
            _mcFolderList.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft"));
            MinecraftFolderListBox.Items.Add(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft"));
            File.WriteAllText(Const.String.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(_mcFolderList, Formatting.Indented));
        };
        ManualAddMinecraftFolderBtn.Click += async (_, _) =>
        {
            var result = await Method.IO.OpenFolderPicker(TopLevel.GetTopLevel(this)!,
                new FolderPickerOpenOptions { AllowMultiple = false, Title = MainLang.SelectMinecraftFolder });
            if (result != null && result.Count > 0)
            {
                var item = result[0];
                if (item.Name == ".minecraft")
                {
                    if (!_mcFolderList.Contains(item.Path))
                    {
                        _mcFolderList.Add(item.Path);
                        File.WriteAllText(Const.String.MinecraftFolderDataPath,
                            JsonConvert.SerializeObject(_mcFolderList, Formatting.Indented));
                        MinecraftFolderListBox.Items.Clear();
                        _mcFolderList.ForEach(folder => { MinecraftFolderListBox.Items.Add(folder); });
                        MinecraftFolderListBox.SelectedIndex = MinecraftFolderListBox.ItemCount - 1;
                        Method.Ui.Toast(MainLang.SuccessAdd + ": " + item.Path, Const.Notification.initialize,
                            NotificationType.Success);
                    }
                    else
                    {
                        Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.initialize,
                            NotificationType.Error);
                    }
                }
                else
                {
                    if (Directory.Exists(Path.Combine(item.Path, ".minecraft")))
                    {
                        if (!_mcFolderList.Contains(Path.Combine(item.Path, ".minecraft")))
                        {
                            _mcFolderList.Add(Path.Combine(item.Path, ".minecraft"));
                            File.WriteAllText(Const.String.MinecraftFolderDataPath,
                                JsonConvert.SerializeObject(_mcFolderList, Formatting.Indented));
                            MinecraftFolderListBox.Items.Clear();
                            _mcFolderList.ForEach(folder => { MinecraftFolderListBox.Items.Add(folder); });
                            MinecraftFolderListBox.SelectedIndex = MinecraftFolderListBox.ItemCount - 1;
                            Method.Ui.Toast(MainLang.SuccessAdd + ": " + Path.Combine(item.Path, ".minecraft"), Const.Notification.initialize,
                                NotificationType.Success);
                        }
                        else
                        {
                            Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.initialize,
                                NotificationType.Error);
                        }
                    }
                    else
                    {
                          Method.Ui.Toast(MainLang.CannotFindMinecraftFolder, Const.Notification.initialize, NotificationType.Error);
                    }
                }
            }
        };
        AutoScanJavaRuntimeBtn.Click += async (_, _) =>
        {
            var javaFetcher = new JavaFetcher();
            var javaList = await javaFetcher.FetchAsync();
            var repeatJavaCount = 0;
            var successAddCount = 0;
            foreach (var java in javaList)
                if (!_javaList.Contains(java))
                {
                    _javaList.Add(java);
                    successAddCount++;
                }
                else
                {
                    repeatJavaCount++;
                }

            JavaRuntimeListBox.Items.Clear();
            _javaList.ForEach(java => { JavaRuntimeListBox.Items.Add(java); });
            File.WriteAllText(Const.String.JavaDataPath, JsonConvert.SerializeObject(_javaList, Formatting.Indented));
            Method.Ui.Toast(
                $"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
                Const.Notification.initialize, NotificationType.Success);
        };
        ManualAddJavaRuntimeBtn.Click += async (_, _) =>
        {
            var list = await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this)!,
                new FilePickerOpenOptions() { AllowMultiple = false, Title = MainLang.SelectJava });
            list.ForEach(java =>
            {
                var javaInfo = JavaUtil.GetJavaInfo(java.Path);
                if (javaInfo == null && !string.IsNullOrWhiteSpace(java.Path))
                {
                    Method.Ui.Toast(MainLang.TheJavaIsError, Const.Notification.initialize, NotificationType.Error);
                }
                else
                {
                    if (_javaList.Contains(javaInfo!))
                        Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.initialize,
                            NotificationType.Error);
                    else
                        _javaList.Add(javaInfo!);
                }
            });
            JavaRuntimeListBox.Items.Clear();
            _javaList.ForEach(java => { JavaRuntimeListBox.Items.Add(java); });
            File.WriteAllText(Const.String.JavaDataPath, JsonConvert.SerializeObject(_javaList, Formatting.Indented));
        };
        AddAccountBtn.Click += async (s, e) =>
        {
            var comboBox = new ComboBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            comboBox.Items.Add(MainLang.OfflineLogin);
            comboBox.Items.Add(MainLang.MicrosoftLogin);
            comboBox.Items.Add(MainLang.ThirdPartyLogin);
            comboBox.SelectedIndex = 0;
            ContentDialog dialog = new()
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Title = MainLang.SelectAccountType,
                PrimaryButtonText = MainLang.Ok,
                CloseButtonText = MainLang.Cancel,
                DefaultButton = ContentDialogButton.Primary,
                Content = comboBox
            };
            var dialogResult = await dialog.ShowAsync(Const.Window.initialize);
            if (dialogResult == ContentDialogResult.Primary)
                switch (comboBox.SelectedIndex)
                {
                    case 0:
                        var textBox = new TextBox
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            TextWrapping = TextWrapping.Wrap
                        };
                        ContentDialog offlineDialog = new()
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            Title = MainLang.InputAccountName,
                            PrimaryButtonText = MainLang.Ok,
                            CloseButtonText = MainLang.Cancel,
                            DefaultButton = ContentDialogButton.Primary,
                            Content = textBox
                        };
                        var dialogResult1 = await offlineDialog.ShowAsync(Const.Window.initialize);
                        if (dialogResult1 == ContentDialogResult.Primary)
                        {
                            if (!string.IsNullOrWhiteSpace(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
                            {
                                var now = DateTime.Now;
                                _accountList.Add(new AccountInfo
                                {
                                    AccountType = AccountType.Offline,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = null,
                                    Name = textBox.Text
                                });
                                AccountListBox.Items.Clear();
                                var setting = Const.Data.Setting;
                                _accountList.ForEach(x =>
                                {
                                    SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
                                    var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
                                    var skin = Method.Value.BytesToBase64(bytes);
                                    AccountListBox.Items.Add(new AccountInfo
                                    {
                                        Name = x.Name,
                                        AccountType = x.AccountType,
                                        AddTime = x.AddTime,
                                        Data = x.Data,
                                        Bitmap = Method.Value.Base64ToBitmap(skin)
                                    });
                                });
                                File.WriteAllText(Const.String.AccountDataPath,
                                    JsonConvert.SerializeObject(_accountList, Formatting.Indented));
                            }
                            else
                            {
                                Method.Ui.Toast(MainLang.AccountNameCannotBeNull, Const.Notification.initialize,
                                    NotificationType.Error);
                            }
                        }

                        break;
                    case 1:
                        var verificationUrl = string.Empty;
                        var verificationCode = string.Empty;
                        MicrosoftAccount userProfile = null;
                        var textBlock = new TextBlock
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            TextWrapping = TextWrapping.Wrap, Text = MainLang.Loading,
                            HorizontalAlignment = HorizontalAlignment.Center, FontSize = 16
                        };
                        ContentDialog microsoftDialog = new()
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            Title = MainLang.VerificationCode,
                            PrimaryButtonText = MainLang.CopyCodeAndOPenBrowser,
                            SecondaryButtonText = MainLang.CannotOpenBrowser,
                            CloseButtonText = MainLang.Cancel,
                            DefaultButton = ContentDialogButton.Primary,
                            Content = textBlock,
                            IsPrimaryButtonEnabled = false,
                            IsSecondaryButtonEnabled = false
                        };
                        MicrosoftAuthenticator authenticator = new(Const.String.AzureClientId);
                        microsoftDialog.PrimaryButtonClick += async (_, _) =>
                        {
                            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                            await clipboard.SetTextAsync(textBlock.Text);
                            var launcher = TopLevel.GetTopLevel(this).Launcher;
                            await launcher.LaunchUriAsync(new Uri(verificationUrl));
                            Method.Ui.Toast(MainLang.WaitForMicrosoftVerification, Const.Notification.initialize);
                        };
                        microsoftDialog.SecondaryButtonClick += (_, _) =>
                        {
                            var urlBox = new TextBox
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                                TextWrapping = TextWrapping.Wrap, IsReadOnly = true
                            };
                            var tip = new TextBlock
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                                Text = MainLang.CopyUrlAndManualOpen
                            };
                            var codeBox = new TextBox
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                                TextWrapping = TextWrapping.Wrap, IsReadOnly = true, Text = verificationCode
                            };
                            var codeTip = new TextBlock
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                                Text = MainLang.VerificationCode
                            };
                            var stackPanel = new StackPanel { Spacing = 10 };
                            stackPanel.Children.Add(tip);
                            stackPanel.Children.Add(urlBox);
                            stackPanel.Children.Add(codeTip);
                            stackPanel.Children.Add(codeBox);
                            ContentDialog urlDialog = new()
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                                Title = MainLang.ManualOpen,
                                PrimaryButtonText = MainLang.Ok,
                                DefaultButton = ContentDialogButton.Primary,
                                Content = stackPanel
                            };
                            urlBox.Text = verificationUrl;
                            _ = urlDialog.ShowAsync(Const.Window.initialize);
                        };
                        _ = microsoftDialog.ShowAsync(Const.Window.initialize);
                        try
                        {
                            await authenticator.DeviceFlowAuthAsync(device =>
                            {
                                textBlock.Text = device.UserCode;
                                verificationUrl = device.VerificationUrl;
                                verificationCode = device.UserCode;
                                microsoftDialog.IsPrimaryButtonEnabled = true;
                                microsoftDialog.IsSecondaryButtonEnabled = true;
                            });
                            userProfile = await authenticator.AuthenticateAsync();
                        }
                        catch (Exception ex)
                        {
                            Method.Ui.ShowShortException(MainLang.LoginFail, ex);
                            return;
                        }

                        try
                        {
                            Method.Ui.Toast(MainLang.VerifyingAccount, Const.Notification.initialize);
                            MicrosoftSkinFetcher skinFetcher = new(userProfile.Uuid.ToString());
                            var bytes = await skinFetcher.GetSkinAsync();
                            var now = DateTime.Now;
                            _accountList.Add(new AccountInfo
                            {
                                AccountType = AccountType.Microsoft,
                                AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                Data = JsonConvert.SerializeObject(userProfile, Formatting.Indented),
                                Name = userProfile.Name,
                                Skin = Method.Value.BytesToBase64(bytes)
                            });
                            AccountListBox.Items.Clear();
                            var setting = Const.Data.Setting;
                            _accountList.ForEach(x =>
                            {
                                SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
                                var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
                                var skin = Method.Value.BytesToBase64(bytes);
                                AccountListBox.Items.Add(new AccountInfo
                                {
                                    Name = x.Name,
                                    AccountType = x.AccountType,
                                    AddTime = x.AddTime,
                                    Data = x.Data,
                                    Bitmap = Method.Value.Base64ToBitmap(skin)
                                });
                            });

                            File.WriteAllText(Const.String.AccountDataPath,
                                JsonConvert.SerializeObject(_accountList, Formatting.Indented));
                            Const.Window.initialize.Activate();
                        }
                        catch (Exception ex)
                        {
                            Method.Ui.ShowShortException(MainLang.LoginFail, ex);
                        }

                        break;
                    case 2:
                        YggdrasilLogin();
                        break;
                }
        };
    }

    private async void UpdatePage()
    {
        Precious.IsEnabled = _page != 1;
        LanguageRoot.Opacity = 0;
        WindowTitleBarStyleRoot.Opacity = 0;
        MinecraftFolderRoot.Opacity = 0;
        JavaRuntimeRoot.Opacity = 0;
        AccountRoot.Opacity = 0;
        await Task.Delay(150);
        LanguageRoot.IsVisible = false;
        WindowTitleBarStyleRoot.IsVisible = false;
        MinecraftFolderRoot.IsVisible = false;
        JavaRuntimeRoot.IsVisible = false;
        AccountRoot.IsVisible = false;
        if (_page == 1)
        {
            LanguageRoot.IsVisible = true;
            LanguageRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        }

        if (_page == 2)
        {
            WindowTitleBarStyleRoot.IsVisible = true;
            WindowTitleBarStyleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
            var lang = "zh-CN";
            switch (LanguageListBox.SelectedIndex)
            {
                case 0:
                    lang = "zh-CN";
                    break;
                case 1:
                    lang = "zh-Hant";
                    break;
                case 2:
                    lang = "en-US";
                    break;
                case 3:
                    lang = "ja-JP";
                    break;
                case 4:
                    lang = "ru-RU";
                    break;
            }

            var setting = Const.Data.Setting;
            setting.Language = lang;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        if (_page == 3)
        {
            MinecraftFolderRoot.IsVisible = true;
            MinecraftFolderRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
            var setting = Const.Data.Setting;
            setting.WindowTitleBarStyle = WindowTitleBarStyleListBox.SelectedIndex == 0
                ? WindowTitleBarStyle.System
                : WindowTitleBarStyle.Ymcl;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        if (_page == 4)
        {
            JavaRuntimeRoot.IsVisible = true;
            JavaRuntimeRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
            var setting = Const.Data.Setting;
            setting.IsCompleteMinecraftFolderInitialize = true;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        if (_page == 5)
        {
            AccountRoot.IsVisible = true;
            AccountRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
            var setting = Const.Data.Setting;
            setting.IsCompleteJavaInitialize = true;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        if (_page == 6)
        {
            var setting = Const.Data.Setting;
            setting.IsCompleteAccountInitialize = true;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            Method.Ui.RestartApp();
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var setting = Const.Data.Setting;
        _mcFolderList.ForEach(x => MinecraftFolderListBox.Items.Add(x));
        _javaList.ForEach(x => JavaRuntimeListBox.Items.Add(x));
        _accountList.ForEach(x =>
        {
            SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
            var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
            var skin = Method.Value.BytesToBase64(bytes);
            AccountListBox.Items.Add(new AccountInfo
            {
                Name = x.Name,
                AccountType = x.AccountType,
                AddTime = x.AddTime,
                Data = x.Data,
                Bitmap = Method.Value.Base64ToBitmap(skin)
            });
        });

        void InitLangSelection()
        {
            _currentLang = setting.Language switch
            {
                "zh-CN" => 0,
                "zh-Hant" => 1,
                "en-US" => 2,
                "ja-JP" => 3,
                "ru-RU" => 4,
                _ => 0
            };

            LanguageListBox.SelectedIndex = _currentLang;
        }

        InitLangSelection();
        if (setting.Language == "Unset")
        {
            Const.Window.initialize.Show();
            Const.Window.initialize.SystemDecorations = SystemDecorations.Full;
            Precious.IsEnabled = false;
            _page = 1;
            UpdatePage();
            return;
        }

        Precious.IsEnabled = true;
        if (setting.WindowTitleBarStyle == WindowTitleBarStyle.Unset)
        {
            Const.Window.initialize.Show();
            Const.Window.initialize.SystemDecorations = SystemDecorations.Full;
            _page = 2;
            UpdatePage();
            return;
        }

        if (!setting.IsCompleteMinecraftFolderInitialize)
        {
            Const.Window.initialize.Show();
            Const.Window.initialize.SystemDecorations = SystemDecorations.Full;
            _page = 3;
            UpdatePage();
            return;
        }

        if (!setting.IsCompleteJavaInitialize)
        {
            Const.Window.initialize.Show();
            Const.Window.initialize.SystemDecorations = SystemDecorations.Full;
            _page = 4;
            UpdatePage();
            return;
        }

        if (!setting.IsCompleteAccountInitialize)
        {
            Const.Window.initialize.Show();
            Const.Window.initialize.SystemDecorations = SystemDecorations.Full;
            _page = 5;
            UpdatePage();
            return;
        }

        Const.Window.initialize._exit = false;
        Const.Window.initialize.Hide();
        Const.Window.main.LoadWindow();
    }

    private async void YggdrasilLogin(string server1 = "", string email1 = "", string password1 = "")
    {
        var stackPanel = new StackPanel { Spacing = 10, Width = 580 };
        var verificationSeverUrlTextBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Watermark = MainLang.VerificationServer, Text = server1, HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = 500
        };
        var emailTextBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Watermark = MainLang.EmailAddress, Text = email1, HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = 500
        };
        var passwordTextBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
            Watermark = MainLang.AccountPassword, Text = password1, HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = 500
        };
        stackPanel.Children.Add(verificationSeverUrlTextBox);
        stackPanel.Children.Add(emailTextBox);
        stackPanel.Children.Add(passwordTextBox);
        ContentDialog thirdPartyDialog = new()
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            Title = MainLang.ThirdPartyLogin,
            PrimaryButtonText = MainLang.Ok,
            CloseButtonText = MainLang.Cancel,
            DefaultButton = ContentDialogButton.Primary,
            Content = stackPanel
        };
        var thirdPartyDialogResult = await thirdPartyDialog.ShowAsync(Const.Window.initialize);
        if (thirdPartyDialogResult == ContentDialogResult.Primary)
        {
            var server = verificationSeverUrlTextBox.Text;
            var email = emailTextBox.Text;
            var password = passwordTextBox.Text;
            var reInput = false;
            if (string.IsNullOrWhiteSpace(server) && string.IsNullOrWhiteSpace(server))
            {
                Method.Ui.Toast(MainLang.YggdrasilServerUrlIsEmpty, Const.Notification.initialize,
                    NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(email))
            {
                Method.Ui.Toast(MainLang.YggdrasilEmailIsEmpty, Const.Notification.initialize, NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(password))
            {
                Method.Ui.Toast(MainLang.YggdrasilPasswordIsEmpty, Const.Notification.initialize,
                    NotificationType.Error);
                reInput = true;
            }

            if (reInput)
            {
                YggdrasilLogin(server, email, password);
            }
            else
            {
                IEnumerable<YggdrasilAccount> yggdrasilAccounts = null;
                try
                {
                    YggdrasilAuthenticator authenticator = new(server, email, password);
                    Method.Ui.Toast(MainLang.VerifyingAccount, Const.Notification.initialize);
                    yggdrasilAccounts = await authenticator.AuthenticateAsync();
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowShortException(MainLang.LoginFail, ex);
                    return;
                }

                try
                {
                    foreach (var account in yggdrasilAccounts)
                    {
                        var now = DateTime.Now;
                        try
                        {
                            YggdrasilSkinFetcher skinFetcher = new(account.YggdrasilServerUrl, account.Uuid.ToString());
                            var bytes = await skinFetcher.GetSkinAsync();
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _accountList.Add(new AccountInfo
                                {
                                    AccountType = AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name,
                                    Skin = Method.Value.BytesToBase64(bytes)
                                });
                                AccountListBox.Items.Clear();
                                var setting = Const.Data.Setting;
                                _accountList.ForEach(x =>
                                {
                                    SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
                                    var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
                                    var skin = Method.Value.BytesToBase64(bytes);
                                    AccountListBox.Items.Add(new AccountInfo
                                    {
                                        Name = x.Name,
                                        AccountType = x.AccountType,
                                        AddTime = x.AddTime,
                                        Data = x.Data,
                                        Bitmap = Method.Value.Base64ToBitmap(skin)
                                    });
                                });
                            });
                        }
                        catch
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _accountList.Add(new AccountInfo
                                {
                                    AccountType = AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name
                                });
                                AccountListBox.Items.Clear();
                                var setting = Const.Data.Setting;
                                _accountList.ForEach(x =>
                                {
                                    SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
                                    var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
                                    var skin = Method.Value.BytesToBase64(bytes);
                                    AccountListBox.Items.Add(new AccountInfo
                                    {
                                        Name = x.Name,
                                        AccountType = x.AccountType,
                                        AddTime = x.AddTime,
                                        Data = x.Data,
                                        Bitmap = Method.Value.Base64ToBitmap(skin)
                                    });
                                });
                            });
                        }
                    }

                    File.WriteAllText(Const.String.AccountDataPath,
                        JsonConvert.SerializeObject(_accountList, Formatting.Indented));
                    Const.Window.initialize.Activate();
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowShortException(MainLang.LoginFail, ex);
                }
            }
        }
    }
}
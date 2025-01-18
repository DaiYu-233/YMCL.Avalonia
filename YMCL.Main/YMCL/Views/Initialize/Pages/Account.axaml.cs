using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Skin;
using MinecraftLaunch.Skin.Class.Fetchers;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;
using YMCL.Public.Module.Ui;
using YMCL.Public.Module.Value;
using Setting = YMCL.Public.Enum.Setting;
using String = YMCL.Public.Const.String;

namespace YMCL.Views.Initialize.Pages;

public partial class Account : UserControl
{
    public Account()
    {
        InitializeComponent();
        BindingEvent();
        AccountListBox.ItemsSource = Data.Accounts;
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
        var thirdPartyDialogResult = await thirdPartyDialog.ShowAsync();
        if (thirdPartyDialogResult == ContentDialogResult.Primary)
        {
            var server = verificationSeverUrlTextBox.Text;
            var email = emailTextBox.Text;
            var password = passwordTextBox.Text;
            var reInput = false;
            if (string.IsNullOrWhiteSpace(server) && string.IsNullOrWhiteSpace(server))
            {
                Toast(MainLang.YggdrasilServerUrlIsEmpty, NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(email))
            {
                Toast(MainLang.YggdrasilEmailIsEmpty,NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(password))
            {
                Toast(MainLang.YggdrasilPasswordIsEmpty, NotificationType.Error);
                reInput = true;
            }

            if (reInput)
            {
                YggdrasilLogin(server, email, password);
            }
            else
            {
                IEnumerable<YggdrasilAccount> yggdrasilAccounts;
                try
                {
                    YggdrasilAuthenticator authenticator = new(server, email, password);
                    Toast(MainLang.VerifyingAccount);
                    yggdrasilAccounts = await authenticator.AuthenticateAsync();
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.LoginFail, ex);
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
                                Data.Accounts.Add(new AccountInfo
                                {
                                    AccountType = Setting.AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name,
                                    Skin = Converter.BytesToBase64(bytes)
                                });
                            });
                        }
                        catch
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Data.Accounts.Add(new AccountInfo
                                {
                                    AccountType = Setting.AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name
                                });
                            });
                        }
                    }

                    await File.WriteAllTextAsync(ConfigPath.AccountDataPath,
                        JsonConvert.SerializeObject(Data.Accounts, Formatting.Indented));
                    if (TopLevel.GetTopLevel(this) is Window window)
                    {
                        window.Activate();
                    }
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.LoginFail, ex);
                }
            }
        }
    }
    
    private void BindingEvent()
    {
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
            var dialogResult = await dialog.ShowAsync();
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
                        var dialogResult1 = await offlineDialog.ShowAsync();
                        if (dialogResult1 == ContentDialogResult.Primary)
                        {
                            if (!string.IsNullOrWhiteSpace(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
                            {
                                var now = DateTime.Now;
                                Data.Accounts.Add(new AccountInfo
                                {
                                    AccountType = Setting.AccountType.Offline,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = null,
                                    Name = textBox.Text
                                });
                                await File.WriteAllTextAsync(ConfigPath.AccountDataPath,
                                    JsonConvert.SerializeObject(Data.Accounts, Formatting.Indented));
                            }
                            else
                            {
                                Toast(MainLang.AccountNameCannotBeNull, NotificationType.Error);
                            }
                        }
                        await File.WriteAllTextAsync(ConfigPath.AccountDataPath,
                            JsonConvert.SerializeObject(Data.Accounts, Formatting.Indented));
                        break;
                    case 1:
                        var verificationUrl = string.Empty;
                        var verificationCode = string.Empty;
                        MicrosoftAccount userProfile;
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
                        MicrosoftAuthenticator authenticator = new(String.AzureClientId);
                        microsoftDialog.PrimaryButtonClick += async (_, _) =>
                        {
                            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                            await clipboard.SetTextAsync(textBlock.Text);
                            var launcher = TopLevel.GetTopLevel(this).Launcher;
                            await launcher.LaunchUriAsync(new Uri(verificationUrl));
                            Toast(MainLang.WaitForMicrosoftVerification);
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
                            _ = urlDialog.ShowAsync();
                        };
                        _ = microsoftDialog.ShowAsync();
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
                            ShowShortException(MainLang.LoginFail, ex);
                            return;
                        }

                        try
                        {
                            Toast(MainLang.VerifyingAccount);
                            MicrosoftSkinFetcher skinFetcher = new(userProfile.Uuid.ToString());
                            var bytes = await skinFetcher.GetSkinAsync();
                            var now = DateTime.Now;
                            Data.Accounts.Add(new AccountInfo
                            {
                                AccountType = Setting.AccountType.Microsoft,
                                AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                Data = JsonConvert.SerializeObject(userProfile, Formatting.Indented),
                                Name = userProfile.Name,
                                Skin = Converter.BytesToBase64(bytes)
                            });
                            AccountListBox.Items.Clear();
                            await File.WriteAllTextAsync(ConfigPath.AccountDataPath,
                                JsonConvert.SerializeObject(Data.Accounts, Formatting.Indented));
                            if (TopLevel.GetTopLevel(this) is Window window)
                            {
                                window.Activate();
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowShortException(MainLang.LoginFail, ex);
                        }
                        await File.WriteAllTextAsync(ConfigPath.AccountDataPath,
                            JsonConvert.SerializeObject(Data.Accounts, Formatting.Indented));
                        break;
                    case 2:
                        YggdrasilLogin();
                        break;
                }
        };
    }
}
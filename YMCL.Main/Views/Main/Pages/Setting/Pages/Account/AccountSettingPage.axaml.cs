using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Skin;
using MinecraftLaunch.Skin.Class.Fetchers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using AccountType = YMCL.Main.Public.AccountType;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Account;

public partial class AccountSettingPage : UserControl
{
    private List<AccountInfo> accounts =
        JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath));

    public AccountSettingPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            var setting = Const.Data.Setting;
            if (setting.AccountSelectionIndex + 1 <= AccountsListView.Items.Count)
            {
                AccountsListView.SelectedIndex = setting.AccountSelectionIndex;
            }
            else
            {
                AccountsListView.SelectedItem = AccountsListView.Items[0];
                setting.AccountSelectionIndex = 0;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            try
            {
                if (AccountsListView.SelectedItem != null)
                {
                    var isMicrosoftAccount = (AccountsListView.SelectedItem as AccountInfo).AccountType ==
                                             AccountType.Microsoft;
                    ModifyMicrosoftSkinBtn.IsEnabled = isMicrosoftAccount ? true : false;
                    RefreshMicrosoftSkinBtn.IsEnabled = isMicrosoftAccount ? true : false;
                }
            }
            catch
            {
            }
        };
        ModifyMicrosoftSkinBtn.Click += async (s, e) =>
        {
            var comboBox = new ComboBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            comboBox.Items.Add("Steve");
            comboBox.Items.Add("Alex");
            comboBox.SelectedIndex = 0;
            var dialog =
                await Method.Ui.ShowDialogAsync(MainLang.SkinModel, p_content: comboBox, b_primary: MainLang.Ok);
            if (dialog == ContentDialogResult.Primary)
            {
                if (AccountsListView.SelectedItem == null) return;
                var account = AccountsListView.SelectedItem as AccountInfo;
                var list = await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this)!,
                    new FilePickerOpenOptions { AllowMultiple = false, Title = MainLang.SelectSkinFile },
                    MainLang.InputSkinFilePath);
                if (list == null || list[0].Path == null) return;
                if (account.AccountType != AccountType.Microsoft) return;
                var jsonObject = JObject.Parse(account.Data!);
                Method.Ui.Toast(MainLang.Loading);
                var result = await Method.IO.UploadMicrosoftSkin(list[0].Path, jsonObject["Uuid"].ToString(),
                    comboBox.SelectedIndex == 0 ? "" : "slim", jsonObject["AccessToken"].ToString());
                if (result) RefreshSelectedAccountSkin();
            }
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
            var dialogResult = await dialog.ShowAsync(Const.Window.main);
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
                        var dialogResult1 = await offlineDialog.ShowAsync(Const.Window.main);
                        if (dialogResult1 == ContentDialogResult.Primary)
                        {
                            if (!string.IsNullOrWhiteSpace(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
                            {
                                var now = DateTime.Now;
                                accounts.Add(new AccountInfo
                                {
                                    AccountType = AccountType.Offline,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = null,
                                    Name = textBox.Text
                                });

                                File.WriteAllText(Const.String.AccountDataPath,
                                    JsonConvert.SerializeObject(accounts, Formatting.Indented));
                                LoadAccounts();
                            }
                            else
                            {
                                Method.Ui.Toast(MainLang.AccountNameCannotBeNull, Const.Notification.main,
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
                            Method.Ui.Toast(MainLang.WaitForMicrosoftVerification, Const.Notification.main);
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
                            _ = urlDialog.ShowAsync(Const.Window.main);
                        };
                        _ = microsoftDialog.ShowAsync(Const.Window.main);
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
                            Method.Ui.Toast(MainLang.VerifyingAccount, Const.Notification.main);
                            MicrosoftSkinFetcher skinFetcher = new(userProfile.Uuid.ToString());
                            var bytes = await skinFetcher.GetSkinAsync();
                            var now = DateTime.Now;
                            accounts.Add(new AccountInfo
                            {
                                AccountType = AccountType.Microsoft,
                                AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                Data = JsonConvert.SerializeObject(userProfile, Formatting.Indented),
                                Name = userProfile.Name,
                                Skin = Method.Value.BytesToBase64(bytes)
                            });

                            File.WriteAllText(Const.String.AccountDataPath,
                                JsonConvert.SerializeObject(accounts, Formatting.Indented));
                            LoadAccounts();
                            Const.Window.main.Activate();
                            AccountsListView.SelectedIndex = AccountsListView.Items.Count - 1;
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
        RefreshMicrosoftSkinBtn.Click += (_, _) => { RefreshSelectedAccountSkin(); };
        DelSeletedAccountBtn.Click += (_, _) =>
        {
            accounts.RemoveAt(AccountsListView.SelectedIndex);
            if (accounts.Count == 0)
                accounts.Add(new AccountInfo
                {
                    AccountType = AccountType.Offline,
                    AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Name = "Steve"
                });
            File.WriteAllText(Const.String.AccountDataPath, JsonConvert.SerializeObject(accounts, Formatting.Indented));
            LoadAccounts();
            AccountsListView.SelectedIndex = 0;
        };
        AccountsListView.SelectionChanged += (_, _) =>
        {
            try
            {
                if (AccountsListView.SelectedItem != null)
                {
                    var isMicrosoftAccount = (AccountsListView.SelectedItem as AccountInfo).AccountType ==
                                             AccountType.Microsoft;
                    ModifyMicrosoftSkinBtn.IsEnabled = isMicrosoftAccount ? true : false;
                    RefreshMicrosoftSkinBtn.IsEnabled = isMicrosoftAccount ? true : false;
                }
            }
            catch
            {
            }

            var setting = Const.Data.Setting;
            setting.AccountSelectionIndex = AccountsListView.SelectedIndex;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
    }

    private void ControlProperty()
    {
        var setting = Const.Data.Setting;
        LoadAccounts();
    }

    private void LoadAccounts()
    {
        accounts = JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath));
        AccountsListView.Items.Clear();
        var setting = Const.Data.Setting;
        accounts.ForEach(x =>
        {
            SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
            var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
            var skin = Method.Value.BytesToBase64(bytes);
            AccountsListView.Items.Add(new AccountInfo
            {
                Name = x.Name,
                AccountType = x.AccountType,
                AddTime = x.AddTime,
                Data = x.Data,
                Bitmap = Method.Value.Base64ToBitmap(skin)
            });
        });

        if (AccountsListView.Items.Count > 0)
        {
            if (setting.AccountSelectionIndex + 1 <= AccountsListView.Items.Count)
            {
                AccountsListView.SelectedIndex = setting.AccountSelectionIndex;
            }
            else
            {
                AccountsListView.SelectedItem = AccountsListView.Items[0];
                setting.AccountSelectionIndex = 0;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        }
        else
        {
            var now = DateTime.Now;
            File.WriteAllText(Const.String.AccountDataPath, JsonConvert.SerializeObject(new List<AccountInfo>
            {
                new()
                {
                    AccountType = AccountType.Offline,
                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Name = "Steve"
                }
            }, Formatting.Indented));
            setting.AccountSelectionIndex = 0;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            LoadAccounts();
        }

        if (setting.AccountSelectionIndex == -1 && accounts.Count > 0)
        {
            setting.AccountSelectionIndex = 0;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            LoadAccounts();
        }
    }

    public async void YggdrasilLogin(string server1 = "", string email1 = "", string password1 = "")
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
        var thirdPartyDialogResult = await thirdPartyDialog.ShowAsync(Const.Window.main);
        if (thirdPartyDialogResult == ContentDialogResult.Primary)
        {
            var server = verificationSeverUrlTextBox.Text;
            var email = emailTextBox.Text;
            var password = passwordTextBox.Text;
            var reInput = false;
            if (string.IsNullOrWhiteSpace(server) && string.IsNullOrWhiteSpace(server))
            {
                Method.Ui.Toast(MainLang.YggdrasilServerUrlIsEmpty, Const.Notification.main, NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(email))
            {
                Method.Ui.Toast(MainLang.YggdrasilEmailIsEmpty, Const.Notification.main, NotificationType.Error);
                reInput = true;
            }

            if (string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(password))
            {
                Method.Ui.Toast(MainLang.YggdrasilPasswordIsEmpty, Const.Notification.main, NotificationType.Error);
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
                    Method.Ui.Toast(MainLang.VerifyingAccount, Const.Notification.main);
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
                                accounts.Add(new AccountInfo
                                {
                                    AccountType = AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name,
                                    Skin = Method.Value.BytesToBase64(bytes)
                                });
                            });
                        }
                        catch
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                accounts.Add(new AccountInfo
                                {
                                    AccountType = AccountType.ThirdParty,
                                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Data = JsonConvert.SerializeObject(account, Formatting.Indented),
                                    Name = account.Name
                                });
                            });
                        }
                    }

                    File.WriteAllText(Const.String.AccountDataPath,
                        JsonConvert.SerializeObject(accounts, Formatting.Indented));
                    LoadAccounts();
                    Const.Window.main.Activate();
                    AccountsListView.SelectedIndex = AccountsListView.Items.Count - 1;
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowShortException(MainLang.LoginFail, ex);
                }
            }
        }
    }

    private async void RefreshSelectedAccountSkin()
    {
        if (AccountsListView.SelectedItem == null) return;
        var account = (AccountInfo)AccountsListView.SelectedItem;
        var index = AccountsListView.SelectedIndex;
        if (account.AccountType != AccountType.Microsoft) return;
        var jsonObject = JObject.Parse(account.Data!);
        MicrosoftSkinFetcher skinFetcher = new(jsonObject["Uuid"].ToString());
        var bytes = await skinFetcher.GetSkinAsync();
        var now = DateTime.Now;
        account.Skin = Method.Value.BytesToBase64(bytes);
        account.Bitmap = null;
        accounts[index] = account;
        File.WriteAllText(Const.String.AccountDataPath, JsonConvert.SerializeObject(accounts, Formatting.Indented));
        LoadAccounts();
    }
}
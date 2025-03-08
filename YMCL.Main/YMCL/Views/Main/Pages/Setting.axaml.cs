﻿using FluentAvalonia.UI.Controls;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages;

public partial class Setting : UserControl
{
    public readonly SettingPages.Launch _launch = new();
    public readonly SettingPages.Account _account = new();
    public readonly SettingPages.Personalize _personalize = new();
    public readonly SettingPages.Download _download = new();
    public readonly SettingPages.Plugin _plugin = new();
    public readonly SettingPages.Launcher _launcher = new();

    public Setting()
    {
        InitializeComponent();
        FrameView.Content = _launch;
        BindingEvent();
    }

    private void BindingEvent()
    {
        Nav.SelectionChanged += (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            var page = tag switch
            {
                "launch" => _launch,
                "account" => _account,
                "personalize" => _personalize,
                "download" => _download,
                "launcher" => _launcher,
                "plugin" => _plugin,
                _ => FrameView.Content as UserControl
            };
            FrameView.Content = page;
            _ = Animator.PageLoading.LevelTwoPage(page);
        };
        Loaded += (_, _) =>
        {
            _ = Animator.PageLoading.LevelTwoPage(FrameView.Content as UserControl);
        };
    }
}
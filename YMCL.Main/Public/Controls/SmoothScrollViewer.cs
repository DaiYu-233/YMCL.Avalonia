using System;
using Avalonia;
using Avalonia.Controls;

namespace YMCL.Main.Public.Controls;

public sealed class SmoothScrollViewer : ScrollViewer {
    protected override Type StyleKeyOverride => typeof(SmoothScrollViewer);

    public static readonly StyledProperty<bool> UseSmoothScrollingProperty = 
        AvaloniaProperty.Register<SmoothScrollViewer, bool>(nameof(UseSmoothScrolling), defaultValue: true);

    public static readonly StyledProperty<double> SmoothScrollingStepProperty = 
        AvaloniaProperty.Register<SmoothScrollViewer, double>(nameof(SmoothScrollingStep), 200);

    public static readonly StyledProperty<bool> UseCustomScrollAnimationProperty = 
        AvaloniaProperty.Register<SmoothScrollViewer, bool>(nameof(UseCustomScrollAnimation), defaultValue: true);

    public bool UseSmoothScrolling {
        get => GetValue(UseSmoothScrollingProperty);
        set => SetValue(UseSmoothScrollingProperty, value);
    }

    public double SmoothScrollingStep {
        get => GetValue(SmoothScrollingStepProperty);
        set => SetValue(SmoothScrollingStepProperty, value);
    }

    public bool UseCustomScrollAnimation {
        get => GetValue(UseCustomScrollAnimationProperty);
        set => SetValue(UseCustomScrollAnimationProperty, value);
    }
}
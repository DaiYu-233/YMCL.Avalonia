using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Layout;

namespace YMCL.Public.Module.Ui;

public class Animator
{
    public class PageLoading
    {
        public static async Task LevelOnePage(UserControl? control)
        {
            if (control != null)
            {
                control.IsVisible = false;
                control.Transitions = [];
                control.Margin = new Thickness(-50, 0, 50, 0);
                control.Opacity = 0;
                control.Transitions.Add(new ThicknessTransition
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    Easing = new SineEaseInOut(),
                    Property = Layoutable.MarginProperty
                });
                control.Transitions.Add(new DoubleTransition
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    Easing = new SineEaseInOut(),
                    Property = Visual.OpacityProperty
                });
                control.IsVisible = true;
                control.Margin = new Thickness(0);
                control.Opacity = 1;
                await Task.Delay(TimeSpan.FromSeconds(0.35));
            }
        }
        
        public static async Task LevelTwoPage(Control? control)
        {
            if (control != null)
            {
                control.IsVisible = false;
                control.Transitions = [];
                control.Margin = new Thickness(0, 50, 0, -50);
                control.Opacity = 0;
                control.Transitions.Add(new ThicknessTransition
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    Easing = new SineEaseInOut(),
                    Property = Layoutable.MarginProperty
                });
                control.Transitions.Add(new DoubleTransition
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    Easing = new SineEaseInOut(),
                    Property = Visual.OpacityProperty
                });
                control.IsVisible = true;
                control.Margin = new Thickness(0);
                control.Opacity = 1;
                await Task.Delay(TimeSpan.FromSeconds(0.35));
            }
        }
        
        public static async Task ReversalLevelTwoPage(Control? control)
        {
            if (control != null)
            {
                control.IsVisible = false;
                control.Transitions = [];
                control.Margin = new Thickness(0);
                control.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
                control.Transitions.Add(new ThicknessTransition
                {
                    Duration = TimeSpan.FromSeconds(0.45),
                    Easing = new SineEaseInOut(),
                    Property = Layoutable.MarginProperty
                });
                control.Transitions.Add(new DoubleTransition
                {
                    Duration = TimeSpan.FromSeconds(0.45),
                    Easing = new SineEaseInOut(),
                    Property = Visual.OpacityProperty
                });
                control.IsVisible = true;
                control.Margin = new Thickness(0, 50, 0, -50);
                control.Opacity = 0;
                await Task.Delay(TimeSpan.FromSeconds(0.45));
            }
        }
    }
}
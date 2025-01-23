using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Utilities;

namespace YMCL.Public.Module.Value;

public class Calculator
{
    public static Color ColorVariant(Color color, float percent)
    {
        // 确保百分比在-1到1之间  
        percent = Math.Max(-1f, Math.Min(1f, percent));

        // 计算调整后的RGB值  
        var adjust = 1f + percent; // 亮化是1+percent，暗化是1+(negative percent)，即小于1  
        var r = (int)Math.Round(color.R * adjust);
        var g = (int)Math.Round(color.G * adjust);
        var b = (int)Math.Round(color.B * adjust);

        // 确保RGB值在有效范围内  
        r = Math.Max(0, Math.Min(255, r));
        g = Math.Max(0, Math.Min(255, g));
        b = Math.Max(0, Math.Min(255, b));

        // 创建一个新的颜色（保持Alpha通道不变）  
        return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
    }

    public static JavaEntry? GetCurrentJava(List<JavaEntry> javaEntries, GameEntry game)
    {
        JavaEntry? result = null;
        try
        {
            List<MinecraftLaunch.Classes.Models.Game.JavaEntry> list = [];
            list.AddRange(javaEntries.Select(JavaEntry.YmclToMl));
            result = JavaEntry.MlToYmcl(JavaUtil.GetCurrentJava(list, game));
        }
        catch
        {
            foreach (var javaEntry in javaEntries)
            {
                if (javaEntry.JavaSlugVersion != game.JavaVersion) continue;
                result = javaEntry;
                break;
            }
        }
        return result;
    }
}
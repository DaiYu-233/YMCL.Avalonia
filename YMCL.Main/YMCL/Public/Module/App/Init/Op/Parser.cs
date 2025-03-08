using System.Collections.Generic;
using System.Linq;
using YMCL.Public.Module.App.Init.Op.SubModule;

namespace YMCL.Public.Module.App.Init.Op;

public class Parser
{
    public static readonly List<string> operations = [];
    public static void Handle(string[] args)
    {
        if(args.Length==0) return;
        if (args[0].StartsWith("ymcl://"))
        {
            Notice(args[0]);
            var urlDecode = System.Web.HttpUtility.UrlDecode(args[0]);
            var replace = urlDecode.Replace("ymcl://", "");
            Parse(replace.Trim().TrimEnd('/').Split(' '));
        }
        else
        {
            Parse(args);
        }
    }
    
    public static void Parse(string[] args)
    {
        foreach (var a in args)
        {
            if (a.Trim().StartsWith("--"))
            {
                operations.Add(a);
            }
            else
            {
                var str = operations.LastOrDefault();
                if (string.IsNullOrWhiteSpace(str)) continue;
                str += $" {a}";
                operations[^1] = str.Trim();
            }
        }
        foreach (var operation in operations)
        {
            switch (operation.Split(' ')[0])
            {
                case "--import-setting":
                    _ = ImportSetting.Invoke(operation);
                    break;
                case "--install-modpack":
                    InstallModPack.Invoke(operation);
                    break;
            }
        }
    }
}
using System.Collections.Generic;

namespace YMCL.Public.Module.Util;

public class VersionComparer : IComparer<string>
{
    public int Compare(string x, string y)
    {
        var versionPartsX = x.Split('.');
        var versionPartsY = y.Split('.');

        var minLength = Math.Min(versionPartsX.Length, versionPartsY.Length);

        for (var i = 0; i < minLength; i++)
        {
            var partX = int.Parse(versionPartsX[i]);
            var partY = int.Parse(versionPartsY[i]);

            if (partX != partY) return partX.CompareTo(partY);
        }

        // 如果所有相同位置的版本号都相同，但长度不同，则较长的版本号应该更大  
        return versionPartsX.Length.CompareTo(versionPartsY.Length);
    }
}
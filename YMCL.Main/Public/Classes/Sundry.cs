#pragma warning disable CS8618
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CurseForge.APIClient.Models.Files;
using CurseForge.APIClient.Models.Mods;
using CurseForge.APIClient.Models;
using System.Collections.Generic;
using System;

namespace YMCL.Main.Public.Classes
{
    public class FolderInfo()
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class FileInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
    public class AccountInfo
    {
        public AccountType AccountType { get; set; }

        public string Name { get; set; } = "Unnamed";

        public string AddTime { get; set; } = "1970-01-01T00:00:00+08:00";

        public string? Data { get; set; }

        public string Skin { get; set; } = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAFDUlEQVR42u2a20sUURzH97G0LKMotPuWbVpslj1olJXdjCgyisowsSjzgrB0gSKyC5UF1ZNQWEEQSBQ9dHsIe+zJ/+nXfM/sb/rN4ZwZ96LOrnPgyxzP/M7Z+X7OZc96JpEISfWrFhK0YcU8knlozeJKunE4HahEqSc2nF6zSEkCgGCyb+82enyqybtCZQWAzdfVVFgBJJNJn1BWFgC49/VpwGVlD0CaxQiA5HSYEwBM5sMAdKTqygcAG9+8coHKY/XXAZhUNgDYuBSPjJL/GkzVVhAEU5tqK5XZ7cnFtHWtq/TahdSw2l0HUisr1UKIWJQBAMehDuqiDdzndsP2EZECAG1ZXaWMwOCODdXqysLf++uXUGv9MhUHIByDOijjdiSAoH3ErANQD73C7TXXuGOsFj1d4YH4OTJAEy8y9Hd0mCaeZ5z8dfp88zw1bVyiYhCLOg1ZeAqC0ybaDttHRGME1DhDeVWV26u17lRAPr2+mj7dvULfHw2q65fhQRrLXKDfIxkau3ZMCTGIRR3URR5toU38HbaPiMwUcKfBAkoun09PzrbQ2KWD1JJaqswjdeweoR93rirzyCMBCmIQizqoizZkm2H7iOgAcHrMHbbV9KijkUYv7qOn55sdc4fo250e+vUg4329/Xk6QB/6DtOws+dHDGJRB3XRBve+XARt+4hIrAF4UAzbnrY0ve07QW8uHfB+0LzqanMM7qVb+3f69LJrD90/1axiEIs6qIs21BTIToewfcSsA+Bfb2x67OoR1aPPzu2i60fSNHRwCw221Suz0O3jO+jh6V1KyCMGse9721XdN5ePutdsewxS30cwuMjtC860T5JUKpXyKbSByUn7psi5l+juDlZYGh9324GcPKbkycaN3jUSAGxb46IAYPNZzW0AzgiQ5tVnzLUpUDCAbakMQXXrOtX1UMtHn+Q9/X5L4wgl7t37r85OSrx+TYl379SCia9KXjxRpiTjIZTBFOvrV1f8ty2eY/T7XJ81FQAwmA8ASH1ob68r5PnBsxA88/xAMh6SpqW4HRnLBrkOA9Xv5wPAZjAUgOkB+SHxgBgR0qSMh0zmZRsmwDJm1gFg2PMDIC8/nAHIMls8x8GgzOsG5WiaqREgYzDvpTwjLDy8NM15LpexDEA3LepjU8Z64my+8PtDCmUyRr+fFwA2J0eAFYA0AxgSgMmYBMZTwFQnO9RNAEaHOj2DXF5UADmvAToA2ftyxZYA5BqgmZZApDkdAK4mAKo8GzPlr8G8AehzMAyA/i1girUA0HtYB2CaIkUBEHQ/cBHSvwF0AKZFS5M0ZwMQtEaEAmhtbSUoDADH9ff3++QZ4o0I957e+zYAMt6wHkhzpjkuAcgpwNcpA7AZDLsvpwiuOkBvxygA6Bsvb0HlaeKIF2EbADZpGiGzBsA0gnwQHGOhW2snRpbpPexbAB2Z1oicAMQpTnGKU5ziFKc4xSlOcYpTnOIUpzgVmgo+XC324WfJAdDO/+ceADkCpuMFiFKbApEHkOv7BfzfXt+5gpT8V7rpfYJcDz+jAsB233r6yyBsJ0mlBCDofuBJkel4vOwBFPv8fyYAFPJ+wbSf/88UANNRVy4Awo6+Ig2gkCmgA5DHWjoA+X7AlM//owLANkX0w0359od++pvX8fdMAcj3/QJ9iJsAFPQCxHSnQt8vMJ3v2wCYpkhkAOR7vG7q4aCXoMoSgG8hFAuc/grMdAD4B/kHl9da7Ne9AAAAAElFTkSuQmCC";
        public Bitmap Bitmap { get; set; }
    }
    public class ModManageEntry
    {
        public string Name { get; set; }
        public string File { get; set; }
        public TextDecorationCollection Decorations { get; set; }
    }
    public class SearchModListViewItemEntry
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public ModLinks Links { get; set; }
        public string Summary { get; set; }
        public ModStatus Status { get; set; }
        public double DownloadCount { get; set; }
        public bool IsFeatured { get; set; }
        public int PrimaryCategoryId { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? ClassId { get; set; }
        public List<ModAuthor> Authors { get; set; } = new List<ModAuthor>();
        public ModAsset Logo { get; set; }
        public List<ModAsset> Screenshots { get; set; } = new List<ModAsset>();
        public int MainFileId { get; set; }
        public List<File> LatestFiles { get; set; } = new List<File>();
        public List<FileIndex> LatestFilesIndexes { get; set; } = new List<FileIndex>();
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
        public DateTimeOffset DateReleased { get; set; }
        public bool? AllowModDistribution { get; set; }
        public int GamePopularityRank { get; set; }
        public bool IsAvailable { get; set; }
        public int ThumbsUpCount { get; set; }
        public string StringDownloadCount { get; set; }
        public string StringDateTime { get; set; }
        public ModSource ModSource { get; set; }

        public SearchModListViewItemEntry(Mod mod)
        {
            Id = mod.Id;
            GameId = mod.GameId;
            Name = mod.Name;
            Slug = mod.Slug;
            Links = mod.Links;
            Summary = mod.Summary;
            Status = mod.Status;
            DownloadCount = mod.DownloadCount;
            IsFeatured = mod.IsFeatured;
            PrimaryCategoryId = mod.PrimaryCategoryId;
            Categories = mod.Categories;
            ClassId = mod.ClassId;
            Authors = mod.Authors;
            Logo = mod.Logo;
            Screenshots = mod.Screenshots;
            MainFileId = mod.MainFileId;
            LatestFiles = mod.LatestFiles;
            LatestFilesIndexes = mod.LatestFilesIndexes;
            DateCreated = mod.DateCreated;
            DateModified = mod.DateModified;
            DateReleased = mod.DateReleased;
            AllowModDistribution = mod.AllowModDistribution;
            GamePopularityRank = mod.GamePopularityRank;
            IsAvailable = mod.IsAvailable;
            ThumbsUpCount = mod.ThumbsUpCount;
        }
    }
}

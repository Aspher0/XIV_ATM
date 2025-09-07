using System.Numerics;

namespace XIVATM.IPC.Honorific;

// From: DynamicBridge/IPC/Honorific/TitleData.cs
public class TitleData
{
    public string Title = string.Empty;
    public bool IsPrefix;
    public bool IsOriginal;
    public Vector3? Color;
    public Vector3? Glow;

    public TitleData(string title, bool isPrefix, bool isOriginal, Vector3? color, Vector3? glow)
    {
        Title = title;
        IsPrefix = isPrefix;
        IsOriginal = isOriginal;
        Color = color;
        Glow = glow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is TitleData otherTitle)
        {
            return this.Title == otherTitle.Title
                && this.IsPrefix == otherTitle.IsPrefix
                && this.IsOriginal == otherTitle.IsOriginal
                && this.Color == otherTitle.Color
                && this.Glow == otherTitle.Glow;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

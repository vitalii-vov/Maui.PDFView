namespace Maui.PDFView;

public partial class PageAppearance
{
    public bool ShadowEnabled { get; set; } = true;
    public Thickness Margin { get; set; }
    public Thickness Crop { get; set; }
}

public partial class PageAppearance
{
    public static PageAppearance Default => new()
    {
        ShadowEnabled = true, 
        Margin = new Thickness(16, 8),
        Crop = new Thickness(0),
    };
}
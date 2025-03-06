namespace Maui.PDFView;

public partial class PageAppearance
{
    public PageAppearance()
    {
    }

    private PageAppearance(bool shadowEnabled, Thickness margin)
    {
        ShadowEnabled = shadowEnabled;
        Margin = margin;
    }
    
    public bool ShadowEnabled { get; set; }
    public Thickness Margin { get; set; }
}

public partial class PageAppearance
{
    public static PageAppearance Default => new( true, new Thickness(16, 8));
}
namespace Maui.PDFView;

public partial class PageAppearance
{
    public bool ShadowEnabled { get; set; } = true;
    public Thickness Margin { get; set; } = new Thickness(16, 8);
    public Thickness Crop { get; set; } = new Thickness(0);
}
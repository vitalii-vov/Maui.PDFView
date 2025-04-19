namespace Maui.PDFView.Helpers;

public class DesiredSizeHelper
{
    private Size _lastSize = new Size(-1, -1);

    public bool UpdateSize(double width, double height)
    {
        var newSize = new Size(width, height);
        if (_lastSize == newSize) return false;
        _lastSize = newSize;
        return true;
    }
}
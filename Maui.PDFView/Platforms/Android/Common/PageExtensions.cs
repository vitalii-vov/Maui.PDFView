using Android.Graphics;
using Android.Graphics.Pdf;

namespace Maui.PDFView.Platforms.Android.Common;

public static class PageExtensions
{
    public static Bitmap RenderTo(this PdfRenderer.Page page, Bitmap bitmap, Thickness? crop)
    {
        page.Render(
            bitmap,
            null,
            new CropMatrix(page, crop ?? Thickness.Zero).Crop(bitmap),
            PdfRenderMode.ForDisplay
        );

        return bitmap;
    }
}
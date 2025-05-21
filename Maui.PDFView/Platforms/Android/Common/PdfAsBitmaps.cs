using System.Diagnostics.CodeAnalysis;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;

namespace Maui.PDFView.Platforms.Android.Common;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public readonly struct PdfAsBitmaps(string? fileName, ScreenHelper screen, Thickness? crop)
{
    public List<Bitmap> ToList()
    {
        if (fileName == null)
        {
            return new List<Bitmap>();
        }
        
        using var renderer = new PdfRenderer(
            ParcelFileDescriptor.Open(new Java.IO.File(fileName), ParcelFileMode.ReadOnly)
        );
        var pages = new List<Bitmap>();
        for (int i = 0; i < renderer.PageCount; i++)
        {
            var page = renderer.OpenPage(i);
            pages.Add(
                page.RenderTo(screen.PageBitmap(page), crop) // Apply the crop to the bitmap
            );
            page.Close();
        }

        return pages;
    }
}
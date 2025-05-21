using Android.Graphics;
using Android.Graphics.Pdf;
using Matrix = Android.Graphics.Matrix;

namespace Maui.PDFView.Platforms.Android.Common;

public readonly struct CropMatrix(PdfRenderer.Page page, Thickness bounds)
{
    public Matrix? Crop(Bitmap bitmap)
    {
        if (bounds.IsEmpty)
        {
            return null;
        }
            
        var cropLeft = (int) bounds.Left;
        int cropTop = (int) bounds.Top;
        int cropRight = page.Width - (int) bounds.Right;
        int cropBottom = page.Height - (int) bounds.Bottom;

        // Scale the cut area to the entire bitmap
        float scaleX = (float)bitmap.Width / (cropRight - cropLeft);
        float scaleY = (float)bitmap.Height / (cropBottom - cropTop);

        // Create a matrix for shifting and scaling
        Matrix matrix = new Matrix();
        matrix.SetScale(scaleX, scaleY);

        // Shift the rendering area so that only the necessary part of the PDF is drawn
        matrix.PostTranslate(-cropLeft * scaleX, -cropTop * scaleY);

        return matrix;
    }
}
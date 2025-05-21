using Android.Content;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Maui.PDFView.Platforms.Android
{
    public class ScreenHelper(Context context, bool isVertical)
    {
        private int _widthPixels;
        private int _heightPixels;
        public float Density { get; private set; }
        
        public ScreenHelper Invalidate()
        {
            IWindowManager? windowManager = context
                .GetSystemService(Context.WindowService)
                .JavaCast<IWindowManager>();
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var bounds = windowManager.CurrentWindowMetrics.Bounds;
                _widthPixels = bounds.Width();
                _heightPixels = bounds.Height();
                
                var displayMetrics = context.Resources?.DisplayMetrics;
                Density = displayMetrics.Density;
                return this;
            }

            var metrics = new DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);
            _widthPixels = metrics.WidthPixels;
            _heightPixels = metrics.HeightPixels;
            Density = metrics.Density;
            return this;
        }

        public Bitmap PageBitmap(PdfRenderer.Page page)
        {
            var widthAndHeight = ImageWidthAndHeight(page);
            //  If you need to apply a color to the page
            //bitmap.EraseColor(Color.White);
            return Bitmap.CreateBitmap(
                widthAndHeight.Width,
                widthAndHeight.Height,
                Bitmap.Config.Argb8888
            );
        }

        private (int Width, int Height) ImageWidthAndHeight(PdfRenderer.Page page)
        {
            int width;
            int height;
            float ratio;

            if (isVertical)
            {
                width = _widthPixels;
                ratio = (float)page.Height / page.Width;
                height = (int)(width * ratio);
            }
            else
            {
                height = _heightPixels;
                ratio = (float)page.Width / page.Height;
                width = (int)(height * ratio);
            }

            return (width, height);
        }
    }
}

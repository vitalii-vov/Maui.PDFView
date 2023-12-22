using Android.Graphics.Pdf;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Maui.PDFView.Platforms.Android
{
    internal class ScreenHelper
    {
        private int _widthPixels;
        private int _heightPixels;

        public void Invalidate()
        {
            IWindowManager windowManager = global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.WindowService).JavaCast<IWindowManager>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                _widthPixels = windowManager.CurrentWindowMetrics.Bounds.Width();
                _heightPixels = windowManager.CurrentWindowMetrics.Bounds.Height();
                return;
            }

            var metrics = new DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);
            _widthPixels = metrics.WidthPixels;
            _heightPixels = metrics.HeightPixels;
        }

        public (int Width, int Height) GetImageWidthAndHeight(bool isVertival, PdfRenderer.Page page)
        {
            int width;
            int height;
            float ratio;

            if (isVertival)
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

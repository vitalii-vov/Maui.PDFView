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
        private float _density;

        public void Invalidate()
        {
            IWindowManager windowManager = global::Android.App.Application.Context.GetSystemService(global::Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var bounds = windowManager.CurrentWindowMetrics.Bounds;
                _widthPixels = bounds.Width();
                _heightPixels = bounds.Height();
                
                var displayMetrics = global::Android.App.Application.Context.Resources.DisplayMetrics;
                _density = displayMetrics.Density;
                return;
            }

            var metrics = new DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);
            _widthPixels = metrics.WidthPixels;
            _heightPixels = metrics.HeightPixels;
            _density = metrics.Density;
        }
        
        public float Density => _density;

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

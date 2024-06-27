using Android.Graphics.Pdf;
using Android.Graphics;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using Maui.PDFView.Platforms.Android.Common;
using Microsoft.Maui.Handlers;
using Android.Widget;
using Android.Views;

namespace Maui.PDFView.Platforms.Android
{
    internal class PdfViewHandler : ViewHandler<IPdfView, FrameLayout>
    {
        private readonly static PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
            [nameof(IPdfView.MaxZoom)] = MapMaxZoom
        };

        private readonly ScreenHelper _screenHelper = new();
        private ZoomableRecyclerView _recycleView;
        private string _fileName;

        public PdfViewHandler() : base(PropertyMapper, null)
        {
        }

        static void MapUri(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._fileName = pdfView.Uri;
            handler.RenderPages();
        }

        static void MapIsHorizontal(PdfViewHandler handler, IPdfView pdfView)
        {
            var layoutManager = handler._recycleView.GetLayoutManager() as LinearLayoutManager;
            layoutManager.Orientation = pdfView.IsHorizontal
                ? LinearLayoutManager.Horizontal
                : LinearLayoutManager.Vertical;
        }

        static void MapMaxZoom(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._recycleView.MaxZoom = pdfView.MaxZoom;
        }

        protected override FrameLayout CreatePlatformView()
        {
            var layout = new FrameLayout(Context)
            {
                LayoutParameters = new(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            _recycleView = new ZoomableRecyclerView(Context)
            {
                LayoutParameters = new(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            var layoutManager = new ZoomableLinearLayoutManager(_recycleView.Context, LinearLayoutManager.Vertical, false);
            _recycleView.SetLayoutManager(layoutManager);

            layout.AddView(_recycleView);
            return layout;
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            RenderPages();
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        void RenderPages()
        {
            if (_fileName == null)
                return;

            var isVertival = !VirtualView.IsHorizontal;

            var renderer = new PdfRenderer(ParcelFileDescriptor.Open(new Java.IO.File(_fileName), ParcelFileMode.ReadOnly));

            var pages = new List<Bitmap>();

            _screenHelper.Invalidate();

            // render all pages
            var pageCount = renderer.PageCount;
            for (int i = 0; i < pageCount; i++)
            {
                var page = renderer.OpenPage(i);

                // create bitmap at appropriate size
                var widthAndHeight = _screenHelper.GetImageWidthAndHeight(isVertival, page);
                var bitmap = Bitmap.CreateBitmap(widthAndHeight.Width, widthAndHeight.Height, Bitmap.Config.Argb8888);

                //  If you need to apply a color to the page
                //bitmap.EraseColor(Color.White);

                // render PDF page to bitmap
                page.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

                // add bitmap to list
                pages.Add(bitmap);

                // close the page
                page.Close();
            }

            // close the renderer
            renderer.Close();

            // Create an adapter for the RecyclerView, and pass it the
            // data set (the bitmap list) to manage:
            var adapter = new PdfBitmapAdapter(pages);

            // Plug the adapter into the RecyclerView:
            _recycleView.SetAdapter(adapter);
        }
    }
}

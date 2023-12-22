using Android.Graphics.Pdf;
using Android.Graphics;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using Maui.PDFView.Platforms.Android.Common;
using Microsoft.Maui.Handlers;

namespace Maui.PDFView.Platforms.Android
{
    internal class PdfViewHandler : ViewHandler<IPdfView, ZoomableRecyclerView>
    {
        private readonly static PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
        };

        private readonly ScreenHelper _screenHelper = new ScreenHelper();
        private string _uri;

        public PdfViewHandler() : base(PropertyMapper, null)
        {
        }

        static void MapUri(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._uri = pdfView.Uri;
            handler.RenderPages();
        }

        static void MapIsHorizontal(PdfViewHandler handler, IPdfView pdfView)
        {
            var layoutManager = handler.PlatformView.GetLayoutManager() as LinearLayoutManager;
            layoutManager.Orientation = pdfView.IsHorizontal
                ? LinearLayoutManager.Horizontal
                : LinearLayoutManager.Vertical;
        }

        protected override ZoomableRecyclerView CreatePlatformView()
        {
            var view = new ZoomableRecyclerView(Context);
            var layoutManager = new ZoomableLinearLayoutManager(view.Context, LinearLayoutManager.Vertical, false);
            view.SetLayoutManager(layoutManager);

            return view;
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            RenderPages();
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        void RenderPages()
        {
            var fileName = _uri;
            if (fileName == null)
                return;

            var isVertival = !VirtualView.IsHorizontal;

            MemoryStream memoryStream = new();
            File.OpenRead(fileName).CopyTo(memoryStream);

            var renderer = new PdfRenderer(ParcelFileDescriptor.Open(new Java.IO.File(fileName), ParcelFileMode.ReadOnly));

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
            PlatformView.SetAdapter(adapter);
        }
    }
}

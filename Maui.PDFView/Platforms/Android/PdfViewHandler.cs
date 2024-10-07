using Android.Graphics.Pdf;
using Android.Graphics;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using Maui.PDFView.Platforms.Android.Common;
using Microsoft.Maui.Handlers;
using Android.Widget;
using Android.Views;
using Maui.PDFView.Events;

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

            _recycleView.AddOnScrollListener(new PdfScrollListener(this));

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

        private class PdfScrollListener : RecyclerView.OnScrollListener
        {
            private readonly PdfViewHandler _handler;

            public PdfScrollListener(PdfViewHandler handler)
            {
                _handler = handler;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);

                var layoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();
                int firstVisibleItemPosition = layoutManager.FindFirstVisibleItemPosition();
                int lastVisibleItemPosition = layoutManager.FindLastVisibleItemPosition();

                // Ensure valid visible positions
                if (firstVisibleItemPosition == RecyclerView.NoPosition || lastVisibleItemPosition == RecyclerView.NoPosition)
                    return;

                // Determine the index of the page that is most visible
                int currentPage = firstVisibleItemPosition;
                float maxVisibleHeight = 0f;

                for (int i = firstVisibleItemPosition; i <= lastVisibleItemPosition; i++)
                {
                    var visibleChild = layoutManager.FindViewByPosition(i);
                    if (visibleChild != null)
                    {
                        float visibleHeight = Math.Min(visibleChild.Bottom, recyclerView.Height) - Math.Max(visibleChild.Top, 0);
                        if (visibleHeight > maxVisibleHeight)
                        {
                            maxVisibleHeight = visibleHeight;
                            currentPage = i; // Update current page
                        }
                    }
                }

                // Execute the command if available
                _handler.VirtualView.PageChangedCommand?.Execute(new PageChangedEventArgs(currentPage + 1, layoutManager.ItemCount));
            }
        }
    }
}

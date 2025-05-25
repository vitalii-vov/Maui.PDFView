using Android.Graphics.Pdf;
using Android.Graphics;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using Maui.PDFView.Platforms.Android.Common;
using Microsoft.Maui.Handlers;
using Android.Widget;
using Android.Views;
using Maui.PDFView.Events;
using Maui.PDFView.Helpers;

namespace Maui.PDFView.Platforms.Android
{
    public class PdfViewHandler : ViewHandler<IPdfView, FrameLayout>
    {
        public static readonly PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
            [nameof(IPdfView.MaxZoom)] = MapMaxZoom,
            [nameof(IPdfView.PageAppearance)] = MapPageAppearance,
            [nameof(IPdfView.PageIndex)] = MapPageIndex,
        };

        private readonly ScreenHelper _screenHelper = new();
        private ZoomableRecyclerView _recycleView;
        private string _fileName;
        private readonly DesiredSizeHelper _sizeHelper = new();
        
        private PageAppearance? _pageAppearance;
        
        private bool _isScrolling;
        private bool _isPageIndexLocked;

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
        
        static void MapPageAppearance(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._pageAppearance = pdfView.PageAppearance ?? new PageAppearance();
        }

        static void MapPageIndex(PdfViewHandler handler, IPdfView pdfView)
        {
            handler.GotoPage(pdfView.PageIndex);
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
            if (_sizeHelper.UpdateSize(widthConstraint, heightConstraint))
            {
                //  Change the behavior of the component if the size of the selected area has been changed
                //  (for example, when the screen is flipped or the screen is split)
                RenderPages();
            }
            
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
                
                // Crop page
                var matrix = GetCropMatrix(page, bitmap, _pageAppearance?.Crop ?? Thickness.Zero);

                // render PDF page to bitmap
                page.Render(bitmap, null, matrix, PdfRenderMode.ForDisplay);

                // add bitmap to list
                pages.Add(bitmap);

                // close the page
                page.Close();
            }

            // close the renderer
            renderer.Close();

            // Create an adapter for the RecyclerView, and pass it the
            // data set (the bitmap list) to manage:
            var adapter = new PdfBitmapAdapter(pages, _pageAppearance);

            // Plug the adapter into the RecyclerView:
            _recycleView.SetAdapter(adapter);
        }

        private Matrix? GetCropMatrix(PdfRenderer.Page page, Bitmap bitmap, Thickness bounds)
        {
            if (bounds.IsEmpty)
                return null;
            
            int pageWidth = page.Width;
            int pageHeight = page.Height;
                
            var cropLeft = (int) bounds.Left;
            int cropTop = (int) bounds.Top;
            int cropRight = pageWidth - (int) bounds.Right;
            int cropBottom = pageHeight - (int) bounds.Bottom;

            // Create a matrix for shifting and scaling
            Matrix matrix = new Matrix();

            // Scale the cut area to the entire bitmap
            float scaleX = (float)bitmap.Width / (cropRight - cropLeft);
            float scaleY = (float)bitmap.Height / (cropBottom - cropTop);

            matrix.SetScale(scaleX, scaleY);

            // Shift the rendering area so that only the necessary part of the PDF is drawn
            matrix.PostTranslate(-cropLeft * scaleX, -cropTop * scaleY);

            return matrix;
        }

        private void GotoPage(uint pageIndex)
        {
            if (_isScrolling)
                return;

            var layoutManager = (LinearLayoutManager)_recycleView.GetLayoutManager()!;

            if (pageIndex >= layoutManager.ItemCount)
                return;

            _isPageIndexLocked = true;
            layoutManager.ScrollToPositionWithOffset((int)pageIndex,1);
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
                float maxVisibleSize = 0f;

                for (int i = firstVisibleItemPosition; i <= lastVisibleItemPosition; i++)
                {
                    var visibleChild = layoutManager.FindViewByPosition(i);
                    if (visibleChild != null)
                    {
                        // Check if the layout is horizontal or vertical
                        float visibleSize = layoutManager.Orientation == LinearLayoutManager.Horizontal
                            ? Math.Min(visibleChild.Right, recyclerView.Width) - Math.Max(visibleChild.Left, 0) // Width
                            : Math.Min(visibleChild.Bottom, recyclerView.Height) - Math.Max(visibleChild.Top, 0); // Height

                        // There may be a situation where the height (or width in case of horizontal orientation) of the pages are different. 
                        // In this case, the page that is fully displayed is considered visible.
                        if (visibleSize == (layoutManager.Orientation == LinearLayoutManager.Horizontal ? visibleChild.Width : visibleChild.Height))
                        {
                            currentPage = i;
                            break;
                        }

                        if (visibleSize > maxVisibleSize)
                        {
                            maxVisibleSize = visibleSize;
                            currentPage = i;
                        }
                    }
                }

                var newPageIndex = (uint)currentPage;
                if (_handler._isPageIndexLocked)
                {
                    _handler._isPageIndexLocked = false;
                }
                else if (_handler.VirtualView.PageIndex != newPageIndex)
                {
                    _handler._isScrolling = true;
                    _handler.VirtualView.PageIndex = newPageIndex;
                    _handler._isScrolling = false;
                }

                if (_handler.VirtualView.PageChangedCommand?.CanExecute(null) == true)
                    // Execute the command if available
                    _handler.VirtualView.PageChangedCommand?.Execute(new PageChangedEventArgs(currentPage + 1, layoutManager.ItemCount));
            }
        }
    }
}

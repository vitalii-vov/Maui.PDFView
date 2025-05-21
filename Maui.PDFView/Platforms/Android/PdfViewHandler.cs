using System.Diagnostics.CodeAnalysis;
using AndroidX.RecyclerView.Widget;
using Maui.PDFView.Platforms.Android.Common;
using Microsoft.Maui.Handlers;
using Android.Widget;
using Android.Views;
using Maui.PDFView.Events;
using Maui.PDFView.Helpers;

namespace Maui.PDFView.Platforms.Android
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
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

        private readonly DesiredSizeHelper _sizeHelper = new();
        private ZoomableRecyclerView _recycleView;
        private string? _fileName;
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
            if (handler._recycleView.GetLayoutManager() is LinearLayoutManager layoutManager)
            {
                layoutManager.Orientation = pdfView.IsHorizontal
                    ? LinearLayoutManager.Horizontal
                    : LinearLayoutManager.Vertical;
            }
        }

        static void MapMaxZoom(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._recycleView.MaxZoom = pdfView.MaxZoom;
        }
        
        static void MapPageAppearance(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._pageAppearance = pdfView.PageAppearance;
        }

        static void MapPageIndex(PdfViewHandler handler, IPdfView pdfView)
        {
            handler.GotoPage(pdfView.PageIndex);
        }

        protected override FrameLayout CreatePlatformView()
        {
            _recycleView = new ZoomableRecyclerView(Context)
            {
                LayoutParameters = new(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            _recycleView.SetLayoutManager(
                new ZoomableLinearLayoutManager(
                    Context,
                    LinearLayoutManager.Vertical,
                    false
                )
            );

            _recycleView.AddOnScrollListener(new PdfScrollListener(this));
            var layout = new FrameLayout(Context)
            {
                LayoutParameters = new(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            layout.AddView(_recycleView);
            return layout;
        }

        /// <summary>
        /// Changes the behavior of the component if the size of the selected area has been changed.
        /// For example, when the screen is flipped or the screen is split.
        /// The method creates an adapter for the RecyclerView, and pass it the
        /// data set (the bitmap list) to manage.
        /// </summary>
        /// <param name="widthConstraint"></param>
        /// <param name="heightConstraint"></param>
        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (_sizeHelper.UpdateSize(widthConstraint, heightConstraint))
            {
                RenderPages();
            }
            
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        void RenderPages()
        {
            var page = _pageAppearance ?? new PageAppearance();
            _recycleView.SetAdapter(
                new PdfBitmapAdapter(
                    new PdfAsBitmaps(
                        _fileName,
                        new ScreenHelper(
                            global::Android.App.Application.Context,
                            !VirtualView.IsHorizontal
                        ).Invalidate(),
                        page.Crop
                    ),
                    page
                )
            );
        }

        private void GotoPage(uint pageIndex)
        {
            if (_isScrolling)
                return;

            var layoutManager = (LinearLayoutManager)_recycleView.GetLayoutManager()!;
            if (pageIndex >= layoutManager.ItemCount)
            {
                return;
            }

            _isPageIndexLocked = true;
            layoutManager.ScrollToPositionWithOffset((int)pageIndex,1);
        }

        public class PdfScrollListener(PdfViewHandler handler) : RecyclerView.OnScrollListener
        {
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
                if (handler._isPageIndexLocked)
                {
                    handler._isPageIndexLocked = false;
                }
                else if (handler.VirtualView.PageIndex != newPageIndex)
                {
                    handler._isScrolling = true;
                    handler.VirtualView.PageIndex = newPageIndex;
                    handler._isScrolling = false;
                }

                if (handler.VirtualView.PageChangedCommand?.CanExecute(null) == true)
                    // Execute the command if available
                    handler.VirtualView.PageChangedCommand?.Execute(new PageChangedEventArgs(currentPage + 1, layoutManager.ItemCount));
            }
        }
    }
}

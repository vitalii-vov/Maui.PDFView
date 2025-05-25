using Maui.PDFView.Events;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Numerics;
using System.Reflection.Metadata;
using Windows.Data.Pdf;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Maui.PDFView.Platforms.Windows
{
    public class PdfViewHandler : ViewHandler<IPdfView, ScrollViewer>
    {
        public static readonly PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
            [nameof(IPdfView.MaxZoom)] = MapMaxZoom,
            [nameof(IPdfView.PageAppearance)] = MapPageAppearance,
            [nameof(IPdfView.PageIndex)] = MapPageIndex,
        };

        private ScrollViewer _scrollViewer;
        private StackPanel _stack;
        private string _fileName;

        private PageAppearance _pageAppearance = new PageAppearance();
        
        private bool _isScrolling;
        private bool _isPageIndexLocked;

        public PdfViewHandler() : base(PropertyMapper, null)
        {
        }

        static async void MapUri(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._fileName = pdfView.Uri;
            await handler.RenderPages();
        }

        static void MapIsHorizontal(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._stack.Orientation = pdfView.IsHorizontal
                ? Orientation.Horizontal 
                : Orientation.Vertical;
        }

        static void MapMaxZoom(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._scrollViewer.MaxZoomFactor = pdfView.MaxZoom;
        }

        static void MapPageAppearance(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._pageAppearance = pdfView.PageAppearance ?? new PageAppearance();
        }

        static void MapPageIndex(PdfViewHandler handler, IPdfView pdfView)
        {
            handler.GotoPage(pdfView.PageIndex);
        }


        protected override ScrollViewer CreatePlatformView()
        {
            _scrollViewer = new ScrollViewer
            {
                ZoomMode = ZoomMode.Enabled,
                MinZoomFactor = 1,
                MaxZoomFactor = 4,
                HorizontalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible,
            };

            // Attach scroll event to track page changes
            _scrollViewer.ViewChanged += OnScrollViewerViewChanged;

            _stack = new StackPanel { Orientation = Orientation.Vertical };
            _scrollViewer.Content = _stack;

            return _scrollViewer;
        }

        async Task RenderPages()
        {
            _stack.Children.Clear();
            _scrollViewer.ZoomToFactor(1);

            if (_fileName == null)
                return;

            var storageFile = await StorageFile.GetFileFromPathAsync(_fileName);
            PdfDocument pdfDoc = await PdfDocument.LoadFromFileAsync(storageFile);

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                var page = pdfDoc.GetPage(i);
                var bundle = _pageAppearance?.Crop ?? new Microsoft.Maui.Thickness();

                using (InMemoryRandomAccessStream stream = new())
                {
                    double logicalDpi = DeviceDisplay.MainDisplayInfo.Density * 96;
                    var k = logicalDpi / 72.0;
                    var renderOptions = new PdfPageRenderOptions
                    {
                        SourceRect = new(
                            bundle.Left * k,
                            bundle.Top * k,
                            page.Size.Width - (bundle.Right * k) - (bundle.Left * k),
                            page.Size.Height - (bundle.Bottom * k) - (bundle.Top * k)
                        )
                    };

                    await page.RenderToStreamAsync(stream, renderOptions);

                    BitmapImage bitmap = new();
                    await bitmap.SetSourceAsync(stream);

                    _stack.Children.Add(MakePage(bitmap, _pageAppearance));
                }
            }

            //  Reset page index
            VirtualView.PageIndex = 0;
        }

        private UIElement MakePage(BitmapImage image, PageAppearance pageAppearance)
        {
            var appearance = pageAppearance ?? new PageAppearance();
            var am = appearance.Margin;

            var border = new Microsoft.UI.Xaml.Controls.Grid
            {
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4),
                Margin = new Microsoft.UI.Xaml.Thickness(am.Left, am.Top, am.Right, am.Bottom),
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };
            border.Children.Add(new Microsoft.UI.Xaml.Controls.Image { Source = image });

            if (appearance.ShadowEnabled)
                AddShadow(border, 32);

            return border;
        }

        public static void AddShadow(Microsoft.UI.Xaml.Controls.Grid target, int ZDepth)
        {
            //  create border for shadow (receiver). make sure there are space to show shadow so set margin negative.
            Microsoft.UI.Xaml.Controls.Border shadowReceiver = new() { Margin = new Microsoft.UI.Xaml.Thickness(-ZDepth) };
            //  add receiver to parent grid
            target.Children.Insert(0, shadowReceiver);
            //  create new theme shadow
            ThemeShadow sharedShadow = new();
            //  connect shadow to framework element
            target.Shadow = sharedShadow;
            //  connect receiver to theme shadow
            sharedShadow.Receivers.Add(shadowReceiver);
            //  set shadow depth
            target.Translation += new Vector3(0, 0, ZDepth);
        }

       

        private void GotoPage(uint pageIndex)
        {
            if (_isScrolling)
                return;

            var layout = (StackPanel)_scrollViewer.Content;
            if (pageIndex >= layout.Children.Count)
                return;

            var child = layout.Children[(int)pageIndex] as FrameworkElement;
            if (child != null)
            {
                var transform = child.TransformToVisual(_scrollViewer);
                var position = transform.TransformBounds(new(0, 0, child.ActualWidth, child.ActualHeight));

                _isPageIndexLocked = true;
                if (_stack.Orientation == Orientation.Vertical)
                    _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ZoomFactor*child.ActualOffset.Y);
                else
                    _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.ZoomFactor * child.ActualOffset.X);
            }
        }

        private void OnScrollViewerViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            // Get the index of the currently visible page
            var layout = (StackPanel)_scrollViewer.Content;
            if (layout.Children.Count == 0)
                return;

            int currentPage = -1;
            double maxVisibleSize = 0.0;

            for (int i = 0; i < layout.Children.Count; i++)
            {
                var child = layout.Children[i] as FrameworkElement;
                if (child != null)
                {
                    var transform = child.TransformToVisual(_scrollViewer);
                    var position = transform.TransformBounds(new(0, 0, child.ActualWidth, child.ActualHeight));

                    // Check visibility based on the scrolling direction
                    bool isVisible = VirtualView.IsHorizontal
                        ? (position.Right >= 0 && position.Left <= _scrollViewer.ViewportWidth)
                        : (position.Bottom >= 0 && position.Top <= _scrollViewer.ViewportHeight);

                    if (isVisible)
                    {
                        // Calculate visible size based on the layout orientation
                        double visibleSize = VirtualView.IsHorizontal
                            ? Math.Min(position.Right, _scrollViewer.ViewportWidth) - Math.Max(position.Left, 0)
                            : Math.Min(position.Bottom, _scrollViewer.ViewportHeight) - Math.Max(position.Top, 0);

                        if (visibleSize > maxVisibleSize)
                        {
                            maxVisibleSize = visibleSize;
                            currentPage = i;
                        }
                    }
                }
            
            }

            var newPageIndex = (uint)currentPage;
            if (_isPageIndexLocked)
            {
                _isPageIndexLocked = false;
            }
            else if (VirtualView.PageIndex != newPageIndex)
            {
                _isScrolling = true;
                VirtualView.PageIndex = newPageIndex;
                _isScrolling = false;
            }

            if (VirtualView.PageChangedCommand?.CanExecute(null) == true)
                VirtualView.PageChangedCommand.Execute(new PageChangedEventArgs(currentPage + 1, layout.Children.Count));
        }
    }
}

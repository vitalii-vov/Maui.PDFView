using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Numerics;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Maui.PDFView.Platforms.Windows
{
    internal class PdfViewHandler : ViewHandler<IPdfView, ScrollViewer>
    {
        private readonly static PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
            [nameof(IPdfView.MaxZoom)] = MapMaxZoom
        };

        private ScrollViewer _scrollViewer;
        private StackPanel _stack;
        private string _fileName;

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

                BitmapImage bitmap = new();
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await bitmap.SetSourceAsync(stream);
                }

                _stack.Children.Add(MakePage(bitmap));
            }
        }

        private UIElement MakePage(BitmapImage image)
        {
            var border = new Microsoft.UI.Xaml.Controls.Grid
            {
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4),
                Margin = new Microsoft.UI.Xaml.Thickness(10),
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };
            border.Children.Add(new Microsoft.UI.Xaml.Controls.Image { Source = image });

            AddShadow(border, 32);

            return border;
        }

        public static void AddShadow(Microsoft.UI.Xaml.Controls.Grid target, int ZDepth)
        {
            //  create border for shadow (reciever). make sure there are space to show shadow so set margin negative.
            Microsoft.UI.Xaml.Controls.Border shadowReciever = new() { Margin = new Microsoft.UI.Xaml.Thickness(-ZDepth) };
            //  add reciever to parent grid
            target.Children.Insert(0, shadowReciever);
            //  create new theme shadow
            ThemeShadow sharedShadow = new();
            //  connect shadow to framework element
            target.Shadow = sharedShadow;
            //  connect reciever to theme shadow
            sharedShadow.Receivers.Add(shadowReciever);
            //  set shadow depth
            target.Translation += new Vector3(0, 0, ZDepth);
        }
    }
}

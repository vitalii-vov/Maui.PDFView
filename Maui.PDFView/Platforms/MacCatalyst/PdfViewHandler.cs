using Foundation;
using Maui.PDFView.Events;
using Microsoft.Maui.Handlers;
using PdfKit;
using UIKit;

namespace Maui.PDFView.Platforms.MacCatalyst
{
    internal class PdfViewHandler : ViewHandler<IPdfView, PdfKit.PdfView>
    {
        private readonly static PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
            [nameof(IPdfView.MaxZoom)] = MapMaxZoom,
            [nameof(IPdfView.PageAppearance)] = MapPageAppearance,
            [nameof(IPdfView.PageNumber)] = MapPageNumber,
        };

        private string _fileName;
        private PageAppearance _appearance = new();

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
            handler.PlatformView.DisplayDirection = pdfView.IsHorizontal
                                        ? PdfDisplayDirection.Horizontal
                                        : PdfDisplayDirection.Vertical;
        }

        static void MapMaxZoom(PdfViewHandler handler, IPdfView pdfView)
        {
            // reset MaxScaleFactor inside RenderPages
            handler.RenderPages();
        }

        static void MapPageAppearance(PdfViewHandler handler, IPdfView pdfView)
        {
            var appearance = pdfView.PageAppearance ?? new PageAppearance();
            handler._appearance = appearance;

            SetPageAppearance(handler, appearance);
        }

        static void MapPageNumber(PdfViewHandler handler, IPdfView pdfView)
        {
            handler.GotoPage(pdfView.PageNumber);
        }

        private static void SetPageAppearance(PdfViewHandler handler, PageAppearance appearance)
        {
            //  set shadow
            if (OperatingSystem.IsIOSVersionAtLeast(12, 0))
                handler.PlatformView.PageShadowsEnabled = appearance.ShadowEnabled;
            
            //  set margin
            handler.PlatformView.PageBreakMargins = new UIEdgeInsets(
                (nfloat)appearance.Margin.Top,
                (nfloat)appearance.Margin.Left, 
                (nfloat)appearance.Margin.Bottom,
                (nfloat)appearance.Margin.Right);
        }

        protected override PdfKit.PdfView CreatePlatformView()
        {
            var pdfView = new PdfKit.PdfView();

            // Subscribe to notification of page changes
            NSNotificationCenter.DefaultCenter.AddObserver(
                PdfKit.PdfView.PageChangedNotification, 
                PageChangedNotificationHandler, 
                pdfView);

            return pdfView;
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            RenderPages();
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }
        
        protected override void DisconnectHandler(PdfKit.PdfView platformView)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(PlatformView);
            base.DisconnectHandler(platformView);
        }

        private void RenderPages()
        {
            if (_fileName == null)
                return;

            var doc = new PdfDocument(NSData.FromFile(_fileName));
            CropPages(doc, _appearance.Crop);
            PlatformView.Document = doc;
            
            PlatformView.AutosizesSubviews = true;
            PlatformView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleBottomMargin;
            PlatformView.DisplayMode = PdfDisplayMode.SinglePageContinuous;
            PlatformView.DisplaysPageBreaks = true;

            PlatformView.MaxScaleFactor = VirtualView.MaxZoom;
            //PlatformView.MinScaleFactor = PlatformView.ScaleFactorForSizeToFit;
            PlatformView.MinScaleFactor = (nfloat)(UIScreen.MainScreen.Bounds.Height * 0.00075);

            PlatformView.AutoScales = true;
        }

        private void GotoPage(uint pageNumber)
        {
            var document = PlatformView.Document;
            if (document is null)
                return;

            if (pageNumber == 0 || document.PageCount <= pageNumber - 1)
                return;

            var newPage = document.GetPage((nint)pageNumber - 1);

            if (newPage is null)
                return;

            PlatformView.GoToPage(newPage);
        }

        private void PageChangedNotificationHandler(NSNotification notification)
        {
            var currentPage = PlatformView.CurrentPage;
            if (currentPage is null)
                return;

            var document = PlatformView.Document;
            if (document is null)
                return;

            var newPage = (uint)document.GetPageIndex(currentPage) + 1;
            if (VirtualView.PageNumber != newPage)
            {
                VirtualView.PageNumber = newPage;
                
                if (!(VirtualView.PageChangedCommand?.CanExecute(null) ?? false))
                    return;

                VirtualView.PageChangedCommand.Execute(new PageChangedEventArgs((int)newPage, (int)document.PageCount));
            }
        }

        
        
        private void CropPages(PdfKit.PdfDocument pdfdoc, Thickness cropBounds)
        {
            if (cropBounds.IsEmpty) 
                return;

            for (var i = 0; i < pdfdoc.PageCount; ++i)
            {
                var page = pdfdoc.GetPage(i);

                var boundW = cropBounds.Left + cropBounds.Right;
                var boundH = cropBounds.Top + cropBounds.Bottom;

                var boxType = PdfKit.PdfDisplayBox.Crop;
                var oldBounds = page.GetBoundsForBox(boxType);
                page.SetBoundsForBox(new CoreGraphics.CGRect(cropBounds.Left, cropBounds.Top, oldBounds.Width - boundW, oldBounds.Height - boundH), boxType);
            }
        }
    }
}
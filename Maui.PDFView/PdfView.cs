using System.Windows.Input;

namespace Maui.PDFView
{
    public class PdfView : View, IPdfView
    {
        public static readonly BindableProperty UriProperty = BindableProperty.Create(
                propertyName: nameof(Uri),
                returnType: typeof(string),
                declaringType: typeof(PdfView),
                defaultValue: default(string));

        public static readonly BindableProperty IsHorizontalProperty = BindableProperty.Create(
                propertyName: nameof(IsHorizontal),
                returnType: typeof(bool),
                declaringType: typeof(PdfView),
                defaultValue: false);

        public static readonly BindableProperty MaxZoomProperty = BindableProperty.Create(
                propertyName: nameof(MaxZoom),
                returnType: typeof(float),
                declaringType: typeof(PdfView),
                defaultValue: 4f,
                propertyChanged: OnMaxZoomPropertyChanged);
        
        public static readonly BindableProperty PageAppearanceProperty = BindableProperty.Create(
                propertyName: nameof(PageAppearance),
                returnType: typeof(PageAppearance), 
                declaringType: typeof(PdfView),
                defaultValue: null);

        public static readonly BindableProperty PageChangedCommandProperty = BindableProperty.Create(
                propertyName: nameof(PageChangedCommand),
                returnType: typeof(ICommand),
                declaringType: typeof(PdfView),
                defaultValue: default(ICommand));

        public static readonly BindableProperty PageIndexProperty = BindableProperty.Create(
                propertyName: nameof(PageIndex),
                returnType: typeof(uint),
                declaringType: typeof(PdfView),
                defaultValue: (uint)0, defaultBindingMode: BindingMode.TwoWay);

        public string Uri
        {
            get => (string)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }

        public bool IsHorizontal
        {
            get => (bool)GetValue(IsHorizontalProperty);
            set => SetValue(IsHorizontalProperty, value);
        }

        public float MaxZoom
        {
            get => (float)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }
        
        public PageAppearance? PageAppearance
        {
            get => (PageAppearance?)GetValue(PageAppearanceProperty);
            set => SetValue(PageAppearanceProperty, value);
        }

        public ICommand PageChangedCommand
        {
            get => (ICommand)GetValue(PageChangedCommandProperty);
            set => SetValue(PageChangedCommandProperty, value);
        }

        public uint PageIndex
        {
            get => (uint)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }

        private static void OnMaxZoomPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((float)newValue < 1f)
                throw new ArgumentException("PdfView: MaxZoom cannot be less than 1");
        }
    }
}

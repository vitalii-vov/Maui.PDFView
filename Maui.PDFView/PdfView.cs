namespace Maui.PDFView
{
    public class PdfView : View, IPdfView
    {
        public static readonly BindableProperty UriProperty = BindableProperty.Create(propertyName: nameof(Uri),
                returnType: typeof(string),
                declaringType: typeof(PdfView),
                defaultValue: default(string));

        public static readonly BindableProperty IsHorizontalProperty = BindableProperty.Create(propertyName: nameof(IsHorizontal),
                returnType: typeof(bool),
                declaringType: typeof(PdfView),
                defaultValue: false);

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public bool IsHorizontal
        {
            get { return (bool)GetValue(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }
    }
}

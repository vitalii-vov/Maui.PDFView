using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Maui.PDFView.Platforms.Android.Common
{
    internal class PdfBitmapAdapter : RecyclerView.Adapter
    {
        public List<Bitmap> Pages { get; set; }

        public PdfBitmapAdapter(List<Bitmap> pages)
        {
            Pages = pages;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            global::Android.Views.View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.card_view, parent, false);
            return new CardViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CardViewHolder vh = (CardViewHolder)holder;
            vh.Image.SetImageBitmap(Pages[position]);
        }

        public override int ItemCount => Pages.Count;
    }
}

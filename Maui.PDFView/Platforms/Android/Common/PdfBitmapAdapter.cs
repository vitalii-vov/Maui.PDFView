using Android.Graphics;
using Android.Views;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;

namespace Maui.PDFView.Platforms.Android.Common
{
    internal class PdfBitmapAdapter(List<Bitmap> pages, PageAppearance pageAppearance) : RecyclerView.Adapter
    {
        public List<Bitmap> Pages { get; } = pages;

        public override int ItemCount => Pages.Count;

        public PdfBitmapAdapter(PdfAsBitmaps pdf, PageAppearance pageAppearance)
            : this(pdf.ToList(), pageAppearance)
        {
        }
        
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater
                .From(parent.Context)?
                .Inflate(Resource.Layout.card_view, parent, false);

            if (itemView is not CardView cardView)
            {
                return new CardViewHolder(itemView);
            }
            
            //  shadow
            cardView.Elevation = pageAppearance.ShadowEnabled ? 4 : 0;
            
            //  margin
            if (cardView.LayoutParameters is ViewGroup.MarginLayoutParams layoutParams)
            {
                layoutParams.SetMargins(
                    (int)pageAppearance.Margin.Left,
                    (int)pageAppearance.Margin.Top,
                    (int)pageAppearance.Margin.Right,
                    (int)pageAppearance.Margin.Bottom);
                cardView.LayoutParameters = layoutParams;
            }

            return new CardViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CardViewHolder vh = (CardViewHolder)holder;
            vh.Image.SetImageBitmap(Pages[position]);
        }
    }
}

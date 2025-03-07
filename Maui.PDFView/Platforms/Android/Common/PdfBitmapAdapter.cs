using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Platform;

namespace Maui.PDFView.Platforms.Android.Common
{
    internal class PdfBitmapAdapter : RecyclerView.Adapter
    {
        private PageAppearance _pageAppearance;
        
        public PdfBitmapAdapter(List<Bitmap> pages, PageAppearance pageAppearance)
        {
            Pages = pages;
            _pageAppearance = pageAppearance;
        }

        public List<Bitmap> Pages { get; }
        
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            global::Android.Views.View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.card_view, parent, false);

            if (itemView is not CardView cardView || _pageAppearance == null) 
                return new CardViewHolder(itemView);
            
            //  shadow
            cardView.Elevation = _pageAppearance.ShadowEnabled ? 4 : 0;
            
            //  margin
            if (cardView.LayoutParameters is ViewGroup.MarginLayoutParams layoutParams)
            {
                layoutParams.SetMargins(
                    (int)_pageAppearance.Margin.Left,
                    (int)_pageAppearance.Margin.Top,
                    (int)_pageAppearance.Margin.Right,
                    (int)_pageAppearance.Margin.Bottom);
                cardView.LayoutParameters = layoutParams;
            }

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

using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Maui.PDFView.Platforms.Android.Common
{
    internal class CardViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; private set; }

        public CardViewHolder(global::Android.Views.View itemView) : base(itemView)
        {
            Image = itemView.FindViewById<ImageView>(Resource.Id.imageView);
        }
    }
}

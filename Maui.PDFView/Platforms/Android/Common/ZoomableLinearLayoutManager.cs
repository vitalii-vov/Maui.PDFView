using Android.Content;
using AndroidX.RecyclerView.Widget;

namespace Maui.PDFView.Platforms.Android.Common
{
    internal class ZoomableLinearLayoutManager : LinearLayoutManager
    {
        private ZoomableRecyclerView _recyclerView;

        public ZoomableLinearLayoutManager(Context context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
        {
        }

        public override void OnAttachedToWindow(RecyclerView? view)
        {
            base.OnAttachedToWindow(view);
            if (view is ZoomableRecyclerView zrv)
                _recyclerView = zrv;
        }

        public override int ScrollVerticallyBy(int dy, RecyclerView.Recycler? recycler, RecyclerView.State? state)
        {
            var scrollAmount = _recyclerView?.CalculateScrollAmountY(dy) ?? dy;
            return base.ScrollVerticallyBy(scrollAmount, recycler, state);
        }

        public override int ScrollHorizontallyBy(int dx, RecyclerView.Recycler? recycler, RecyclerView.State? state)
        {
            var scrollAmount = _recyclerView?.CalculateScrollAmountY(dx) ?? dx;
            return base.ScrollHorizontallyBy(scrollAmount, recycler, state);
        }
    }
}

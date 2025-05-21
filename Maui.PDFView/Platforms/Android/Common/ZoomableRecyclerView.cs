using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using static Android.Views.GestureDetector;

namespace Maui.PDFView.Platforms.Android.Common
{
    internal partial class ZoomableRecyclerView : RecyclerView
    {
        private const float MinZoom = 1f;
        private const float DefaultMaxZoom = 4f;

        private readonly ScaleGestureDetector _scaleDetector;
        private readonly GestureDetectorCompat _gestureDetector;
        private float _scaleFactor = MinZoom;
        private bool _isScaling = false;

        private float _scaleFocusX = 0f;
        private float _scaleFocusY = 0f;

        private float _tranX = 0f;
        private float _tranY = 0f;
        private float _maxTranX = 0f;
        private float _maxTranY = 0f;

        public ZoomableRecyclerView(Context context, IAttributeSet? attrs = null, int defStyleAttr = 0)
            : base(context, attrs, defStyleAttr)
        {
            _scaleDetector = new ScaleGestureDetector(context, this);
            _gestureDetector = new GestureDetectorCompat(context, new GestureListener(this));
            SetLayoutManager(new ZoomableLinearLayoutManager(context, LinearLayoutManager.Vertical, false));
        }

        public bool IsZoomEnabled { get; set; } = true;
        public float MaxZoom { get; set; } = DefaultMaxZoom;

        public int CalculateScrollAmountY(int dy)
        {
            if (dy == 0)
                return 0;

            if (dy > 0)
            {
                if (_tranY > -_maxTranY)
                { // Don't allow scroll, consume translation first
                    return 0;
                }

                return (int)(dy / _scaleFactor);
            }

            //else
            if (_tranY < 0)
            { // Don't allow scroll, consume translation first
                return 0;
            }

            return (int)(dy / _scaleFactor);
        }

        public int CalculateScrollAmountX(int dx)
        {
            if (dx == 0)
                return 0;

            if (dx > 0)
            {
                if (_tranX > -_maxTranX)
                { 
                    // Don't allow scroll, consume translation first
                    return 0;
                }

                return (int)(dx / _scaleFactor);
            }

            if (_tranX < 0)
            { // Don't allow scroll, consume translation first
                return 0;
            }

            return (int)(dx / _scaleFactor);
        }

        public override bool OnTouchEvent(MotionEvent? ev)
        {
            if (!IsZoomEnabled)
                return base.OnTouchEvent(ev);

            var returnValue = _scaleDetector.OnTouchEvent(ev);
            returnValue = _gestureDetector.OnTouchEvent(ev) || returnValue;
            return base.OnTouchEvent(ev) || returnValue;
        }

        protected override void DispatchDraw(Canvas canvas)
        {
            canvas.Translate(_tranX, _tranY);
            canvas.Scale(_scaleFactor, _scaleFactor);
            base.DispatchDraw(canvas);
        }
    }

    internal partial class ZoomableRecyclerView : ScaleGestureDetector.IOnScaleGestureListener
    {
        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            _isScaling = true;
            return true;
        }

        public bool OnScale(ScaleGestureDetector detector)
        {
            var previousScaleFactor = _scaleFactor;
            _scaleFactor *= detector.ScaleFactor;
            _scaleFactor = CoerceIn(_scaleFactor, MinZoom, MaxZoom);
            _maxTranX = Width * _scaleFactor - Width;
            _maxTranY = Height * _scaleFactor - Height;
            _scaleFocusX = detector.FocusX;
            _scaleFocusY = detector.FocusY;

            _tranX += _scaleFocusX * (previousScaleFactor - _scaleFactor);
            _tranX = CoerceIn(_tranX, -_maxTranX, 0f);
            _tranY += _scaleFocusY * (previousScaleFactor - _scaleFactor);
            _tranY = CoerceIn(_tranY, -_maxTranY, 0f);

            OverScrollMode = _scaleFactor > MinZoom
                ? OverScrollMode.Never
                : OverScrollMode.IfContentScrolls;

            Invalidate();
            return true;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
            _isScaling = false;
        }

        /// <summary>
        /// Ensures that this value lies in the specified range
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static float CoerceIn(float val, float min, float max)
        {
            var returnValue = Math.Max(val, min);
            return Math.Min(returnValue, max);
        }
    }

    internal partial class ZoomableRecyclerView
    {
        private sealed class GestureListener(ZoomableRecyclerView zoomableRecycler) : SimpleOnGestureListener
        {
            public override bool OnScroll(MotionEvent? e1, MotionEvent e2, float distanceX, float distanceY)
            {
                if (zoomableRecycler is { _isScaling: false, _scaleFactor: > MinZoom })
                {
                    var newTranX = zoomableRecycler._tranX - distanceX;
                    zoomableRecycler._tranX = CoerceIn(newTranX, -zoomableRecycler._maxTranX, 0f);
                    var newTranY = zoomableRecycler._tranY - distanceY;
                    zoomableRecycler._tranY = CoerceIn(newTranY, -zoomableRecycler._maxTranY, 0f);
                    zoomableRecycler.Invalidate();
                }

                return base.OnScroll(e1, e2, distanceX, distanceY);
            }
        }
    }
}

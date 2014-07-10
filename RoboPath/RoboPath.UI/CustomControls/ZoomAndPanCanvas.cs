//// *******************************************************
//// Project: RoboPath.UI
//// File Name: ZoomAndPanCanvas.cs
//// By: Frank Perks
//// *******************************************************

//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Media;

//namespace RoboPath.UI.CustomControls
//{
//    public class ZoomAndPanCanvas : ContentControl, IScrollInfo
//    {
//        #region Internal Data Members

//        /// <summary>
//        /// Reference to the underlying _content, which is named PART_Content in the template.
//        /// </summary>
//        private FrameworkElement _content;

//        /// <summary>
//        /// The transform that is applied to the _content to scale it by 'ContentScale'.
//        /// </summary>
//        private ScaleTransform _contentScaleTransform;

//        /// <summary>
//        /// The transform that is applied to the _content to offset it by 'ContentOffsetX' and 'ContentOffsetY'.
//        /// </summary>
//        private TranslateTransform _contentOffsetTransform;

//        /// <summary>
//        /// Enable the update of the _content offset as the _content scale changes.
//        /// This enabled for zooming about a point (google-maps style zooming) and zooming to a rect.
//        /// </summary>
//        private bool _enableContentOffsetUpdateFromScale;

//        /// <summary>
//        /// Used to disable syncronization between IScrollInfo interface and ContentOffsetX/ContentOffsetY.
//        /// </summary>
//        private bool _disableScrollOffsetSync;

//        /// <summary>
//        /// Normally when _content offsets changes the _content focus is automatically updated.
//        /// This syncronization is disabled when 'disableContentFocusSync' is set to 'true'.
//        /// When we are zooming in or out we 'disableContentFocusSync' is set to 'true' because 
//        /// we are zooming in or out relative to the _content focus we don't want to update the focus.
//        /// </summary>
//        private bool _disableContentFocusSync;

//        /// <summary>
//        /// The width of the viewport in _content coordinates, clamped to the width of the _content.
//        /// </summary>
//        private double _constrainedContentViewportWidth;

//        /// <summary>
//        /// The height of the viewport in _content coordinates, clamped to the height of the _content.
//        /// </summary>
//        private double _constrainedContentViewportHeight;

//        #endregion Internal Data Members

//        #region IScrollInfo Data Members

//        //
//        // These data members are for the implementation of the IScrollInfo interface.
//        // This interface works with the ScrollViewer such that when ZoomAndPanControl is 
//        // wrapped (in XAML) with a ScrollViewer the IScrollInfo interface allows the ZoomAndPanControl to
//        // handle the the scrollbar offsets.
//        //
//        // The IScrollInfo properties and member functions are implemented in ZoomAndPanControl_IScrollInfo.cs.
//        //
//        // There is a good series of articles showing how to implement IScrollInfo starting here:
//        //     http://blogs.msdn.com/bencon/archive/2006/01/05/509991.aspx
//        //

//        /// <summary>
//        /// Set to 'true' when the vertical scrollbar is enabled.
//        /// </summary>
//        private bool _canVerticallyScroll;

//        /// <summary>
//        /// Set to 'true' when the vertical scrollbar is enabled.
//        /// </summary>
//        private bool _canHorizontallyScroll;

//        /// <summary>
//        /// Records the unscaled extent of the _content.
//        /// This is calculated during the measure and arrange.
//        /// </summary>
//        private Size _unScaledExtent = new Size(0, 0);

//        /// <summary>
//        /// Records the size of the viewport (in viewport coordinates) onto the _content.
//        /// This is calculated during the measure and arrange.
//        /// </summary>
//        private Size _viewport = new Size(0, 0);

//        /// <summary>
//        /// Reference to the ScrollViewer that is wrapped (in XAML) around the ZoomAndPanControl.
//        /// Or set to null if there is no ScrollViewer.
//        /// </summary>
//        private ScrollViewer _scrollOwner;

//        #endregion IScrollInfo Data Members

//        #region Dependency Property Definitions

//        //
//        // Definitions for dependency properties.
//        //

//        public static readonly DependencyProperty ContentScaleProperty =
//                DependencyProperty.Register("ContentScale", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(1.0, ContentScalePropertyChanged, ContentScaleCoerce));

//        public static readonly DependencyProperty MinContentScaleProperty =
//                DependencyProperty.Register("MinContentScale", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.01, MinOrMaxContentScalePropertyChanged));

//        public static readonly DependencyProperty MaxContentScaleProperty =
//                DependencyProperty.Register("MaxContentScale", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(10.0, MinOrMaxContentScalePropertyChanged));

//        public static readonly DependencyProperty ContentOffsetXProperty =
//                DependencyProperty.Register("ContentOffsetX", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0, ContentOffsetXPropertyChanged, ContentOffsetXCoerce));

//        public static readonly DependencyProperty ContentOffsetYProperty =
//                DependencyProperty.Register("ContentOffsetY", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0, ContentOffsetYPropertyChanged, ContentOffsetYCoerce));

//        public static readonly DependencyProperty AnimationDurationProperty =
//                DependencyProperty.Register("AnimationDuration", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.4));

//        public static readonly DependencyProperty ContentZoomFocusXProperty =
//                DependencyProperty.Register("ContentZoomFocusX", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0));

//        public static readonly DependencyProperty ContentZoomFocusYProperty =
//                DependencyProperty.Register("ContentZoomFocusY", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0));

//        public static readonly DependencyProperty ViewportZoomFocusXProperty =
//                DependencyProperty.Register("ViewportZoomFocusX", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0));

//        public static readonly DependencyProperty ViewportZoomFocusYProperty =
//                DependencyProperty.Register("ViewportZoomFocusY", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0));

//        public static readonly DependencyProperty ContentViewportWidthProperty =
//                DependencyProperty.Register("ContentViewportWidth", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0));

//        public static readonly DependencyProperty ContentViewportHeightProperty =
//                DependencyProperty.Register("ContentViewportHeight", typeof(double), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(0.0));

//        public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty =
//                DependencyProperty.Register("IsMouseWheelScrollingEnabled", typeof(bool), typeof(ZoomAndPanCanvas),
//                                            new FrameworkPropertyMetadata(false));

//        #endregion Dependency Property Definitions

//        #region Public Methods

//        /// <summary>
//        /// Get/set the X offset (in _content coordinates) of the view on the _content.
//        /// </summary>
//        public double ContentOffsetX
//        {
//            get
//            {
//                return (double)GetValue(ContentOffsetXProperty);
//            }
//            set
//            {
//                SetValue(ContentOffsetXProperty, value);
//            }
//        }

//        /// <summary>
//        /// Event raised when the ContentOffsetX property has changed.
//        /// </summary>
//        public event EventHandler ContentOffsetXChanged;

//        /// <summary>
//        /// Get/set the Y offset (in _content coordinates) of the view on the _content.
//        /// </summary>
//        public double ContentOffsetY
//        {
//            get
//            {
//                return (double)GetValue(ContentOffsetYProperty);
//            }
//            set
//            {
//                SetValue(ContentOffsetYProperty, value);
//            }
//        }

//        /// <summary>
//        /// Event raised when the ContentOffsetY property has changed.
//        /// </summary>
//        public event EventHandler ContentOffsetYChanged;

//        /// <summary>
//        /// Get/set the current scale (or zoom factor) of the _content.
//        /// </summary>
//        public double ContentScale
//        {
//            get
//            {
//                return (double)GetValue(ContentScaleProperty);
//            }
//            set
//            {
//                SetValue(ContentScaleProperty, value);
//            }
//        }

//        /// <summary>
//        /// Event raised when the ContentScale property has changed.
//        /// </summary>
//        public event EventHandler ContentScaleChanged;

//        /// <summary>
//        /// Get/set the minimum value for 'ContentScale'.
//        /// </summary>
//        public double MinContentScale
//        {
//            get
//            {
//                return (double)GetValue(MinContentScaleProperty);
//            }
//            set
//            {
//                SetValue(MinContentScaleProperty, value);
//            }
//        }

//        /// <summary>
//        /// Get/set the maximum value for 'ContentScale'.
//        /// </summary>
//        public double MaxContentScale
//        {
//            get
//            {
//                return (double)GetValue(MaxContentScaleProperty);
//            }
//            set
//            {
//                SetValue(MaxContentScaleProperty, value);
//            }
//        }

//        /// <summary>
//        /// The X coordinate of the _content focus, this is the point that we are focusing on when zooming.
//        /// </summary>
//        public double ContentZoomFocusX
//        {
//            get
//            {
//                return (double)GetValue(ContentZoomFocusXProperty);
//            }
//            set
//            {
//                SetValue(ContentZoomFocusXProperty, value);
//            }
//        }

//        /// <summary>
//        /// The Y coordinate of the _content focus, this is the point that we are focusing on when zooming.
//        /// </summary>
//        public double ContentZoomFocusY
//        {
//            get
//            {
//                return (double)GetValue(ContentZoomFocusYProperty);
//            }
//            set
//            {
//                SetValue(ContentZoomFocusYProperty, value);
//            }
//        }

//        /// <summary>
//        /// The X coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
//        /// that the _content focus point is locked to while zooming in.
//        /// </summary>
//        public double ViewportZoomFocusX
//        {
//            get
//            {
//                return (double)GetValue(ViewportZoomFocusXProperty);
//            }
//            set
//            {
//                SetValue(ViewportZoomFocusXProperty, value);
//            }
//        }

//        /// <summary>
//        /// The Y coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
//        /// that the _content focus point is locked to while zooming in.
//        /// </summary>
//        public double ViewportZoomFocusY
//        {
//            get
//            {
//                return (double)GetValue(ViewportZoomFocusYProperty);
//            }
//            set
//            {
//                SetValue(ViewportZoomFocusYProperty, value);
//            }
//        }

//        /// <summary>
//        /// The duration of the animations (in seconds) started by calling AnimatedZoomTo and the other animation methods.
//        /// </summary>
//        public double AnimationDuration
//        {
//            get
//            {
//                return (double)GetValue(AnimationDurationProperty);
//            }
//            set
//            {
//                SetValue(AnimationDurationProperty, value);
//            }
//        }

//        /// <summary>
//        /// Get the viewport width, in _content coordinates.
//        /// </summary>
//        public double ContentViewportWidth
//        {
//            get
//            {
//                return (double)GetValue(ContentViewportWidthProperty);
//            }
//            set
//            {
//                SetValue(ContentViewportWidthProperty, value);
//            }
//        }

//        /// <summary>
//        /// Get the viewport height, in _content coordinates.
//        /// </summary>
//        public double ContentViewportHeight
//        {
//            get
//            {
//                return (double)GetValue(ContentViewportHeightProperty);
//            }
//            set
//            {
//                SetValue(ContentViewportHeightProperty, value);
//            }
//        }

//        /// <summary>
//        /// Set to 'true' to enable the mouse wheel to scroll the zoom and pan control.
//        /// This is set to 'false' by default.
//        /// </summary>
//        public bool IsMouseWheelScrollingEnabled
//        {
//            get
//            {
//                return (bool)GetValue(IsMouseWheelScrollingEnabledProperty);
//            }
//            set
//            {
//                SetValue(IsMouseWheelScrollingEnabledProperty, value);
//            }
//        }

//        /// <summary>
//        /// Do an animated zoom to view a specific scale and rectangle (in _content coordinates).
//        /// </summary>
//        public void AnimatedZoomTo(double newScale, Rect contentRect)
//        {
//            AnimatedZoomPointToViewportCenter(
//                newScale,
//                new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)),
//                (sender, args) =>
//                {
//                    //
//                    // At the end of the animation, ensure that we are snapped to the specified _content offset.
//                    // Due to zooming in on the _content focus point and rounding errors, the _content offset may
//                    // be slightly off what we want at the end of the animation and this bit of code corrects it.
//                    //
//                    ContentOffsetX = contentRect.X;
//                    ContentOffsetY = contentRect.Y;
//                });
//        }

//        /// <summary>
//        /// Do an animated zoom to the specified rectangle (in _content coordinates).
//        /// </summary>
//        public void AnimatedZoomTo(Rect contentRect)
//        {
//            double scaleX = ContentViewportWidth / contentRect.Width;
//            double scaleY = ContentViewportHeight / contentRect.Height;
//            double newScale = ContentScale * Math.Min(scaleX, scaleY);

//            AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)), null);
//        }

//        /// <summary>
//        /// Instantly zoom to the specified rectangle (in _content coordinates).
//        /// </summary>
//        public void ZoomTo(Rect contentRect)
//        {
//            double scaleX = ContentViewportWidth / contentRect.Width;
//            double scaleY = ContentViewportHeight / contentRect.Height;
//            double newScale = ContentScale * Math.Min(scaleX, scaleY);

//            ZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)));
//        }

//        /// <summary>
//        /// Instantly center the view on the specified point (in _content coordinates).
//        /// </summary>
//        public void SnapContentOffsetTo(Point contentOffset)
//        {
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

//            ContentOffsetX = contentOffset.X;
//            ContentOffsetY = contentOffset.Y;
//        }

//        /// <summary>
//        /// Instantly center the view on the specified point (in _content coordinates).
//        /// </summary>
//        public void SnapTo(Point contentPoint)
//        {
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

//            ContentOffsetX = contentPoint.X - (ContentViewportWidth / 2);
//            ContentOffsetY = contentPoint.Y - (ContentViewportHeight / 2);
//        }

//        /// <summary>
//        /// Use animation to center the view on the specified point (in _content coordinates).
//        /// </summary>
//        public void AnimatedSnapTo(Point contentPoint)
//        {
//            var newX = contentPoint.X - (ContentViewportWidth / 2);
//            var newY = contentPoint.Y - (ContentViewportHeight / 2);
//            ZoomAnimationHelper.StartAnimation(this, ContentOffsetXProperty, newX, AnimationDuration);
//            ZoomAnimationHelper.StartAnimation(this, ContentOffsetYProperty, newY, AnimationDuration);
//        }

//        /// <summary>
//        /// Zoom in/out centered on the specified point (in _content coordinates).
//        /// The focus point is kept locked to it's on screen position (ala google maps).
//        /// </summary>
//        public void AnimatedZoomAboutPoint(double newContentScale, Point contentZoomFocus)
//        {
//            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

//            ZoomAnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

//            ContentZoomFocusX = contentZoomFocus.X;
//            ContentZoomFocusY = contentZoomFocus.Y;
//            ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * ContentScale;
//            ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * ContentScale;

//            //
//            // When zooming about a point make updates to ContentScale also update _content offset.
//            //
//            _enableContentOffsetUpdateFromScale = true;

//            ZoomAnimationHelper.StartAnimation(
//                this,
//                ContentScaleProperty,
//                newContentScale,
//                AnimationDuration,
//                (sender, args) =>
//                {
//                    _enableContentOffsetUpdateFromScale = false;
//                    ResetViewportZoomFocus();
//                });
//        }

//        /// <summary>
//        /// Zoom in/out centered on the specified point (in _content coordinates).
//        /// The focus point is kept locked to it's on screen position (ala google maps).
//        /// </summary>
//        public void ZoomAboutPoint(double newContentScale, Point contentZoomFocus)
//        {
//            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

//            double screenSpaceZoomOffsetX = (contentZoomFocus.X - ContentOffsetX) * ContentScale;
//            double screenSpaceZoomOffsetY = (contentZoomFocus.Y - ContentOffsetY) * ContentScale;
//            double contentSpaceZoomOffsetX = screenSpaceZoomOffsetX / newContentScale;
//            double contentSpaceZoomOffsetY = screenSpaceZoomOffsetY / newContentScale;
//            double newContentOffsetX = contentZoomFocus.X - contentSpaceZoomOffsetX;
//            double newContentOffsetY = contentZoomFocus.Y - contentSpaceZoomOffsetY;

//            ZoomAnimationHelper.CancelAnimation(this, ContentScaleProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

//            ContentScale = newContentScale;
//            ContentOffsetX = newContentOffsetX;
//            ContentOffsetY = newContentOffsetY;
//        }

//        /// <summary>
//        /// Zoom in/out centered on the viewport center.
//        /// </summary>
//        public void AnimatedZoomTo(double contentScale)
//        {
//            var zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
//            AnimatedZoomAboutPoint(contentScale, zoomCenter);
//        }

//        /// <summary>
//        /// Zoom in/out centered on the viewport center.
//        /// </summary>
//        public void ZoomTo(double contentScale)
//        {
//            var zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
//            ZoomAboutPoint(contentScale, zoomCenter);
//        }

//        /// <summary>
//        /// Do animation that scales the _content so that it fits completely in the control.
//        /// </summary>
//        public void AnimatedScaleToFit()
//        {
//            if (_content == null)
//            {
//                throw new ApplicationException("PART_Content was not found in the ZoomAndPanControl visual template!");
//            }

//            AnimatedZoomTo(new Rect(0, 0, _content.ActualWidth, _content.ActualHeight));
//        }

//        /// <summary>
//        /// Instantly scale the _content so that it fits completely in the control.
//        /// </summary>
//        public void ScaleToFit()
//        {
//            if (_content == null)
//            {
//                throw new ApplicationException("PART_Content was not found in the ZoomAndPanControl visual template!");
//            }

//            ZoomTo(new Rect(0, 0, _content.ActualWidth, _content.ActualHeight));
//        }

//        #endregion Public Methods

//        #region IScrollInfo Implementation

//        /// <summary>
//        /// Set to 'true' when the vertical scrollbar is enabled.
//        /// </summary>
//        public bool CanVerticallyScroll
//        {
//            get
//            {
//                return _canVerticallyScroll;
//            }
//            set
//            {
//                _canVerticallyScroll = value;
//            }
//        }

//        /// <summary>
//        /// Set to 'true' when the vertical scrollbar is enabled.
//        /// </summary>
//        public bool CanHorizontallyScroll
//        {
//            get
//            {
//                return _canHorizontallyScroll;
//            }
//            set
//            {
//                _canHorizontallyScroll = value;
//            }
//        }

//        /// <summary>
//        /// The width of the _content (with 'ContentScale' applied).
//        /// </summary>
//        public double ExtentWidth
//        {
//            get
//            {
//                return _unScaledExtent.Width * ContentScale;
//            }
//        }

//        /// <summary>
//        /// The height of the _content (with 'ContentScale' applied).
//        /// </summary>
//        public double ExtentHeight
//        {
//            get
//            {
//                return _unScaledExtent.Height * ContentScale;
//            }
//        }

//        /// <summary>
//        /// Get the width of the viewport onto the _content.
//        /// </summary>
//        public double ViewportWidth
//        {
//            get
//            {
//                return _viewport.Width;
//            }
//        }

//        /// <summary>
//        /// Get the height of the viewport onto the _content.
//        /// </summary>
//        public double ViewportHeight
//        {
//            get
//            {
//                return _viewport.Height;
//            }
//        }

//        /// <summary>
//        /// Reference to the ScrollViewer that is wrapped (in XAML) around the ZoomAndPanControl.
//        /// Or set to null if there is no ScrollViewer.
//        /// </summary>
//        public ScrollViewer ScrollOwner
//        {
//            get
//            {
//                return _scrollOwner;
//            }
//            set
//            {
//                _scrollOwner = value;
//            }
//        }

//        /// <summary>
//        /// The offset of the horizontal scrollbar.
//        /// </summary>
//        public double HorizontalOffset
//        {
//            get
//            {
//                return ContentOffsetX * ContentScale;
//            }
//        }

//        /// <summary>
//        /// The offset of the vertical scrollbar.
//        /// </summary>
//        public double VerticalOffset
//        {
//            get
//            {
//                return ContentOffsetY * ContentScale;
//            }
//        }

//        /// <summary>
//        /// Called when the offset of the horizontal scrollbar has been set.
//        /// </summary>
//        public void SetHorizontalOffset(double offset)
//        {
//            if (_disableScrollOffsetSync)
//            {
//                return;
//            }

//            try
//            {
//                _disableScrollOffsetSync = true;

//                ContentOffsetX = offset / ContentScale;
//            }
//            finally
//            {
//                _disableScrollOffsetSync = false;
//            }
//        }

//        /// <summary>
//        /// Called when the offset of the vertical scrollbar has been set.
//        /// </summary>
//        public void SetVerticalOffset(double offset)
//        {
//            if (_disableScrollOffsetSync)
//            {
//                return;
//            }

//            try
//            {
//                _disableScrollOffsetSync = true;

//                ContentOffsetY = offset / ContentScale;
//            }
//            finally
//            {
//                _disableScrollOffsetSync = false;
//            }
//        }

//        /// <summary>
//        /// Shift the _content offset one line up.
//        /// </summary>
//        public void LineUp()
//        {
//            ContentOffsetY -= (ContentViewportHeight / 10);
//        }

//        /// <summary>
//        /// Shift the _content offset one line down.
//        /// </summary>
//        public void LineDown()
//        {
//            ContentOffsetY += (ContentViewportHeight / 10);
//        }

//        /// <summary>
//        /// Shift the _content offset one line left.
//        /// </summary>
//        public void LineLeft()
//        {
//            ContentOffsetX -= (ContentViewportWidth / 10);
//        }

//        /// <summary>
//        /// Shift the _content offset one line right.
//        /// </summary>
//        public void LineRight()
//        {
//            ContentOffsetX += (ContentViewportWidth / 10);
//        }

//        /// <summary>
//        /// Shift the _content offset one page up.
//        /// </summary>
//        public void PageUp()
//        {
//            ContentOffsetY -= ContentViewportHeight;
//        }

//        /// <summary>
//        /// Shift the _content offset one page down.
//        /// </summary>
//        public void PageDown()
//        {
//            ContentOffsetY += ContentViewportHeight;
//        }

//        /// <summary>
//        /// Shift the _content offset one page left.
//        /// </summary>
//        public void PageLeft()
//        {
//            ContentOffsetX -= ContentViewportWidth;
//        }

//        /// <summary>
//        /// Shift the _content offset one page right.
//        /// </summary>
//        public void PageRight()
//        {
//            ContentOffsetX += ContentViewportWidth;
//        }

//        /// <summary>
//        /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
//        /// used for zooming in and out, not for manipulating the scrollbars.
//        /// </summary>
//        public void MouseWheelDown()
//        {
//            if (IsMouseWheelScrollingEnabled)
//            {
//                LineDown();
//            }
//        }

//        /// <summary>
//        /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
//        /// used for zooming in and out, not for manipulating the scrollbars.
//        /// </summary>
//        public void MouseWheelLeft()
//        {
//            if (IsMouseWheelScrollingEnabled)
//            {
//                LineLeft();
//            }
//        }

//        /// <summary>
//        /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
//        /// used for zooming in and out, not for manipulating the scrollbars.
//        /// </summary>
//        public void MouseWheelRight()
//        {
//            if (IsMouseWheelScrollingEnabled)
//            {
//                LineRight();
//            }
//        }

//        /// <summary>
//        /// Don't handle mouse wheel input from the ScrollViewer, the mouse wheel is
//        /// used for zooming in and out, not for manipulating the scrollbars.
//        /// </summary>
//        public void MouseWheelUp()
//        {
//            if (IsMouseWheelScrollingEnabled)
//            {
//                LineUp();
//            }
//        }

//        /// <summary>
//        /// Bring the specified rectangle to view.
//        /// </summary>
//        public Rect MakeVisible(Visual visual, Rect rectangle)
//        {
//            if (_content.IsAncestorOf(visual))
//            {
//                var transformedRect = visual.TransformToAncestor(_content).TransformBounds(rectangle);
//                var viewportRect = new Rect(ContentOffsetX, ContentOffsetY, ContentViewportWidth, ContentViewportHeight);
//                if (!transformedRect.Contains(viewportRect))
//                {
//                    double horizOffset = 0;
//                    double vertOffset = 0;

//                    if (transformedRect.Left < viewportRect.Left)
//                    {
//                        //
//                        // Want to move viewport left.
//                        //
//                        horizOffset = transformedRect.Left - viewportRect.Left;
//                    }
//                    else if (transformedRect.Right > viewportRect.Right)
//                    {
//                        //
//                        // Want to move viewport right.
//                        //
//                        horizOffset = transformedRect.Right - viewportRect.Right;
//                    }

//                    if (transformedRect.Top < viewportRect.Top)
//                    {
//                        //
//                        // Want to move viewport up.
//                        //
//                        vertOffset = transformedRect.Top - viewportRect.Top;
//                    }
//                    else if (transformedRect.Bottom > viewportRect.Bottom)
//                    {
//                        //
//                        // Want to move viewport down.
//                        //
//                        vertOffset = transformedRect.Bottom - viewportRect.Bottom;
//                    }

//                    SnapContentOffsetTo(new Point(ContentOffsetX + horizOffset, ContentOffsetY + vertOffset));
//                }
//            }
//            return rectangle;
//        }


//        #endregion IScrollInfo Implementation

//        #region Internal Methods

//        /// <summary>
//        /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
//        /// </summary>
//        static ZoomAndPanCanvas()
//        {
//            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomAndPanCanvas), new FrameworkPropertyMetadata(typeof(ZoomAndPanCanvas)));
//        }

//        /// <summary>
//        /// Called when a template has been applied to the control.
//        /// </summary>
//        public override void OnApplyTemplate()
//        {
//            base.OnApplyTemplate();

//            _content = Template.FindName("PART_Content", this) as FrameworkElement;
//            if (_content != null)
//            {
//                //
//                // Setup the transform on the _content so that we can scale it by 'ContentScale'.
//                //
//                _contentScaleTransform = new ScaleTransform(ContentScale, ContentScale);

//                //
//                // Setup the transform on the _content so that we can translate it by 'ContentOffsetX' and 'ContentOffsetY'.
//                //
//                _contentOffsetTransform = new TranslateTransform();
//                UpdateTranslationX();
//                UpdateTranslationY();

//                //
//                // Setup a transform group to contain the translation and scale transforms, and then
//                // assign this to the _content's 'RenderTransform'.
//                //
//                var transformGroup = new TransformGroup();
//                transformGroup.Children.Add(_contentOffsetTransform);
//                transformGroup.Children.Add(_contentScaleTransform);
//                _content.RenderTransform = transformGroup;
//            }
//        }

//        /// <summary>
//        /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
//        /// </summary>
//        private void AnimatedZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus, EventHandler callback)
//        {
//            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

//            ZoomAnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

//            ContentZoomFocusX = contentZoomFocus.X;
//            ContentZoomFocusY = contentZoomFocus.Y;
//            ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * ContentScale;
//            ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * ContentScale;

//            //
//            // When zooming about a point make updates to ContentScale also update _content offset.
//            //
//            _enableContentOffsetUpdateFromScale = true;

//            ZoomAnimationHelper.StartAnimation(
//                this,
//                ContentScaleProperty,
//                newContentScale,
//                AnimationDuration,
//                (sender, args) =>
//                {
//                    _enableContentOffsetUpdateFromScale = false;
//                    if (callback != null)
//                    {
//                        callback(this, EventArgs.Empty);
//                    }
//                });

//            ZoomAnimationHelper.StartAnimation(this, ViewportZoomFocusXProperty, ViewportWidth / 2, AnimationDuration);
//            ZoomAnimationHelper.StartAnimation(this, ViewportZoomFocusYProperty, ViewportHeight / 2, AnimationDuration);
//        }

//        /// <summary>
//        /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
//        /// </summary>
//        private void ZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus)
//        {
//            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

//            ZoomAnimationHelper.CancelAnimation(this, ContentScaleProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
//            ZoomAnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

//            ContentScale = newContentScale;
//            ContentOffsetX = contentZoomFocus.X - (ContentViewportWidth / 2);
//            ContentOffsetY = contentZoomFocus.Y - (ContentViewportHeight / 2);
//        }

//        /// <summary>
//        /// Event raised when the 'ContentScale' property has changed value.
//        /// </summary>
//        private static void ContentScalePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
//        {
//            var c = (ZoomAndPanCanvas)o;

//            if (c._contentScaleTransform != null)
//            {
//                //
//                // Update the _content scale transform whenever 'ContentScale' changes.
//                //
//                c._contentScaleTransform.ScaleX = c.ContentScale;
//                c._contentScaleTransform.ScaleY = c.ContentScale;
//            }

//            //
//            // Update the size of the viewport in _content coordinates.
//            //
//            c.UpdateContentViewportSize();

//            if (c._enableContentOffsetUpdateFromScale)
//            {
//                try
//                {
//                    // 
//                    // Disable _content focus syncronization.  We are about to update _content offset whilst zooming
//                    // to ensure that the viewport is focused on our desired _content focus point.  Setting this
//                    // to 'true' stops the automatic update of the _content focus when _content offset changes.
//                    //
//                    c._disableContentFocusSync = true;

//                    //
//                    // Whilst zooming in or out keep the _content offset up-to-date so that the viewport is always
//                    // focused on the _content focus point (and also so that the _content focus is locked to the 
//                    // viewport focus point - this is how the google maps style zooming works).
//                    //
//                    double viewportOffsetX = c.ViewportZoomFocusX - (c.ViewportWidth / 2);
//                    double viewportOffsetY = c.ViewportZoomFocusY - (c.ViewportHeight / 2);
//                    double contentOffsetX = viewportOffsetX / c.ContentScale;
//                    double contentOffsetY = viewportOffsetY / c.ContentScale;
//                    c.ContentOffsetX = (c.ContentZoomFocusX - (c.ContentViewportWidth / 2)) - contentOffsetX;
//                    c.ContentOffsetY = (c.ContentZoomFocusY - (c.ContentViewportHeight / 2)) - contentOffsetY;
//                }
//                finally
//                {
//                    c._disableContentFocusSync = false;
//                }
//            }

//            if (c.ContentScaleChanged != null)
//            {
//                c.ContentScaleChanged(c, EventArgs.Empty);
//            }

//            if (c._scrollOwner != null)
//            {
//                c._scrollOwner.InvalidateScrollInfo();
//            }
//        }

//        /// <summary>
//        /// Method called to clamp the 'ContentScale' value to its valid range.
//        /// </summary>
//        private static object ContentScaleCoerce(DependencyObject d, object baseValue)
//        {
//            var c = (ZoomAndPanCanvas)d;
//            var value = (double)baseValue;
//            value = Math.Min(Math.Max(value, c.MinContentScale), c.MaxContentScale);
//            return value;
//        }

//        /// <summary>
//        /// Event raised 'MinContentScale' or 'MaxContentScale' has changed.
//        /// </summary>
//        private static void MinOrMaxContentScalePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
//        {
//            var c = (ZoomAndPanCanvas)o;
//            c.ContentScale = Math.Min(Math.Max(c.ContentScale, c.MinContentScale), c.MaxContentScale);
//        }

//        /// <summary>
//        /// Event raised when the 'ContentOffsetX' property has changed value.
//        /// </summary>
//        private static void ContentOffsetXPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
//        {
//            var c = (ZoomAndPanCanvas)o;
//            c.UpdateTranslationX();
//            if (!c._disableContentFocusSync)
//            {
//                //
//                // Normally want to automatically update _content focus when _content offset changes.
//                // Although this is disabled using 'disableContentFocusSync' when _content offset changes due to in-progress zooming.
//                //
//                c.UpdateContentZoomFocusX();
//            }

//            if (c.ContentOffsetXChanged != null)
//            {
//                //
//                // Raise an event to let users of the control know that the _content offset has changed.
//                //
//                c.ContentOffsetXChanged(c, EventArgs.Empty);
//            }

//            if (!c._disableScrollOffsetSync && c._scrollOwner != null)
//            {
//                //
//                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
//                //
//                c._scrollOwner.InvalidateScrollInfo();
//            }
//        }

//        /// <summary>
//        /// Method called to clamp the 'ContentOffsetX' value to its valid range.
//        /// </summary>
//        private static object ContentOffsetXCoerce(DependencyObject d, object baseValue)
//        {
//            var c = (ZoomAndPanCanvas)d;
//            var value = (double)baseValue;
//            const double minOffsetX = 0.0;
//            var maxOffsetX = Math.Max(0.0, c._unScaledExtent.Width - c._constrainedContentViewportWidth);
//            value = Math.Min(Math.Max(value, minOffsetX), maxOffsetX);
//            return value;
//        }

//        /// <summary>
//        /// Event raised when the 'ContentOffsetY' property has changed value.
//        /// </summary>
//        private static void ContentOffsetYPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
//        {
//            var c = (ZoomAndPanCanvas)o;

//            c.UpdateTranslationY();

//            if (!c._disableContentFocusSync)
//            {
//                //
//                // Normally want to automatically update _content focus when _content offset changes.
//                // Although this is disabled using 'disableContentFocusSync' when _content offset changes due to in-progress zooming.
//                //
//                c.UpdateContentZoomFocusY();
//            }

//            if (c.ContentOffsetYChanged != null)
//            {
//                //
//                // Raise an event to let users of the control know that the _content offset has changed.
//                //
//                c.ContentOffsetYChanged(c, EventArgs.Empty);
//            }

//            if (!c._disableScrollOffsetSync && c._scrollOwner != null)
//            {
//                //
//                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
//                //
//                c._scrollOwner.InvalidateScrollInfo();
//            }

//        }

//        /// <summary>
//        /// Method called to clamp the 'ContentOffsetY' value to its valid range.
//        /// </summary>
//        private static object ContentOffsetYCoerce(DependencyObject d, object baseValue)
//        {
//            var c = (ZoomAndPanCanvas)d;
//            var value = (double)baseValue;
//            const double minOffsetY = 0.0;
//            var maxOffsetY = Math.Max(0.0, c._unScaledExtent.Height - c._constrainedContentViewportHeight);
//            value = Math.Min(Math.Max(value, minOffsetY), maxOffsetY);
//            return value;
//        }

//        /// <summary>
//        /// Reset the viewport zoom focus to the center of the viewport.
//        /// </summary>
//        private void ResetViewportZoomFocus()
//        {
//            ViewportZoomFocusX = ViewportWidth / 2;
//            ViewportZoomFocusY = ViewportHeight / 2;
//        }

//        /// <summary>
//        /// Update the viewport size from the specified size.
//        /// </summary>
//        private void UpdateViewportSize(Size newSize)
//        {
//            if (_viewport == newSize)
//            {
//                //
//                // The viewport is already the specified size.
//                //
//                return;
//            }

//            _viewport = newSize;

//            //
//            // Update the viewport size in _content coordiates.
//            //
//            UpdateContentViewportSize();

//            //
//            // Initialise the _content zoom focus point.
//            //
//            UpdateContentZoomFocusX();
//            UpdateContentZoomFocusY();

//            //
//            // Reset the viewport zoom focus to the center of the viewport.
//            //
//            ResetViewportZoomFocus();

//            //
//            // Update _content offset from itself when the size of the viewport changes.
//            // This ensures that the _content offset remains properly clamped to its valid range.
//            //
//            ContentOffsetX = ContentOffsetX;
//            ContentOffsetY = ContentOffsetY;

//            if (_scrollOwner != null)
//            {
//                //
//                // Tell that owning ScrollViewer that scrollbar data has changed.
//                //
//                _scrollOwner.InvalidateScrollInfo();
//            }
//        }

//        /// <summary>
//        /// Update the size of the viewport in _content coordinates after the viewport size or 'ContentScale' has changed.
//        /// </summary>
//        private void UpdateContentViewportSize()
//        {
//            ContentViewportWidth = ViewportWidth / ContentScale;
//            ContentViewportHeight = ViewportHeight / ContentScale;

//            _constrainedContentViewportWidth = Math.Min(ContentViewportWidth, _unScaledExtent.Width);
//            _constrainedContentViewportHeight = Math.Min(ContentViewportHeight, _unScaledExtent.Height);

//            UpdateTranslationX();
//            UpdateTranslationY();
//        }

//        /// <summary>
//        /// Update the X coordinate of the translation transformation.
//        /// </summary>
//        private void UpdateTranslationX()
//        {
//            if (_contentOffsetTransform != null)
//            {
//                double scaledContentWidth = _unScaledExtent.Width * ContentScale;
//                if (scaledContentWidth < ViewportWidth)
//                {
//                    //
//                    // When the _content can fit entirely within the viewport, center it.
//                    //
//                    _contentOffsetTransform.X = (ContentViewportWidth - _unScaledExtent.Width) / 2;
//                }
//                else
//                {
//                    _contentOffsetTransform.X = -ContentOffsetX;
//                }
//            }
//        }

//        /// <summary>
//        /// Update the Y coordinate of the translation transformation.
//        /// </summary>
//        private void UpdateTranslationY()
//        {
//            if (_contentOffsetTransform != null)
//            {
//                double scaledContentHeight = _unScaledExtent.Height * ContentScale;
//                if (scaledContentHeight < ViewportHeight)
//                {
//                    //
//                    // When the _content can fit entirely within the viewport, center it.
//                    //
//                    _contentOffsetTransform.Y = (ContentViewportHeight - _unScaledExtent.Height) / 2;
//                }
//                else
//                {
//                    _contentOffsetTransform.Y = -ContentOffsetY;
//                }
//            }
//        }

//        /// <summary>
//        /// Update the X coordinate of the zoom focus point in _content coordinates.
//        /// </summary>
//        private void UpdateContentZoomFocusX()
//        {
//            ContentZoomFocusX = ContentOffsetX + (_constrainedContentViewportWidth / 2);
//        }

//        /// <summary>
//        /// Update the Y coordinate of the zoom focus point in _content coordinates.
//        /// </summary>
//        private void UpdateContentZoomFocusY()
//        {
//            ContentZoomFocusY = ContentOffsetY + (_constrainedContentViewportHeight / 2);
//        }

//        /// <summary>
//        /// Measure the control and it's children.
//        /// </summary>
//        protected override Size MeasureOverride(Size constraint)
//        {
//            var infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
//            var childSize = base.MeasureOverride(infiniteSize);

//            if (childSize != _unScaledExtent)
//            {
//                //
//                // Use the size of the child as the un-scaled extent _content.
//                //
//                _unScaledExtent = childSize;

//                if (_scrollOwner != null)
//                {
//                    _scrollOwner.InvalidateScrollInfo();
//                }
//            }

//            //
//            // Update the size of the viewport onto the _content based on the passed in 'constraint'.
//            //
//            UpdateViewportSize(constraint);

//            var width = constraint.Width;
//            var height = constraint.Height;

//            if (double.IsInfinity(width))
//            {
//                //
//                // Make sure we don't return infinity!
//                //
//                width = childSize.Width;
//            }

//            if (double.IsInfinity(height))
//            {
//                //
//                // Make sure we don't return infinity!
//                //
//                height = childSize.Height;
//            }

//            UpdateTranslationX();
//            UpdateTranslationY();

//            return new Size(width, height);
//        }

//        /// <summary>
//        /// Arrange the control and it's children.
//        /// </summary>
//        protected override Size ArrangeOverride(Size arrangeBounds)
//        {
//            var size = base.ArrangeOverride(DesiredSize);
//            if (_content.DesiredSize != _unScaledExtent)
//            {
//                //
//                // Use the size of the child as the un-scaled extent _content.
//                //
//                _unScaledExtent = _content.DesiredSize;

//                if (_scrollOwner != null)
//                {
//                    _scrollOwner.InvalidateScrollInfo();
//                }
//            }

//            //
//            // Update the size of the viewport onto the _content based on the passed in 'arrangeBounds'.
//            //
//            UpdateViewportSize(arrangeBounds);
//            return size;
//        }

//        #endregion Internal Methods 
//    }
//}
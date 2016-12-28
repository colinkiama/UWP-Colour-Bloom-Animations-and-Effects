using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using colourBloomPivot.ImageLoader;
using System;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace colourBloomPivot
{
    //This is where the fun part begins!!
    public class ColorBloomTransitionHelper : IDisposable
    {
        UIElement hostForVisual;
        Compositor _compositor;
        ContainerVisual _containerForVisuals;
        ScalarKeyFrameAnimation _bloomAnimation;
        IImageLoader _imageLoader;
        ICircleSurface _circleMaskSurface;
        

        public ColorBloomTransitionHelper(UIElement hostForVisual)
        {
            this.hostForVisual = hostForVisual;

            var visual = ElementCompositionPreview.GetElementVisual(hostForVisual);
            _compositor = visual.Compositor;


            _containerForVisuals = _compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(hostForVisual, _containerForVisuals);

            _imageLoader = ImageLoaderFactory.CreateImageLoader(_compositor);
            _circleMaskSurface = _imageLoader.CreateCircleSurface(200, Colors.White);
        }

        public delegate void ColorBloomTransitionCompletedEventHandler(object sender, EventArgs e);
        public event ColorBloomTransitionCompletedEventHandler ColorBloomTransitionCompleted;
        public void Start(Windows.UI.Color color, Rect initialBounds, Rect finalBounds)
        {

            var colorVisual = CreateVisualWithColorAndPosition(color, initialBounds, finalBounds);

            _containerForVisuals.Children.InsertAtTop(colorVisual);

            TriggerBloomAnimation(colorVisual);

        }


        public void Dispose()
        {


            _circleMaskSurface.Dispose();
            _imageLoader.Dispose();

        }

        private SpriteVisual CreateVisualWithColorAndPosition(Windows.UI.Color color, Windows.Foundation.Rect initialBounds, Windows.Foundation.Rect finalBounds)
        {

            var width = (float)initialBounds.Width;
            var height = (float)initialBounds.Height;
            var positionX = initialBounds.X;
            var positionY = initialBounds.Y;

            var circleColorVisualDiameter = (float)Math.Min(width, height);

            if (_bloomAnimation == null) InitializeBloomAnimation(circleColorVisualDiameter / 2, finalBounds, color);

            var diagonal = Math.Sqrt(2 * (circleColorVisualDiameter * circleColorVisualDiameter));
            var deltaForOffset = (diagonal - circleColorVisualDiameter) / 2;




            var offset = new Vector3((float)positionX + (float)deltaForOffset + circleColorVisualDiameter / 2,
                                           (float)positionY + circleColorVisualDiameter / 2,
                                           0f);
            var size = new Vector2(circleColorVisualDiameter);

            // create the visual with a solid colored circle as brush
            SpriteVisual coloredCircleVisual = _compositor.CreateSpriteVisual();
            coloredCircleVisual.Brush = CreateCircleBrushWithColor(color);
            coloredCircleVisual.Offset = offset;
            coloredCircleVisual.Size = size;

            // we want our scale animation to be anchored around the center of the visual
            coloredCircleVisual.AnchorPoint = new Vector2(0.5f, 0.5f);


            return coloredCircleVisual;

        }


        /// <summary>
        /// Creates a circular solid colored brush that we can apply to a visual
        /// </summary>
        private CompositionEffectBrush CreateCircleBrushWithColor(Windows.UI.Color color)
        {

            var colorBrush = _compositor.CreateColorBrush(color);

            //
            // Because Windows.UI.Composition does not have a Circle visual, we will 
            // work around by using a circular opacity mask
            // Create a simple Composite Effect, using DestinationIn (S * DA), 
            // with a color source and a named parameter source.
            //
            var effect = new CompositeEffect
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    new ColorSourceEffect()
                    {
                        Color = color
                    },
                    new CompositionEffectSourceParameter("mask")
                }

            };
            var factory = _compositor.CreateEffectFactory(effect);
            var brush = factory.CreateBrush();

            //
            // Create the mask brush using the circle mask
            //
            CompositionSurfaceBrush maskBrush = _compositor.CreateSurfaceBrush();
            maskBrush.Surface = _circleMaskSurface.Surface;
            brush.SetSourceParameter("mask", maskBrush);

            return brush;

        }

        /// <summary>
        /// Creates an animation template for a "color bloom" type effect on a circular colored visual.
        /// This is a sub-second animation on the Scale property of the visual.
        /// 
        /// <param name="initialRadius">the Radius of the circular visual</param>
        /// <param name="finalBounds">the final area to occupy</param>
        /// </summary>
        private void InitializeBloomAnimation(float initialRadius, Rect finalBounds, Windows.UI.Color color)
        {
            var maxWidth = finalBounds.Width;
            var maxHeight = finalBounds.Height;

            // when fully scaled, the circle must cover the entire viewport
            // so we use the window's diagonal width as our max radius, assuming 0,0 placement
            var maxRadius = (float)Math.Sqrt((maxWidth * maxWidth) + (maxHeight * maxHeight)); // hypotenuse

            // the scale factor is the ratio of the max radius to the original radius
            var scaleFactor = (float)Math.Round(maxRadius / initialRadius, MidpointRounding.AwayFromZero);


            var bloomEase = _compositor.CreateCubicBezierEasingFunction(  //these numbers seem to give a consistent circle even on small sized windows
                    new Vector2(0.1f, 0.4f),
                    new Vector2(0.99f, 0.65f)
                );
            _bloomAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _bloomAnimation.InsertKeyFrame(1.0f, scaleFactor, bloomEase);
            _bloomAnimation.Duration = TimeSpan.FromMilliseconds(800); // keeping this under a sec to not be obtrusive
            


        }

        /// <summary>
        /// Runs the animation
        /// </summary>
        private void TriggerBloomAnimation(SpriteVisual colorVisual)
        {

            // animate the Scale of the visual within a scoped batch
            // this gives us transactionality and allows us to do work once the transaction completes
            var batchTransaction = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            // as with all animations on Visuals, these too will run independent of the UI thread
            // so if the UI thread is busy with app code or doing layout on state/page transition,
            // these animations still run uninterruped and glitch free
            colorVisual.StartAnimation("Scale.X", _bloomAnimation);
            colorVisual.StartAnimation("Scale.Y", _bloomAnimation);

            batchTransaction.Completed += (sender, args) =>
            {
                // remove this visual from visual tree
                _containerForVisuals.Children.Remove(colorVisual);

                // notify interested parties
                ColorBloomTransitionCompleted(this, EventArgs.Empty);
            };

            batchTransaction.End();

        }


    }
}
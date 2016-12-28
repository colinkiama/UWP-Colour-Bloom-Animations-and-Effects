using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI.Xaml.Shapes;
using Windows.UI.ViewManagement;
using System.Runtime.Serialization;
using Windows.UI.Core;
using Windows.Storage;
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace colourBloomPivot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class bloomPage : Page
    {
        ColorBloomTransitionHelper transition;
        bool stopDisposing = false;
        public bloomPage()
        {
            this.InitializeComponent();
            this.InitializeTransitionHelper();
            this.Unloaded += ColorBloomTransition_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
          
        }

        private void InitializeTransitionHelper()
        {
            // we pass in the UIElement that will host our Visuals
            transition = new ColorBloomTransitionHelper(hostForVisual);

            transition.ColorBloomTransitionCompleted += ColorBloomTransitionCompleted;
        }

        private void ColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            //Changes colour of background to "White Smoke " when 
            //the animations have finished.
            UICanvas.Background = new SolidColorBrush(Windows.UI.Colors.SkyBlue);
        }

        /// Cleans up remaining surfaces when the page is unloaded.
        private void ColorBloomTransition_Unloaded(object sender, RoutedEventArgs e)
        {
            if (stopDisposing == false)
            {
                transition.Dispose();
                stopDisposing = true;
            }

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var header = bloomHeader;

            var headerPosition = bloomHeader.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));
            var initialBounds = new Windows.Foundation.Rect()
            {
                Width = header.RenderSize.Width,
                Height = header.RenderSize.Height,
                X = headerPosition.X,
                Y = headerPosition.Y
            };

            var finalBounds = Window.Current.Bounds; // maps to the bounds of the current window
            //The code is super easy to understand if you set a break point here and 
            //check to see what happens step by step ;)
            transition.Start((Windows.UI.Colors.SkyBlue),  // the color for the circlular bloom
                                 initialBounds,                                  // the initial size and position
                                       finalBounds);                             // the area to fill over the animation duration
        }
        private void UICanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var uiCanvasLocation = UICanvas.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));
            var clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(uiCanvasLocation, e.NewSize)
            };
            UICanvas.Clip = clip;
        }


    }
}

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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace colourBloomPivot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>m
    public sealed partial class MainPage : Page
    {
        bool carefulPlz = false;
        bool stopDisposing = false;
        bool gridColorBloom = false;
        bool pivotColorBloom = false;
        PropertySet _colorsByPivotItem;
        ColorBloomTransitionHelper transition;
        ColorBloomTransitionHelper buttonTransition;
        ColorBloomTransitionHelper surroundButtonTransition;
        Queue<PivotItem> pendingTransitions = new Queue<PivotItem>();
        Queue<Rectangle> pendingPageTransitions = new Queue<Rectangle>();

        public MainPage()
        {
            this.InitializeComponent();





            if (carefulPlz == false)
            {


                this.InitializeColors();
                this.InitializeTransitionHelper();
                this.Unloaded += ColorBloomTransition_Unloaded;
            }
            else
            {
                this.InitializeColors();
            }


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //If something is carried over when coming to this page
            //The following code will run
            //Note: This is so that the code will never run when the program starts since
            //this fix for the bug (when navigating to this page) would make the app crash at launch
            if (!(e.Parameter == null) && e.Parameter.ToString() != "")
            {

                carefulPlz = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {


            //this also helps combat the bug that occurs when navigated to this page
            if (carefulPlz == true)
            {

                pivotColorBloom = true;
                this.InitializeTransitionHelper();
                this.Unloaded += ColorBloomTransition_Unloaded;

            }


        }




        /// Assign a key and a colour to each pivot item. I made the key the same as the name of the 
        /// items themeselves to make it easier for myself.
        private void InitializeColors()
        {
            _colorsByPivotItem = new PropertySet();
            _colorsByPivotItem.Add("firstPivot", Windows.UI.Colors.White);
            _colorsByPivotItem.Add("secondPivot", Windows.UI.Colors.LightBlue);
            _colorsByPivotItem.Add("thirdPivot", Windows.UI.Colors.SkyBlue);

        }



        /// All of the Color Bloom transition functionality is encapsulated in this handy helper
        /// which we will init once

        private void InitializeTransitionHelper()
        {
            // we pass in the UIElement that will host our Visuals
            transition = new ColorBloomTransitionHelper(hostForVisual);
            buttonTransition = new ColorBloomTransitionHelper(hostForButtonVisual);
            surroundButtonTransition = new ColorBloomTransitionHelper(anotherHost);
        
            // when the transition completes, we need to know so we can update other property values
            transition.ColorBloomTransitionCompleted += ColorBloomTransitionCompleted;
            buttonTransition.ColorBloomTransitionCompleted += buttonColorBloomTransitionCompleted;
            surroundButtonTransition.ColorBloomTransitionCompleted += SurroundButtonTransition_ColorBloomTransitionCompleted;
        }

       










        /// Updates the background of the layout panel to the same color whose transition animation just completed.

        private void ColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            //PivotItem item = new PivotItem();
            //// Grab an item off the pending transitions queue
            //if (pivotColorBloom == true)
            //{
            //    int itemsToRemove = 0;
            //    if (pendingTransitions.Count > 0)
            //    {
            //        //var item = pendingTransitions.Dequeue();
            //        var itemUsed = pendingTransitions.Last();
            //        item = itemUsed;
            //        //Following code ensures that list is cleaned up to prevent any memory leaks/errors
            //        foreach (var itemToBeRemoved in pendingTransitions)
            //        {
            //            itemsToRemove += 1;

            //        }
            //        for (int i = 0; i == itemsToRemove; i++)
            //        {
            //            pendingTransitions.Dequeue();
            //        }
            //    }
            //    //

            //    //pendingTransitions.Dequeue();
            //    var header = item;
            //    UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[header.Name]);

            //}
            //if (gridColorBloom == true)
            //{
            //    pendingPageTransitions.Dequeue();
            //    // now remember, that bloom animation was just transitional
            //    // so we need to explicitly set the correct color as background of the layout panel
            //    UICanvas.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            //}

            //pivotColorBloom = false;
            //gridColorBloom = false;

            // Grab an item off the pending transitions queue

            var item = pendingTransitions.Dequeue();



            // now remember, that bloom animation was just transitional

            // so we need to explicitly set the correct color as background of the layout panel

            var header = (AppBarButton)item.Header;

            UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[header.Name]);

        }



        /// In response to a XAML layout event on the Grid (named UICanvas) we will keep the animation
        /// inside UICanvas.


        private void UICanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var uiCanvasLocation = UICanvas.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));
            var clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(uiCanvasLocation, e.NewSize)
            };
            UICanvas.Clip = clip;
        }


        /// Cleans up remaining surfaces when the page is unloaded.
        private void ColorBloomTransition_Unloaded(object sender, RoutedEventArgs e)
        {
            if (stopDisposing == false)
            {
                transition.Dispose();
                buttonTransition.Dispose();
                surroundButtonTransition.Dispose();
                stopDisposing = true;
            }

        }




        private void treePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {
            pivotColorBloom = true;
            var beforeheader = sender as Pivot;
            var rightBeforeHeader = beforeheader.SelectedItem as PivotItem;
            var header = rightBeforeHeader.Header as AppBarButton;
            var headerPosition = header.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var initialBounds = new Windows.Foundation.Rect()
            {
                Width = header.RenderSize.Width,
                Height = header.RenderSize.Height,
                X = headerPosition.X,
                Y = headerPosition.Y
            };

            var finalBounds = Window.Current.Bounds;  // maps to the bounds of the current window

            //The code is super easy to understand if you set a break point here and 
            //check to see what happens step by step ;)
            transition.Start((Windows.UI.Color)_colorsByPivotItem[header.Name],  // the color for the circlular bloom
                                 initialBounds,                                  // the initial size and position
                                       finalBounds);                             // the area to fill over the animation duration

            // Add item to queue of transitions
            var pivotItem = (PivotItem)mainPivot.Items.Single(i => ((AppBarButton)((PivotItem)i).Header).Name.Equals(header.Name));
            pendingTransitions.Enqueue(pivotItem);

            //This code deals with a bug that occurs when you go navigate to a new page then come back to this one.
            if (carefulPlz == true)
            {
                var item = pendingTransitions.Dequeue();
                var headerFinish = item;
                UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[headerFinish.Name]);
                carefulPlz = false;
            }
            // Make the content visible immediately, when first clicked. Subsequent clicks will be handled by Pivot Control
            var content = (FrameworkElement)pivotItem.Content;
            if (content.Visibility == Visibility.Collapsed)
            {
                content.Visibility = Visibility.Visible;
            }


            //var headerPosition = header.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            ////Uses values of the rectangle size as the size of the "header" (Initially
            ////I wanted it to use the pivot's size but I couldn't get it to work. 
            ////Would be awesome if someonebody found a way to make it work...
            //var initialBounds = new Windows.Foundation.Rect()
            //{
            //    Width = header.RenderSize.Width,
            //    Height = header.RenderSize.Height,
            //    X = headerPosition.X,
            //    Y = headerPosition.Y
            //};

            //var finalBounds = Window.Current.Bounds;  // maps to the bounds of the current window

            ////The code is super easy to understand if you set a break point here and 
            ////check to see what happens step by step ;)
            //transition.Start((Windows.UI.Color)_colorsByPivotItem[header.Name],  // the color for the circlular bloom
            //                     initialBounds,                                  // the initial size and position
            //                           finalBounds);                             // the area to fill over the animation duration

            //// Add item to queue of transitions
            //var pivotItem = (PivotItem)mainPivot.Items.Single(i => (((PivotItem)i).Name.Equals(header.Name)));
            //pendingTransitions.Enqueue(pivotItem);

            ////This code deals with a bug that occurs when you go navigate to a new page then come back to this one.
            //if (carefulPlz == true)
            //{
            //    var item = pendingTransitions.Dequeue();
            //    var headerFinish = item;
            //    UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[headerFinish.Name]);
            //    carefulPlz = false;
            //}
            //// Make the content visible immediately, when first clicked. Subsequent clicks will be handled by Pivot Control
            //var content = (FrameworkElement)pivotItem.Content;
            //if (content.Visibility == Visibility.Collapsed)
            //{
            //    content.Visibility = Visibility.Visible;
            //}
        }

        private void colourBloomButton_Click(object sender, RoutedEventArgs e)
        {
            //This is what casues the animation to occur in the button

            var header = buttonHeader;
            var headerPosition = buttonHeader.TransformToVisual(limitOfAnimation).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            //var header = sender as Button;

            //var headerPosition = header.TransformToVisual(colourBloomSpace).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            //Uses values of the rectangle size as the size of the "header" (Initially
            //I wanted it to use the pivot's size but I couldn't get it to work). 
            //Would be awesome if someonebody found a way to make it work..
             

            var initialBounds = new Windows.Foundation.Rect()
            {
                Width = header.RenderSize.Width,
                Height = header.RenderSize.Height,
                X = headerPosition.X,
                Y = headerPosition.Y
            };

            var finalPosition = limitOfAnimation.TransformToVisual(limitOfAnimation).TransformPoint(new Windows.Foundation.Point(0d, 0d));
            var finalBounds = new Rect(finalPosition, limitOfAnimation.RenderSize); // maps to the bounds of the current window
            //The code is super easy to understand if you set a break point here and 
            //check to see what happens step by step ;)
            buttonTransition.Start((Windows.UI.Color.FromArgb(255, 255, 0, 0)),  // the color for the circlular bloom
                                 initialBounds,                                  // the initial size and position
                                       finalBounds);                             // the area to fill over the animation duration

            // Add item to queue of transitions
           
        }
        
        private void limitOfAnimation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //This method is extremly vital. This creates the clipping which stops the 
            //animation from occuring outside of the button.
            var colourBloomCanvasLocation = limitOfAnimation.TransformToVisual(limitOfAnimation).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var clip = new RectangleGeometry()

            {

                Rect = new Windows.Foundation.Rect(colourBloomCanvasLocation, e.NewSize)

            };

            limitOfAnimation.Clip = clip;
        }

        //Applies color to the button after the animation has finished.
        private void buttonColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            colourBloomButton.Background = new SolidColorBrush(Windows.UI.Colors.Red);
        }

        private void surroundBloomButton_Click(object sender, RoutedEventArgs e)
        {
            //Where the multiple colorBloom effect is activated
          
            //all of these headers are actually textblocks I've placed on the sides of the grid.
            var header = topFlower;

            var headerPosition = topFlower.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var header2 = rightFlower;

            var header2Position = rightFlower.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var header3 = bottomFlower;

            var header3Position = bottomFlower.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var header4 = leftFlower;

            var header4Position = leftFlower.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));




            //Uses values of the textBlock size


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
            surroundButtonTransition.Start((Windows.UI.Color.FromArgb(255, 255, 0, 0)),  // the color for the circlular bloom
                                 initialBounds,                                  // the initial size and position
                                       finalBounds);                             // the area to fill over the animation duration

            // Add item to queue of transitions

            initialBounds = new Rect()
            {
                Width = header2.RenderSize.Width,
                Height = header2.RenderSize.Height,
                X = header2Position.X,
                Y = header2Position.Y
            };

            surroundButtonTransition.Start((Windows.UI.Color.FromArgb(255, 255, 150, 0)),  // the color for the circlular bloom
                               initialBounds,                                  // the initial size and position
                                     finalBounds);                             // the area to fill over the animation duration

            initialBounds = new Rect()
            {
                Width = header3.RenderSize.Width,
                Height = header3.RenderSize.Height,
                X = header3Position.X,
                Y = header3Position.Y
            };

            surroundButtonTransition.Start((Windows.UI.Color.FromArgb(255, 0, 255, 0)),  // the color for the circlular bloom
                               initialBounds,                                  // the initial size and position
                                     finalBounds);                             // the area to fill over the animation duration

            initialBounds = new Rect()
            {
                Width = header4.RenderSize.Width,
                Height = header4.RenderSize.Height,
                X = header4Position.X,
                Y = header4Position.Y
            };

            surroundButtonTransition.Start((Windows.UI.Color.FromArgb(255, 0, 0, 255)),  // the color for the circlular bloom
                               initialBounds,                                  // the initial size and position
                                     finalBounds);                             // the area to fill over the animation duration
        }

        private void SurroundButtonTransition_ColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            //Changes colour of background to "White Smoke " when 
            //the animations have finished.
            UICanvas.Background = new SolidColorBrush(Windows.UI.Colors.WhiteSmoke);
        }

        private void toNextPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(bloomPage));
        }

        private void header_Click(object sender, RoutedEventArgs e)
        {
            pivotColorBloom = true;
            var header = sender as AppBarButton;

            var headerPosition = header.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            //Uses values of the rectangle size as the size of the "header" (Initially
            //I wanted it to use the pivot's size but I couldn't get it to work. 
            //Would be awesome if someonebody found a way to make it work...
            var initialBounds = new Windows.Foundation.Rect()
            {
                Width = header.RenderSize.Width,
                Height = header.RenderSize.Height,
                X = headerPosition.X,
                Y = headerPosition.Y
            };

            var finalBounds = Window.Current.Bounds;  // maps to the bounds of the current window

            //The code is super easy to understand if you set a break point here and 
            //check to see what happens step by step ;)
            transition.Start((Windows.UI.Color)_colorsByPivotItem[header.Name],  // the color for the circlular bloom
                                 initialBounds,                                  // the initial size and position
                                       finalBounds);                             // the area to fill over the animation duration

            // Add item to queue of transitions
            var pivotItem = (PivotItem)mainPivot.Items.Single(i => ((AppBarButton)((PivotItem)i).Header).Name.Equals(header.Name));
            pendingTransitions.Enqueue(pivotItem);

            //This code deals with a bug that occurs when you go navigate to a new page then come back to this one.
            if (carefulPlz == true)
            {
                var item = pendingTransitions.Dequeue();
                var headerFinish = item;
                UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[headerFinish.Name]);
                carefulPlz = false;
            }
            // Make the content visible immediately, when first clicked. Subsequent clicks will be handled by Pivot Control
            var content = (FrameworkElement)pivotItem.Content;
            if (content.Visibility == Visibility.Collapsed)
            {
                content.Visibility = Visibility.Visible;
            }
        }
    }
}



    



      

       

       





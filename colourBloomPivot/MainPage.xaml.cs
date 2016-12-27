﻿
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

            // when the transition completes, we need to know so we can update other property values
            transition.ColorBloomTransitionCompleted += ColorBloomTransitionCompleted;
        }


       


       


       
        /// Updates the background of the layout panel to the same color whose transition animation just completed.
        
        private void ColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            PivotItem item = new PivotItem();
            // Grab an item off the pending transitions queue
            if (pivotColorBloom == true)
            {
                int itemsToRemove = 0;
                if (pendingTransitions.Count > 0)
                {
                    //var item = pendingTransitions.Dequeue();
                    var itemUsed = pendingTransitions.Last();
                    item = itemUsed;
                    //Following code ensures that list is cleaned up to prevent any memory leaks/errors
                    foreach (var itemToBeRemoved in pendingTransitions)
                    {
                        itemsToRemove += 1;

                    }
                    for (int i = 0; i == itemsToRemove; i++)
                    {
                        pendingTransitions.Dequeue();
                    }
                }
                //

                //pendingTransitions.Dequeue();
                var header = item;
                UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[header.Name]);

            }
            if (gridColorBloom == true)
            {
                pendingPageTransitions.Dequeue();
                // now remember, that bloom animation was just transitional
                // so we need to explicitly set the correct color as background of the layout panel
                UICanvas.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            }

            pivotColorBloom = false;
            gridColorBloom = false;
           

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
                stopDisposing = true;
            }

        }


       

        private void treePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {
            pivotColorBloom = true;
            var beforeheader = sender as Pivot;
            var header = (PivotItem)beforeheader.SelectedItem;

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
            var pivotItem = (PivotItem)mainPivot.Items.Single(i => (((PivotItem)i).Name.Equals(header.Name)));
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

    



      

       

       




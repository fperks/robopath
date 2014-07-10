//// *******************************************************
//// Project: RoboPath
//// File Name: ZoomAnimationHelper.cs
//// By: Frank Perks, S#100704853
//// *******************************************************

//using System;
//using System.Windows;
//using System.Windows.Media.Animation;

//namespace RoboPath.UI.CustomControls
//{
//    public static class ZoomAnimationHelper
//    {
//        public static void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty, double toValue, double animationDurationSeconds)
//        {
//            StartAnimation(animatableElement, dependencyProperty, toValue, animationDurationSeconds, null);
//        }

//        public static void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty, double toValue, double animationDurationSeconds, EventHandler completedEvent)
//        {
//            var fromValue = (double)animatableElement.GetValue(dependencyProperty);
//            var animation = new DoubleAnimation();
//            animation.From = fromValue;
//            animation.To = toValue;
//            animation.Duration = TimeSpan.FromSeconds(animationDurationSeconds);

//            animation.Completed += delegate(object sender, EventArgs e)
//            {
//                //
//                // When the animation has completed bake final value of the animation
//                // into the property.
//                //
//                animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
//                CancelAnimation(animatableElement, dependencyProperty);

//                if (completedEvent != null)
//                {
//                    completedEvent(sender, e);
//                }
//            };

//            animation.Freeze();

//            animatableElement.BeginAnimation(dependencyProperty, animation);
//        }

//        public static void CancelAnimation(UIElement animatableElement, DependencyProperty dependencyProperty)
//        {
//            animatableElement.BeginAnimation(dependencyProperty, null);
//        }
//    }
//}
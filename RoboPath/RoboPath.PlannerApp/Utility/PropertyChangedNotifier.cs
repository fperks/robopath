// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PropertyChangedNotifier.cs
// By: Frank Perks
// *******************************************************

using System.ComponentModel;

namespace RoboPath.PlannerApp.Utility
{
    public class PropertyChangedNotifier : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
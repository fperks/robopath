// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PropertyChangedEventFilter.cs
// By: Frank Perks
// *******************************************************

using System;
using System.ComponentModel;

namespace RoboPath.PlannerApp.Utility
{
    public class PropertyChangedEventFilter<T>
    {
        #region Fields

        private INotifyPropertyChanged _source;

        #endregion Fields

        #region Properties

        public bool IsEnabled { get; private set; }
        public string PropertyName { get; private set; }
        public Action<T> OnChangedCallback { get; private set; }

        public INotifyPropertyChanged Source
        {
            get { return _source; }
            set
            {
                if(_source != null)
                {
                    _source.PropertyChanged -= OnPropertyChanged;
                }
                _source = value;
                if(_source != null)
                {
                    _source.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        #endregion Properties

        #region Public Methods

        public PropertyChangedEventFilter(INotifyPropertyChanged source, string propertyName, Action<T> callback)
        {
            IsEnabled = true;
            Source = source;
            OnChangedCallback = callback;
            PropertyName = propertyName;
        }

        #endregion Public Methods

        #region Internal Methods

        protected void OnPropertyChanged(object source, PropertyChangedEventArgs args)
        {
            if(!IsEnabled)
            {
                return;
            }

            if(args.PropertyName != PropertyName)
            {
                return;
            }
            var value = (T)source.GetType().GetProperty(args.PropertyName).GetValue(source, null);
            OnChangedCallback(value);
        }

        #endregion Internal Methods 
    }
}
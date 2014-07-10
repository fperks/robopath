// *******************************************************
// Project: RoboPath.UI
// File Name: ToolDeactivatedEventArgs.cs
// By: Frank Perks
// *******************************************************

using System;

namespace RoboPath.UI.Tools
{
    public class ToolDeactivatedEventArgs : EventArgs
    {
        public ITool Source { get; private set; }

        public ToolDeactivatedEventArgs(ITool sourceTool)
        {
            Source = sourceTool;
        }
    }
}
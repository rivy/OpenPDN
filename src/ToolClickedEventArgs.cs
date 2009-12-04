/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    internal class ToolClickedEventArgs
        : System.EventArgs
    {
        private Type toolType;
        public Type ToolType
        {
            get
            {
                return toolType;
            }
        }

        public ToolClickedEventArgs(Tool tool)
        {
            this.toolType = tool.GetType();
        }

        public ToolClickedEventArgs(Type toolType)
        {
            this.toolType = toolType;
        }
    }
}

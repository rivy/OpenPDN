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
    internal interface IToolChooser
    {
        void SelectTool(Type toolType);
        void SelectTool(Type toolType, bool raiseEvent);
        void SetTools(ToolInfo[] toolInfos);
        event ToolClickedEventHandler ToolClicked;
    }
}

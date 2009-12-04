/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public static class LayoutUtility
    {
        public static void PerformAutoLayout(
            System.Windows.Forms.ButtonBase button,
            AutoSizeStrategy autoSizeLayoutStrategy,
            EdgeSnapOptions edgeSnapOptions)
        {
            Control container = button.Parent;

            // Make sure no extra, not-yet-defined options were snuck in
            EdgeSnapOptions allEdgeSnapOptions = EdgeSnapOptions.SnapLeftEdgeToContainerLeftEdge | EdgeSnapOptions.SnapRightEdgeToContainerRightEdge;
            if (0 != (edgeSnapOptions & ~allEdgeSnapOptions))
            {
                throw new InvalidEnumArgumentException("edgeSnapOptions");
            }

            if ((edgeSnapOptions & EdgeSnapOptions.SnapLeftEdgeToContainerLeftEdge) != 0)
            {
                int oldLeft = button.Left;
                button.Left = 0;
                button.Width += oldLeft;
            }

            if (container != null && (edgeSnapOptions & EdgeSnapOptions.SnapRightEdgeToContainerRightEdge) != 0)
            {
                button.Width = container.Width - button.Left;
            }

            switch (autoSizeLayoutStrategy)
            {
                case AutoSizeStrategy.AutoHeightAndExpandWidthToContent:
                    button.Size = button.GetPreferredSize(new Size(0, 0));
                    break;

                case AutoSizeStrategy.ExpandHeightToContentAndKeepWidth:
                    if (button.Width != 0)
                    {
                        Size preferredSizeP = button.GetPreferredSize(new Size(button.Width, 1));
                        Size preferredSize = new Size((preferredSizeP.Width * 11) / 10, preferredSizeP.Height); // add 10% padding

                        int lineHeight = preferredSize.Height;
                        int overageScale = (preferredSize.Width + (button.Width - 1)) / button.Width;
                        button.Height = lineHeight * overageScale;
                    }
                    break;

                case AutoSizeStrategy.None:
                    break;

                default:
                    throw new InvalidEnumArgumentException("autoSizeLayoutStrategy");
            }
        }
    }
}

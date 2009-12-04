/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class LassoSelectTool
        : SelectionTool
    {
        protected override List<PointF> CreateShape(List<Point> inputTracePoints)
        {
            List<PointF> inputTracePointsF = base.CreateShape(inputTracePoints);

            if (this.SelectionMode != CombineMode.Replace &&
                inputTracePointsF.Count > 2 &&
                inputTracePointsF[0] != inputTracePointsF[inputTracePointsF.Count - 1])
            {
                inputTracePointsF.Add(inputTracePointsF[0]);
            }

            return inputTracePointsF;
        }

        protected override void OnActivate()
        {
            SetCursors(
                "Cursors.LassoSelectToolCursor.cur",
                "Cursors.LassoSelectToolCursorMinus.cur",
                "Cursors.LassoSelectToolCursorPlus.cur",
                "Cursors.LassoSelectToolCursorMouseDown.cur");

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        public LassoSelectTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.LassoSelectToolIcon.png"),
                   PdnResources.GetString("LassoSelectTool.Name"),
                   PdnResources.GetString("LassoSelectTool.HelpText"),
                   's',
                   ToolBarConfigItems.None)
        {
        }
    }
}

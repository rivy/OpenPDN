/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class PaintBucketTool
        : FloodToolBase
    {
        private Cursor cursorMouseUp;
        private Brush brush;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            brush = AppEnvironment.CreateBrush((e.Button != MouseButtons.Left));
            Cursor = Cursors.WaitCursor;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Cursor = cursorMouseUp;
            base.OnMouseUp (e);
        }

        protected override void OnFillRegionComputed(Point[][] polygonSet)
        {
            using (PdnGraphicsPath path = new PdnGraphicsPath())
            {
                path.AddPolygons(polygonSet);

                using (PdnRegion fillRegion = new PdnRegion(path))
                {
                    Rectangle boundingBox = fillRegion.GetBoundsInt();

                    Surface surface = ((BitmapLayer)ActiveLayer).Surface;
                    RenderArgs ra = new RenderArgs(surface);
                    HistoryMemento ha;

                    using (PdnRegion affected = Utility.SimplifyAndInflateRegion(fillRegion))
                    {
                        ha = new BitmapHistoryMemento(Name, Image, DocumentWorkspace, DocumentWorkspace.ActiveLayerIndex, affected);
                    }

                    ra.Graphics.CompositingMode = AppEnvironment.GetCompositingMode();
                    ra.Graphics.FillRegion(brush, fillRegion.GetRegionReadOnly());

                    HistoryStack.PushNewMemento(ha);
                    ActiveLayer.Invalidate(boundingBox);
                    Update();
                }
            }
        }

        protected override void OnActivate()
        {
            // cursor-transitions
            cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.PaintBucketToolCursor.cur"));
            Cursor = cursorMouseUp;

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (cursorMouseUp != null)
            {
                cursorMouseUp.Dispose();
                cursorMouseUp = null;
            }

            base.OnDeactivate();
        }


        public PaintBucketTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.PaintBucketIcon.png"),
                   PdnResources.GetString("PaintBucketTool.Name"),
                   PdnResources.GetString("PaintBucketTool.HelpText"),
                   'f',
                   false,
                   ToolBarConfigItems.Brush | ToolBarConfigItems.Antialiasing | ToolBarConfigItems.AlphaBlending)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                if (brush != null)
                {
                    brush.Dispose();
                    brush = null;
                }
            }
        }
    }
}

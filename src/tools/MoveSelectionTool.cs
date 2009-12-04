/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryMementos;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class MoveSelectionTool
        : MoveToolBase
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MoveSelectionTool.Name");
            }
        }

        private class ContextHistoryMemento
            : ToolHistoryMemento
        {
            [Serializable]
            private class OurHistoryMementoData
                : HistoryMementoData
            {
                public Context context;

                public OurHistoryMementoData(Context context)
                {
                    this.context = (Context)context.Clone();
                }
            }

            protected override HistoryMemento OnToolUndo()
            {
                MoveSelectionTool moveSelectionTool = DocumentWorkspace.Tool as MoveSelectionTool;

                if (moveSelectionTool == null)
                {
                    throw new InvalidOperationException("Current Tool is not the MoveSelectionTool");
                }

                ContextHistoryMemento cha = new ContextHistoryMemento(DocumentWorkspace, moveSelectionTool.context, this.Name, this.Image);
                OurHistoryMementoData ohad = (OurHistoryMementoData)this.Data;
                Context newContext = ohad.context;

                moveSelectionTool.context.Dispose();
                moveSelectionTool.context = newContext;

                moveSelectionTool.DestroyNubs();

                if (moveSelectionTool.context.lifted)
                {
                    moveSelectionTool.PositionNubs(moveSelectionTool.context.currentMode);
                }

                return cha;
            }

            public ContextHistoryMemento(DocumentWorkspace documentWorkspace, Context context, string name, ImageResource image)
                : base(documentWorkspace, name, image)
            {
                this.Data = new OurHistoryMementoData(context);
            }
        }

        protected override void OnActivate()
        {
            DocumentWorkspace.EnableSelectionTinting = true;

            this.moveToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.MoveSelectionToolCursor.cur"));
            this.Cursor = this.moveToolCursor;

            this.context.offset = new Point(0, 0);
            this.context.liftedBounds = Selection.GetBoundsF();

            this.tracking = false;
            PositionNubs(this.context.currentMode);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DocumentWorkspace.EnableSelectionTinting = false;

            if (this.moveToolCursor != null)
            {
                this.moveToolCursor.Dispose();
                this.moveToolCursor = null;
            }

            if (context.lifted)
            {   
                Drop();
            }

            this.tracking = false;
            DestroyNubs();

            base.OnDeactivate();
        }

        protected override void Drop()
        {
            ContextHistoryMemento cha = new ContextHistoryMemento(this.DocumentWorkspace, this.context, this.Name, this.Image);
            this.currentHistoryMementos.Add(cha);

            SelectionHistoryMemento sha = new SelectionHistoryMemento(this.Name, this.Image, this.DocumentWorkspace);
            this.currentHistoryMementos.Add(sha);

            this.context.Dispose();
            this.context = new Context();

            this.FlushHistoryMementos(PdnResources.GetString("MoveSelectionTool.HistoryMemento.DropSelection"));
        }

        protected override void OnSelectionChanging()
        {
            base.OnSelectionChanging();

            if (!dontDrop)
            {
                if (context.lifted)
                {
                    Drop();
                }

                if (tracking)
                {
                    tracking = false;
                }
            }
        }

        protected override void OnSelectionChanged()
        {
            if (!this.context.lifted)
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            base.OnSelectionChanged();
        }

        protected override void OnLift(MouseEventArgs e)
        {
            // do nothing
        }

        protected override void PushContextHistoryMemento()
        {
            ContextHistoryMemento cha = new ContextHistoryMemento(this.DocumentWorkspace, this.context, null, null);
            this.currentHistoryMementos.Add(cha);
        }

        protected override void Render(Point newOffset, bool useNewOffset)
        {
            PositionNubs(this.context.currentMode);
        }

        protected override void PreRender()
        {
            // do nothing
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!tracking)
            {
                return;
            }

            OnMouseMove(e);

            this.rotateNub.Visible = false;
            tracking = false;
            PositionNubs(this.context.currentMode);

            string resourceName;
            switch (this.context.currentMode)
            {
                default:
                    throw new InvalidEnumArgumentException();

                case Mode.Rotate:
                    resourceName = "MoveSelectionTool.HistoryMemento.Rotate";
                    break;

                case Mode.Scale:
                    resourceName = "MoveSelectionTool.HistoryMemento.Scale";
                    break;

                case Mode.Translate:
                    resourceName = "MoveSelectionTool.HistoryMemento.Translate";
                    break;
            }

            this.context.startAngle += this.angleDelta;
            
            string actionName = PdnResources.GetString(resourceName);
            FlushHistoryMementos(actionName);
        }

        private void FlushHistoryMementos(string name)
        {
            if (this.currentHistoryMementos.Count > 0)
            {
                CompoundHistoryMemento cha = new CompoundHistoryMemento(null, null,
                    this.currentHistoryMementos.ToArray());

                string haName;

                if (name == null)
                {
                    haName = this.Name;
                }
                else
                {
                    haName = name;
                }

                ImageResource image = this.Image;

                CompoundToolHistoryMemento ctha = new CompoundToolHistoryMemento(cha, DocumentWorkspace, haName, image);

                ctha.SeriesGuid = context.seriesGuid;
                HistoryStack.PushNewMemento(ctha);

                this.currentHistoryMementos.Clear();
            }
        }

        public MoveSelectionTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.MoveSelectionToolIcon.png"),
                   MoveSelectionTool.StaticName,
                   PdnResources.GetString("MoveSelectionTool.HelpText"), // "Click and drag to move a selected region",
                   'm',
                   false,
                   ToolBarConfigItems.None)
        {
            this.context = new Context();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DestroyNubs();

                if (this.context != null)
                {
                    this.context.Dispose();
                    this.context = null;
                }
            }
        }

        protected override void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            this.dontDrop = true;

            if (e.MayAlterSuspendTool)
            {
                e.SuspendTool = false;
            }
        }

        protected override void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (this.context.lifted)
            {
                Render(context.offset, true);
            }
            else
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            this.dontDrop = false;
        }
    }
}

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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class MagicWandTool
        : FloodToolBase
    {
        private Cursor cursorMouseUp;
        private Cursor cursorMouseUpMinus;
        private Cursor cursorMouseUpPlus;
        private CombineMode combineMode;

        private Cursor GetCursor(bool ctrlDown, bool altDown)
        {
            Cursor cursor;

            if (ctrlDown)
            {
                cursor = this.cursorMouseUpPlus;
            }
            else if (altDown)
            {
                cursor = this.cursorMouseUpMinus;
            }
            else
            {
                cursor = this.cursorMouseUp;
            }

            return cursor;
        }

        private Cursor GetCursor()
        {
            return GetCursor((ModifierKeys & Keys.Control) != 0, (ModifierKeys & Keys.Alt) != 0);
        }

        protected override void OnActivate()
        {
            DocumentWorkspace.EnableSelectionTinting = true;
            this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.MagicWandToolCursor.cur"));
            this.cursorMouseUpMinus = new Cursor(PdnResources.GetResourceStream("Cursors.MagicWandToolCursorMinus.cur"));
            this.cursorMouseUpPlus = new Cursor(PdnResources.GetResourceStream("Cursors.MagicWandToolCursorPlus.cur"));
            this.Cursor = GetCursor();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }

            if (this.cursorMouseUpMinus != null)
            {
                this.cursorMouseUpMinus.Dispose();
                this.cursorMouseUpMinus = null;
            }

            if (this.cursorMouseUpPlus != null)
            {
                this.cursorMouseUpPlus.Dispose();
                this.cursorMouseUpPlus = null;
            }

            DocumentWorkspace.EnableSelectionTinting = false;
            base.OnDeactivate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Cursor = GetCursor();
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Cursor = GetCursor();
            base.OnKeyUp(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Cursor = GetCursor();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Left)
            {
                this.combineMode = CombineMode.Union;
            }
            else if ((ModifierKeys & Keys.Alt) != 0 && e.Button == MouseButtons.Left)
            {
                this.combineMode = CombineMode.Exclude;
            }
            else if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Right)
            {
                this.combineMode = CombineMode.Xor;
            }
            else if ((ModifierKeys & Keys.Alt) != 0 && e.Button == MouseButtons.Right)
            {
                this.combineMode = CombineMode.Intersect;
            }
            else
            {
                this.combineMode = AppEnvironment.SelectionCombineMode;
            }

            base.OnMouseDown(e);
        }

        protected override void OnFillRegionComputed(Point[][] polygonSet)
        {
            SelectionHistoryMemento undoAction = new SelectionHistoryMemento(this.Name, this.Image, this.DocumentWorkspace);

            Selection.PerformChanging();
            Selection.SetContinuation(polygonSet, this.combineMode);
            Selection.CommitContinuation();
            Selection.PerformChanged();

            HistoryStack.PushNewMemento(undoAction);           
        }

        public MagicWandTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.MagicWandToolIcon.png"),
                   PdnResources.GetString("MagicWandTool.Name"),
                   PdnResources.GetString("MagicWandTool.HelpText"), 
                   's',
                   false,
                   ToolBarConfigItems.SelectionCombineMode)
        {
            ClipToSelection = false;
        }
    }
}
/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryFunctions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class EditMenu
        : PdnMenuItem
    {
        private PdnMenuItem menuEditUndo;
        private PdnMenuItem menuEditRedo;
        private ToolStripSeparator menuEditSeparator1;
        private PdnMenuItem menuEditCut;
        private PdnMenuItem menuEditCopy;
        private PdnMenuItem menuEditPaste;
        private PdnMenuItem menuEditPasteInToNewLayer;
        private PdnMenuItem menuEditPasteInToNewImage;
        private ToolStripSeparator menuEditSeparator2;
        private PdnMenuItem menuEditEraseSelection;
        private PdnMenuItem menuEditFillSelection;
        private PdnMenuItem menuEditInvertSelection;
        private PdnMenuItem menuEditSelectAll;
        private PdnMenuItem menuEditDeselect;

        private bool OnBackspaceTyped(Keys keys)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null &&
                !AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty)
            {
                this.menuEditFillSelection.PerformClick();
                return true;
            }
            else
            {
                return false;
            }
        }

        public EditMenu()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Back, OnBackspaceTyped);
            PdnBaseForm.RegisterFormHotKey(Keys.Shift | Keys.Delete, OnLeftHandedCutHotKey);
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Insert, OnLeftHandedCopyHotKey);
            PdnBaseForm.RegisterFormHotKey(Keys.Shift | Keys.Insert, OnLeftHandedPasteHotKey);
            InitializeComponent();
        }

        private bool OnLeftHandedCutHotKey(Keys keys)
        {
            this.menuEditCut.PerformClick();
            return true;
        }

        private bool OnLeftHandedCopyHotKey(Keys keys)
        {
            this.menuEditCopy.PerformClick();
            return true;
        }

        private bool OnLeftHandedPasteHotKey(Keys keys)
        {
            this.menuEditPaste.PerformClick();
            return true;
        }

        private void InitializeComponent()
        {
            this.menuEditUndo = new PdnMenuItem();
            this.menuEditRedo = new PdnMenuItem();
            this.menuEditSeparator1 = new ToolStripSeparator();
            this.menuEditCut = new PdnMenuItem();
            this.menuEditCopy = new PdnMenuItem();
            this.menuEditPaste = new PdnMenuItem();
            this.menuEditPasteInToNewLayer = new PdnMenuItem();
            this.menuEditPasteInToNewImage = new PdnMenuItem();
            this.menuEditSeparator2 = new ToolStripSeparator();
            this.menuEditEraseSelection = new PdnMenuItem();
            this.menuEditFillSelection = new PdnMenuItem();
            this.menuEditInvertSelection = new PdnMenuItem();
            this.menuEditSelectAll = new PdnMenuItem();
            this.menuEditDeselect = new PdnMenuItem();
            //
            // EditMenu
            //
            this.DropDownItems.AddRange(
                new ToolStripItem[] 
                {
                    this.menuEditUndo,
                    this.menuEditRedo,
                    this.menuEditSeparator1,
                    this.menuEditCut,
                    this.menuEditCopy,
                    this.menuEditPaste,
                    this.menuEditPasteInToNewLayer,
                    this.menuEditPasteInToNewImage,
                    this.menuEditSeparator2,
                    this.menuEditEraseSelection,
                    this.menuEditFillSelection,
                    this.menuEditInvertSelection,
                    this.menuEditSelectAll,
                    this.menuEditDeselect
                });
            this.Name = "Menu.Edit";
            this.Text = PdnResources.GetString("Menu.Edit.Text");
            // 
            // menuEditUndo
            // 
            this.menuEditUndo.Name = "Undo";
            this.menuEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
            this.menuEditUndo.Click += new System.EventHandler(this.MenuEditUndo_Click);
            // 
            // menuEditRedo
            // 
            this.menuEditRedo.Name = "Redo";
            this.menuEditRedo.ShortcutKeys = Keys.Control | Keys.Y;
            this.menuEditRedo.Click += new System.EventHandler(this.MenuEditRedo_Click);
            // 
            // menuEditCut
            // 
            this.menuEditCut.Name = "Cut";
            this.menuEditCut.ShortcutKeys = Keys.Control | Keys.X;
            this.menuEditCut.Click += new System.EventHandler(this.MenuEditCut_Click);
            // 
            // menuEditCopy
            // 
            this.menuEditCopy.Name = "Copy";
            this.menuEditCopy.ShortcutKeys = Keys.Control | Keys.C;
            this.menuEditCopy.Click += new System.EventHandler(this.MenuEditCopy_Click);
            // 
            // menuEditPaste
            // 
            this.menuEditPaste.Name = "Paste";
            this.menuEditPaste.ShortcutKeys = Keys.Control | Keys.V;
            this.menuEditPaste.Click += new System.EventHandler(this.MenuEditPaste_Click);
            // 
            // menuEditPasteInToNewLayer
            // 
            this.menuEditPasteInToNewLayer.Name = "PasteInToNewLayer";
            this.menuEditPasteInToNewLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
            this.menuEditPasteInToNewLayer.Click += new System.EventHandler(this.MenuEditPasteInToNewLayer_Click);
            //
            // menuEditPasteInToNewImage
            //
            this.menuEditPasteInToNewImage.Name = "PasteInToNewImage";
            this.menuEditPasteInToNewImage.ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;
            this.menuEditPasteInToNewImage.Click += new EventHandler(MenuEditPasteInToNewImage_Click);
            // 
            // menuEditEraseSelection
            // 
            this.menuEditEraseSelection.Name = "EraseSelection";
            this.menuEditEraseSelection.ShortcutKeys = Keys.Delete;
            this.menuEditEraseSelection.Click += new System.EventHandler(this.MenuEditClearSelection_Click);
            //
            // menuEditFillSelection
            //
            this.menuEditFillSelection.Name = "FillSelection";
            this.menuEditFillSelection.ShortcutKeyDisplayString = PdnResources.GetString("Menu.Edit.FillSelection.ShortcutKeysDisplayString");
            this.menuEditFillSelection.Click += new EventHandler(MenuEditFillSelection_Click);
            // 
            // menuEditInvertSelection
            // 
            this.menuEditInvertSelection.Name = "InvertSelection";
            this.menuEditInvertSelection.Click += new System.EventHandler(this.MenuEditInvertSelection_Click);
            this.menuEditInvertSelection.ShortcutKeys = Keys.Control | Keys.I;
            // 
            // menuEditSelectAll
            // 
            this.menuEditSelectAll.Name = "SelectAll";
            this.menuEditSelectAll.ShortcutKeys = Keys.Control | Keys.A;
            this.menuEditSelectAll.Click += new System.EventHandler(this.MenuEditSelectAll_Click);
            // 
            // menuEditDeselect
            // 
            this.menuEditDeselect.Name = "Deselect";
            this.menuEditDeselect.ShortcutKeys = Keys.Control | Keys.D;
            this.menuEditDeselect.Click += new System.EventHandler(this.MenuEditDeselect_Click);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            bool selection;
            bool bitmapLayer;

            if (AppWorkspace.ActiveDocumentWorkspace == null)
            {
                selection = false;
                bitmapLayer = false;
                this.menuEditSelectAll.Enabled = false;
            }
            else
            {
                selection = !AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty;
                bitmapLayer = AppWorkspace.ActiveDocumentWorkspace.ActiveLayer is BitmapLayer;
                this.menuEditSelectAll.Enabled = true;
            }

            this.menuEditCopy.Enabled = selection;
            this.menuEditCut.Enabled = selection && bitmapLayer;
            this.menuEditEraseSelection.Enabled = selection;
            this.menuEditFillSelection.Enabled = selection;
            this.menuEditInvertSelection.Enabled = selection;
            this.menuEditDeselect.Enabled = selection;
            
            // find out if there's anything on the clipboard that we can use
            bool isClipImageAvailable = Utility.IsClipboardImageAvailable();

            this.menuEditPaste.Enabled = isClipImageAvailable && (AppWorkspace.ActiveDocumentWorkspace != null);

            if (!this.menuEditPaste.Enabled)
            {
                if (AppWorkspace.ActiveDocumentWorkspace != null &&
                    AppWorkspace.ActiveDocumentWorkspace.Tool != null)
                {
                    bool canHandle;

                    try
                    {
                        IDataObject pasted = Clipboard.GetDataObject();
                        AppWorkspace.ActiveDocumentWorkspace.Tool.PerformPasteQuery(pasted, out canHandle);
                    }

                    catch (ExternalException)
                    {
                        canHandle = false;
                    }

                    if (canHandle)
                    {
                        this.menuEditPaste.Enabled = true;
                    }
                }
            }

            this.menuEditPasteInToNewLayer.Enabled = isClipImageAvailable && (AppWorkspace.ActiveDocumentWorkspace != null);
            this.menuEditPasteInToNewImage.Enabled = isClipImageAvailable;

            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuEditUndo.Enabled = (AppWorkspace.ActiveDocumentWorkspace.History.UndoStack.Count > 1); // top of stack is always assumed to be a "NullHistoryMemento," which is not undoable! thus we don't count it
                this.menuEditRedo.Enabled = (AppWorkspace.ActiveDocumentWorkspace.History.RedoStack.Count > 0);
            }
            else
            {
                this.menuEditUndo.Enabled = false;
                this.menuEditRedo.Enabled = false;
            }

            base.OnDropDownOpening(e);
        }

        private void MenuEditUndo_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null &&
                !AppWorkspace.ActiveDocumentWorkspace.IsMouseCaptured())
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
            }
        }

        private void MenuEditRedo_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null &&
                !AppWorkspace.ActiveDocumentWorkspace.IsMouseCaptured())
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
            }
        }

        private void MenuEditCopy_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new CopyToClipboardAction(AppWorkspace.ActiveDocumentWorkspace).PerformAction();
            }
        }

        private void MenuEditCut_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                CutAction cutAction = new CutAction();
                cutAction.PerformAction(AppWorkspace.ActiveDocumentWorkspace);
            }
        }

        private void MenuEditInvertSelection_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null &&
                !AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty)
            {
                HistoryFunctionResult result = AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new InvertSelectionFunction());

                // Make sure that the selection info shows up in the status bar, and not the tool's help text
                if (result == HistoryFunctionResult.Success)
                {
                    AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanging();
                    AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanged();
                }
            }
        }

        private void MenuEditClearSelection_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new EraseSelectionFunction());
            }
        }

        private void MenuEditFillSelection_Click(object sender, EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FillSelectionFunction(AppWorkspace.AppEnvironment.PrimaryColor));
            }
        }

        private void MenuEditSelectAll_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new SelectAllFunction());

                // Make sure that the selection info shows up in the status bar, and not the tool's help text
                AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanging();
                AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanged();
            }
        }

        private void MenuEditDeselect_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new DeselectFunction());
            }
        }

        private void MenuEditPaste_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new PasteAction(AppWorkspace.ActiveDocumentWorkspace).PerformAction();
            }
        }

        private void MenuEditPasteInToNewLayer_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new PasteInToNewLayerAction(AppWorkspace.ActiveDocumentWorkspace).PerformAction();
            }
        }

        private void MenuEditPasteInToNewImage_Click(object sender, EventArgs e)
        {
            AppWorkspace.PerformAction(new PasteInToNewImageAction());
        }
    }
}

/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class FileMenu
        : PdnMenuItem
    {
        private PdnMenuItem menuFileNew;
        private PdnMenuItem menuFileOpen;
        private PdnMenuItem menuFileOpenRecent;
        private PdnMenuItem menuFileOpenRecentSentinel;
        private PdnMenuItem menuFileAcquire;
        private PdnMenuItem menuFileAcquireFromScannerOrCamera;
        private PdnMenuItem menuFileClose;
        private ToolStripSeparator menuFileSeparator1;
        private PdnMenuItem menuFileSave;
        private PdnMenuItem menuFileSaveAs;
        private ToolStripSeparator menuFileSeparator2;
        private PdnMenuItem menuFilePrint;
        private ToolStripSeparator menuFileSeparator3;
        private PdnMenuItem menuFileViewPluginLoadErrors;
        private ToolStripSeparator menuFileSeparator4;
        private PdnMenuItem menuFileExit;

        private bool OnCtrlF4Typed(Keys keys)
        {
            this.menuFileClose.PerformClick();
            return true;
        }

        public FileMenu()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.F4, OnCtrlF4Typed);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuFileNew = new PdnMenuItem();
            this.menuFileOpen = new PdnMenuItem();
            this.menuFileOpenRecent = new PdnMenuItem();
            this.menuFileOpenRecentSentinel = new PdnMenuItem();
            this.menuFileAcquire = new PdnMenuItem();
            this.menuFileAcquireFromScannerOrCamera = new PdnMenuItem();
            this.menuFileClose = new PdnMenuItem();
            this.menuFileSeparator1 = new ToolStripSeparator();
            this.menuFileSave = new PdnMenuItem();
            this.menuFileSaveAs = new PdnMenuItem();
            this.menuFileSeparator2 = new ToolStripSeparator();
            this.menuFilePrint = new PdnMenuItem();
            this.menuFileSeparator3 = new ToolStripSeparator();
            this.menuFileViewPluginLoadErrors = new PdnMenuItem();
            this.menuFileSeparator4 = new ToolStripSeparator();
            this.menuFileExit = new PdnMenuItem();
            //
            // FileMenu
            //
            this.DropDownItems.AddRange(GetMenuItemsToAdd(true));
            this.Name = "Menu.File";
            this.Text = PdnResources.GetString("Menu.File.Text");
            // 
            // menuFileNew
            // 
            this.menuFileNew.Name = "New";
            this.menuFileNew.ShortcutKeys = Keys.Control | Keys.N;
            this.menuFileNew.Click += new System.EventHandler(this.MenuFileNew_Click);
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Name = "Open";
            this.menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            this.menuFileOpen.Click += new System.EventHandler(this.MenuFileOpen_Click);
            // 
            // menuFileOpenRecent
            // 
            this.menuFileOpenRecent.Name = "OpenRecent";
            this.menuFileOpenRecent.DropDownItems.AddRange(
                new ToolStripItem[] 
                {
                    this.menuFileOpenRecentSentinel
                });
            this.menuFileOpenRecent.DropDownOpening += new System.EventHandler(this.MenuFileOpenRecent_DropDownOpening);
            // 
            // menuFileOpenRecentSentinel
            // 
            this.menuFileOpenRecentSentinel.Text = "sentinel";
            // 
            // menuFileAcquire
            // 
            this.menuFileAcquire.Name = "Acquire";
            this.menuFileAcquire.DropDownItems.AddRange(
                new ToolStripItem[] 
                {
                    this.menuFileAcquireFromScannerOrCamera
                });
            this.menuFileAcquire.DropDownOpening += new System.EventHandler(this.MenuFileAcquire_DropDownOpening);
            // 
            // menuFileAcquireFromScannerOrCamera
            // 
            this.menuFileAcquireFromScannerOrCamera.Name = "FromScannerOrCamera";
            this.menuFileAcquireFromScannerOrCamera.Click += new System.EventHandler(this.MenuFileAcquireFromScannerOrCamera_Click);
            //
            // menuFileClose
            //
            this.menuFileClose.Name = "Close";
            this.menuFileClose.Click += new EventHandler(MenuFileClose_Click);
            this.menuFileClose.ShortcutKeys = Keys.Control | Keys.W;
            // 
            // menuFileSave
            // 
            this.menuFileSave.Name = "Save";
            this.menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
            this.menuFileSave.Click += new System.EventHandler(this.MenuFileSave_Click);
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.Name = "SaveAs";
            this.menuFileSaveAs.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            this.menuFileSaveAs.Click += new System.EventHandler(this.MenuFileSaveAs_Click);
            // 
            // menuFilePrint
            // 
            this.menuFilePrint.Name = "Print";
            this.menuFilePrint.ShortcutKeys = Keys.Control | Keys.P;
            this.menuFilePrint.Click += new System.EventHandler(this.MenuFilePrint_Click);
            //
            // menuFileViewPluginLoadErrors
            //
            this.menuFileViewPluginLoadErrors.Name = "ViewPluginLoadErrors";
            this.menuFileViewPluginLoadErrors.Click += new EventHandler(MenuFileViewPluginLoadErrors_Click);
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "Exit";
            this.menuFileExit.Click += new System.EventHandler(this.MenuFileExit_Click);
        }

        private ToolStripItem[] GetMenuItemsToAdd(bool includeLoadErrors)
        {
            List<ToolStripItem> items = new List<ToolStripItem>();

            items.Add(this.menuFileNew);
            items.Add(this.menuFileOpen);
            items.Add(this.menuFileOpenRecent);
            items.Add(this.menuFileAcquire);
            items.Add(this.menuFileClose);
            items.Add(this.menuFileSeparator1);
            items.Add(this.menuFileSave);
            items.Add(this.menuFileSaveAs);
            items.Add(this.menuFileSeparator2);
            items.Add(this.menuFilePrint);
            items.Add(this.menuFileSeparator3);

            if (includeLoadErrors)
            {
                items.Add(this.menuFileViewPluginLoadErrors);
                items.Add(this.menuFileSeparator4);
            }

            items.Add(this.menuFileExit);

            return items.ToArray();
        }

        private List<Triple<Assembly, Type, Exception>> RemoveDuplicates(IList<Triple<Assembly, Type, Exception>> allErrors)
        {
            // Exception has reference identity, but we want to collate based on the message contents

            Set<Triple<Assembly, Type, string>> internedList = new Set<Triple<Assembly, Type, string>>();
            List<Triple<Assembly, Type, Exception>> noDupesList = new List<Triple<Assembly, Type, Exception>>();

            for (int i = 0; i < allErrors.Count; ++i)
            {
                Triple<Assembly, Type, string> interned = Triple.Create(
                    allErrors[i].First, allErrors[i].Second, string.Intern(allErrors[i].Third.ToString()));

                if (!internedList.Contains(interned))
                {
                    internedList.Add(interned);
                    noDupesList.Add(allErrors[i]);
                }
            }

            return noDupesList;
        }

        private void MenuFileViewPluginLoadErrors_Click(object sender, EventArgs e)
        {
            IList<Triple<Assembly, Type, Exception>> allErrors = AppWorkspace.GetEffectLoadErrors();
            IList<Triple<Assembly, Type, Exception>> errors = RemoveDuplicates(allErrors);

            using (Form errorsDialog = new Form())
            {
                errorsDialog.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileViewPluginLoadErrorsIcon.png").Reference);
                errorsDialog.Text = PdnResources.GetString("Effects.PluginLoadErrorsDialog.Text");

                Label messageLabel = new Label();
                messageLabel.Name = "messageLabel";
                messageLabel.Text = PdnResources.GetString("Effects.PluginLoadErrorsDialog.Message.Text");

                TextBox errorsBox = new TextBox();
                errorsBox.Font = new Font(FontFamily.GenericMonospace, errorsBox.Font.Size);
                errorsBox.ReadOnly = true;
                errorsBox.Multiline = true;
                errorsBox.ScrollBars = ScrollBars.Vertical;

                StringBuilder allErrorsText = new StringBuilder();
                string headerTextFormat = PdnResources.GetString("EffectErrorMessage.HeaderFormat");

                for (int i = 0; i < errors.Count; ++i)
                {
                    Assembly assembly = errors[i].First;
                    Type type = errors[i].Second;
                    Exception exception = errors[i].Third;

                    string headerText = string.Format(headerTextFormat, i + 1, errors.Count);
                    string errorText = AppWorkspace.GetLocalizedEffectErrorMessage(assembly, type, exception);

                    allErrorsText.Append(headerText);
                    allErrorsText.Append(Environment.NewLine);
                    allErrorsText.Append(errorText);

                    if (i != errors.Count - 1)
                    {
                        allErrorsText.Append(Environment.NewLine);
                    }
                }

                errorsBox.Text = allErrorsText.ToString();

                errorsDialog.Layout +=
                    delegate(object sender2, LayoutEventArgs e2)
                    {
                        int hMargin = UI.ScaleWidth(8);
                        int vMargin = UI.ScaleHeight(8);
                        int insetWidth = errorsDialog.ClientSize.Width - (hMargin * 2);

                        messageLabel.Location = new Point(hMargin, vMargin);
                        messageLabel.Width = insetWidth;
                        messageLabel.Size = messageLabel.GetPreferredSize(new Size(messageLabel.Width, 1));

                        errorsBox.Location = new Point(hMargin, messageLabel.Bottom + vMargin);
                        errorsBox.Width = insetWidth;
                        errorsBox.Height = errorsDialog.ClientSize.Height - vMargin - errorsBox.Top;
                    };

                errorsDialog.StartPosition = FormStartPosition.CenterParent;
                errorsDialog.ShowInTaskbar = false;
                errorsDialog.MinimizeBox = false;
                errorsDialog.Width *= 2;
                errorsDialog.Size = UI.ScaleSize(errorsDialog.Size);
                errorsDialog.Controls.Add(messageLabel);
                errorsDialog.Controls.Add(errorsBox);

                errorsDialog.ShowDialog(AppWorkspace);
            }
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            this.DropDownItems.Clear();

            IList<Triple<Assembly, Type, Exception>> pluginLoadErrors = AppWorkspace.GetEffectLoadErrors();

            this.DropDownItems.AddRange(GetMenuItemsToAdd(pluginLoadErrors.Count > 0));

            this.menuFileNew.Enabled = true;
            this.menuFileOpen.Enabled = true;
            this.menuFileOpenRecent.Enabled = true;
            this.menuFileOpenRecentSentinel.Enabled = true;
            this.menuFileAcquire.Enabled = true;
            this.menuFileAcquireFromScannerOrCamera.Enabled = true;
            this.menuFileExit.Enabled = true;

            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuFileSave.Enabled = true;
                this.menuFileSaveAs.Enabled = true;
                this.menuFileClose.Enabled = true;
                this.menuFilePrint.Enabled = true;
            }
            else
            {
                this.menuFileSave.Enabled = false;
                this.menuFileSaveAs.Enabled = false;
                this.menuFileClose.Enabled = false;
                this.menuFilePrint.Enabled = false;
            }

            base.OnDropDownOpening(e);
        }

        private void MenuFileOpen_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.PerformAction(new OpenFileAction());
        }

        private void DoExit()
        {
            Startup.CloseApplication();
        }

        private void MenuFileExit_Click(object sender, System.EventArgs e)
        {
            DoExit();
        }

        private void MenuFileClose_Click(object sender, EventArgs e)
        {
            if (this.AppWorkspace.DocumentWorkspaces.Length > 0)
            {
                this.AppWorkspace.PerformAction(new CloseWorkspaceAction());
            }
            else
            {
                DoExit();
            }
        }

        private void MenuFileSaveAs_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.DoSaveAs();
            }
        }

        private void MenuFileSave_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.DoSave();
            }
        }

        private void MenuFileAcquire_DropDownOpening(object sender, System.EventArgs e)
        {
            // We only disable the scanner menu item if we know for sure a scanner is not available
            // If WIA isn't available we leave the menu item enabled. That way we can give an
            // informative error message when the user clicks on it and say "scanning requires XP SP1"
            // Otherwise the user is confused and will make scathing posts on our forum.
            bool scannerEnabled = true;

            if (ScanningAndPrinting.IsComponentAvailable)
            {
                if (!ScanningAndPrinting.CanScan)
                {
                    scannerEnabled = false;
                }
            }

            menuFileAcquireFromScannerOrCamera.Enabled = scannerEnabled;
        }

        private void MenuFilePrint_Click(object sender, System.EventArgs e)
        {
            if (this.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new PrintAction());
            }
        }

        private void MenuFileOpenInNewWindow_Click(object sender, System.EventArgs e)
        {
            string fileName;
            string startingDir = Path.GetDirectoryName(AppWorkspace.ActiveDocumentWorkspace.FilePath);
            DialogResult result = DocumentWorkspace.ChooseFile(AppWorkspace, out fileName, startingDir);

            if (result == DialogResult.OK)
            {
                Startup.StartNewInstance(AppWorkspace, fileName);
            }
        }

        private void MenuFileNewWindow_Click(object sender, System.EventArgs e)
        {
            Startup.StartNewInstance(AppWorkspace, null);
        }

        private void MenuFileOpenRecent_DropDownOpening(object sender, System.EventArgs e)
        {
            AppWorkspace.MostRecentFiles.LoadMruList();
            MostRecentFile[] filesReverse = AppWorkspace.MostRecentFiles.GetFileList();
            MostRecentFile[] files = new MostRecentFile[filesReverse.Length];
            int i;

            for (i = 0; i < filesReverse.Length; ++i)
            {
                files[files.Length - i - 1] = filesReverse[i];
            }

            foreach (ToolStripItem mi in menuFileOpenRecent.DropDownItems)
            {
                mi.Click -= new EventHandler(MenuFileOpenRecentFile_Click);
            }

            menuFileOpenRecent.DropDownItems.Clear();

            i = 0;

            foreach (MostRecentFile mrf in files)
            {
                string menuName;

                if (i < 9)
                {
                    menuName = "&";
                }
                else
                {
                    menuName = "";
                }

                menuName += (1 + i).ToString() + " " + Path.GetFileName(mrf.FileName);
                ToolStripMenuItem mi = new ToolStripMenuItem(menuName);
                mi.Click += new EventHandler(MenuFileOpenRecentFile_Click);
                mi.ImageScaling = ToolStripItemImageScaling.None;
                mi.Image = (Image)mrf.Thumb.Clone();
                menuFileOpenRecent.DropDownItems.Add(mi);
                ++i;
            }

            if (menuFileOpenRecent.DropDownItems.Count == 0)
            {
                ToolStripMenuItem none = new ToolStripMenuItem(PdnResources.GetString("Menu.File.OpenRecent.None"));
                none.Enabled = false;
                menuFileOpenRecent.DropDownItems.Add(none);
            }
            else
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                menuFileOpenRecent.DropDownItems.Add(separator);

                ToolStripMenuItem clearList = new ToolStripMenuItem();
                clearList.Text = PdnResources.GetString("Menu.File.OpenRecent.ClearThisList");
                menuFileOpenRecent.DropDownItems.Add(clearList);
                Image deleteIcon = PdnResources.GetImageResource("Icons.MenuEditEraseSelectionIcon.png").Reference;
                clearList.ImageTransparentColor = Utility.TransparentKey;
                clearList.ImageAlign = ContentAlignment.MiddleCenter;
                clearList.ImageScaling = ToolStripItemImageScaling.None;
                int iconSize = AppWorkspace.MostRecentFiles.IconSize;
                Bitmap bitmap = new Bitmap(iconSize + 2, iconSize + 2);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(clearList.ImageTransparentColor);

                    Point offset = new Point((bitmap.Width - deleteIcon.Width) / 2,
                        (bitmap.Height - deleteIcon.Height) / 2);

                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(deleteIcon, offset.X, offset.Y, deleteIcon.Width, deleteIcon.Height);
                }

                clearList.Image = bitmap;
                clearList.Click += new EventHandler(ClearList_Click);
            }
        }

        private void MenuFileOpenRecentFile_Click(object sender, System.EventArgs e)
        {
            try
            {
                ToolStripMenuItem mi = (ToolStripMenuItem)sender;
                int spaceIndex = mi.Text.IndexOf(" ");
                string indexString = mi.Text.Substring(1, spaceIndex - 1);
                int index = int.Parse(indexString) - 1;
                MostRecentFile[] recentFiles = AppWorkspace.MostRecentFiles.GetFileList();
                string fileName = recentFiles[recentFiles.Length - index - 1].FileName;
                AppWorkspace.OpenFileInNewWorkspace(fileName);
            }

            catch (Exception)
            {
            }
        }

        private void MenuFileNew_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.PerformAction(new NewImageAction());
        }

        private void MenuFileAcquireFromScannerOrCamera_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.PerformAction(new AcquireFromScannerOrCameraAction());
        }

        private void ClearList_Click(object sender, EventArgs e)
        {
            AppWorkspace.PerformAction(new ClearMruListAction());
        }
    }
}

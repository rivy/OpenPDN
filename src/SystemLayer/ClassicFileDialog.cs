/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    internal abstract class ClassicFileDialog
        : IFileDialog
    {
        private FileDialog fileDialog;

        protected FileDialog FileDialog
        {
            get
            {
                return this.fileDialog;
            }
        }

        public bool CheckPathExists
        {
            get
            {
                return this.fileDialog.CheckPathExists;
            }

            set
            {
                this.fileDialog.CheckPathExists = value;
            }
        }

        public bool DereferenceLinks
        {
            get
            {
                return this.fileDialog.DereferenceLinks;
            }
            set
            {
                this.fileDialog.DereferenceLinks = value;
            }
        }

        public string Filter
        {
            get
            {
                return this.fileDialog.Filter;
            }

            set
            {
                this.fileDialog.Filter = value;
            }
        }

        public int FilterIndex
        {
            get
            {
                return this.fileDialog.FilterIndex;
            }

            set
            {
                this.fileDialog.FilterIndex = value;
            }
        }

        public string InitialDirectory
        {
            get
            {
                return this.fileDialog.InitialDirectory;
            }

            set
            {
                this.fileDialog.InitialDirectory = value;
            }
        }

        public string Title
        {
            set
            {
                this.fileDialog.Title = value;
            }
        }

        // This is a major hack to get the .NET's OFD to show with Thumbnail view by default!
        // Luckily for us this is a covert hack, and not one where we're working around a bug
        // in the framework or OS.
        // This hack works by retrieving a private property of the OFD class after it has shown
        // the dialog box.
        // Based off code found here: http://vbnet.mvps.org/index.html?code/hooks/fileopensavedlghooklvview.htm
        private static void EnableThumbnailView(FileDialog ofd)
        {
            // HACK: Must verify this still works with each new revision of .NET
            try
            {
                Type ofdType = typeof(FileDialog);
                FieldInfo fi = ofdType.GetField("dialogHWnd", BindingFlags.Instance | BindingFlags.NonPublic);

                if (fi != null)
                {
                    object dialogHWndObject = fi.GetValue(ofd);
                    IntPtr dialogHWnd = (IntPtr)dialogHWndObject;
                    IntPtr hwndLV = SafeNativeMethods.FindWindowExW(dialogHWnd, IntPtr.Zero, "SHELLDLL_DefView", null);

                    if (hwndLV != IntPtr.Zero)
                    {
                        SafeNativeMethods.SendMessageW(hwndLV, NativeConstants.WM_COMMAND, new IntPtr(NativeConstants.SHVIEW_THUMBNAIL), IntPtr.Zero);
                    }
                }
            }

            catch (Exception)
            {
                // Ignore.
            }
        }

        public DialogResult ShowDialog(IWin32Window owner, IFileDialogUICallbacks uiCallbacks)
        {
            Control ownerAsControl = owner as Control;

            if (uiCallbacks == null)
            {
                throw new ArgumentNullException("uiCallbacks");
            }

            Cursor.Current = Cursors.WaitCursor;

            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                UI.InvokeThroughModalTrampoline(
                    owner,
                    delegate(IWin32Window modalOwner)
                    {
                        while ((Control.ModifierKeys & Keys.Shift) != 0)
                        {
                            System.Threading.Thread.Sleep(1);
                            Application.DoEvents();
                        }
                    });
            }

            Cursor.Current = Cursors.Default;

            DialogResult result = DialogResult.Cancel;

            UI.InvokeThroughModalTrampoline(
                owner,
                delegate(IWin32Window modalOwner)
                {
                    if (ownerAsControl != null && ownerAsControl.IsHandleCreated)
                    {
                        ownerAsControl.BeginInvoke(new Procedure<FileDialog>(EnableThumbnailView), new object[] { this.fileDialog });
                    }

                    result = this.fileDialog.ShowDialog(modalOwner);
                });

            return result;
        }

        protected ClassicFileDialog(FileDialog fileDialog)
        {
            this.fileDialog = fileDialog;
            this.fileDialog.RestoreDirectory = true;
            this.fileDialog.ShowHelp = false;
            this.fileDialog.ValidateNames = true;
        }

        ~ClassicFileDialog()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.fileDialog != null)
                {
                    this.fileDialog.Dispose();
                    this.fileDialog = null;
                }
            }
        }
    }
}

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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PaintDotNet.SystemLayer
{
    internal sealed class VistaFileSaveDialog
          : VistaFileDialog,
            IFileSaveDialog,
            NativeInterfaces.IFileDialogEvents
    {
        private bool addExtension = true;
        private bool overwritePrompt = false;

        private NativeInterfaces.IFileSaveDialog FileSaveDialog
        {
            get
            {
                return this.FileDialog as NativeInterfaces.IFileSaveDialog;
            }
        }

        public bool AddExtension
        {
            get
            {
                return this.addExtension;
            }

            set
            {
                this.addExtension = value;
            }
        }

        private string FileNameCore
        {
            get
            {
                NativeInterfaces.IShellItem shellItem = null;
                string path;

                try
                {
                    int hr = this.FileSaveDialog.GetResult(out shellItem);

                    if (NativeMethods.SUCCEEDED(hr))
                    {
                        shellItem.GetDisplayName(NativeConstants.SIGDN.SIGDN_FILESYSPATH, out path);
                    }
                    else
                    {
                        this.FileSaveDialog.GetFileName(out path);
                    }
                }

                catch (Exception)
                {
                    this.FileSaveDialog.GetFileName(out path);
                }

                finally
                {
                    if (shellItem != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(shellItem);
                        }

                        catch (ArgumentException)
                        {
                            // Ignore error
                        }

                        shellItem = null;
                    }
                }

                return path;
            }
        }

        public string FileName
        {
            get
            {
                string pathNameCore = FileNameCore;
                string pathNameResolved = ResolveName(pathNameCore);
                return pathNameResolved;
            }

            set
            {
                this.FileSaveDialog.SetFileName(value);
            }
        }

        private string ResolveName(string path)
        {
            string returnPath;

            if (this.addExtension && path != null)
            {
                string ext = Path.GetExtension(path);

                // If they did not specify an extension, then we should add on the default one for the file type they chose
                if (string.IsNullOrEmpty(ext))
                {
                    int filterIndex = this.FilterIndex;
                    string allFilters = this.Filter;
                    string[] filtersArray = allFilters.Split('|');
                    string filter = filtersArray[1 + ((filterIndex - 1) * 2)];
                    string[] exts = filter.Split(';');

                    string newSpec = exts[0];
                    if (newSpec[0] == '*')
                    {
                        newSpec = newSpec.Substring(1);
                    }

                    returnPath = Path.ChangeExtension(path, newSpec);
                }
                else
                {
                    returnPath = path;
                }
            }
            else
            {
                returnPath = path;
            }

            return returnPath;
        }

        protected override void OnBeforeShow()
        {
            try
            {
                string fileNameCore = FileNameCore;

                if (!string.IsNullOrEmpty(fileNameCore))
                {
                    string justTheFileName = Path.GetFileName(fileNameCore);
                    string dir = Path.GetDirectoryName(fileNameCore);

                    string fullPathName = Path.GetFullPath(dir);
                    InitialDirectory = fullPathName;

                    FileName = justTheFileName;
                }
            }

            catch (Exception)
            {
            }

            SetOptions(NativeConstants.FOS.FOS_FORCEFILESYSTEM, true);
            SetOptions(NativeConstants.FOS.FOS_OVERWRITEPROMPT, false); // we handle this ourself

            base.OnBeforeShow();
        }

        public bool OverwritePrompt
        {
            get
            {
                return this.overwritePrompt;
            }

            set
            {
                this.overwritePrompt = value;
            }
        }

        public VistaFileSaveDialog()
            : base(new NativeInterfaces.NativeFileSaveDialog())
        {
            this.FileDialogEvents = this;
        }

        int NativeInterfaces.IFileDialogEvents.OnFileOk(NativeInterfaces.IFileDialog pfd)
        {
            int hr = NativeConstants.S_OK;

            NativeInterfaces.IShellItem shellItem = null;

            if (NativeMethods.SUCCEEDED(hr))
            {
                hr = FileSaveDialog.GetResult(out shellItem);
            }

            if (!NativeMethods.SUCCEEDED(hr))
            {
                throw Marshal.GetExceptionForHR(hr);
            }

            string pathName = null;

            try
            {
                shellItem.GetDisplayName(NativeConstants.SIGDN.SIGDN_FILESYSPATH, out pathName);
            }

            finally
            {
                if (shellItem != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(shellItem);
                    }

                    catch (Exception)
                    {
                    }

                    shellItem = null;
                }
            }

            string pathNameResolved = ResolveName(pathName);
            NativeInterfaces.IOleWindow oleWindow = (NativeInterfaces.IOleWindow)pfd;

            try
            {
                IntPtr hWnd = IntPtr.Zero;
                oleWindow.GetWindow(out hWnd);
                Win32Window win32Window = new Win32Window(hWnd, oleWindow);

                // File name/path validation
                if (hr >= 0)
                {
                    try
                    {
                        // Verify that these can be parsed correctly
                        string fileName = Path.GetFileName(pathNameResolved);
                        string dirName = Path.GetDirectoryName(pathNameResolved);
                    }

                    catch (Exception ex)
                    {
                        if (!FileDialogUICallbacks.ShowError(win32Window, pathNameResolved, ex))
                        {
                            throw;
                        }

                        hr = NativeConstants.S_FALSE;
                    }
                }

                if (hr >= 0)
                {
                    // Overwrite existing file
                    if (!OverwritePrompt)
                    {
                        hr = NativeConstants.S_OK;
                    }
                    else if (File.Exists(pathNameResolved))
                    {
                        FileOverwriteAction action = FileDialogUICallbacks.ShowOverwritePrompt(win32Window, pathNameResolved);

                        switch (action)
                        {
                            case FileOverwriteAction.Cancel:
                                hr = NativeConstants.S_FALSE;
                                break;

                            case FileOverwriteAction.Overwrite:
                                hr = NativeConstants.S_OK;
                                break;

                            default:
                                throw new InvalidEnumArgumentException();
                        }
                    }
                }
            }

            catch (Exception)
            {
            }

            finally
            {
                try
                {
                    Marshal.ReleaseComObject(oleWindow);
                }

                catch (Exception)
                {
                }

                oleWindow = null;
            }

            return hr;
        }

        int NativeInterfaces.IFileDialogEvents.OnFolderChanging(
            NativeInterfaces.IFileDialog pfd,
            NativeInterfaces.IShellItem psiFolder)
        {
            return NativeConstants.E_NOTIMPL;
        }

        void NativeInterfaces.IFileDialogEvents.OnFolderChange(NativeInterfaces.IFileDialog pfd)
        {
        }

        void NativeInterfaces.IFileDialogEvents.OnSelectionChange(NativeInterfaces.IFileDialog pfd)
        {
        }

        void NativeInterfaces.IFileDialogEvents.OnShareViolation(
            NativeInterfaces.IFileDialog pfd,
            NativeInterfaces.IShellItem psi,
            out NativeConstants.FDE_SHAREVIOLATION_RESPONSE pResponse)
        {
            pResponse = NativeConstants.FDE_SHAREVIOLATION_RESPONSE.FDESVR_DEFAULT;
        }

        void NativeInterfaces.IFileDialogEvents.OnTypeChange(NativeInterfaces.IFileDialog pfd)
        {
        }

        void NativeInterfaces.IFileDialogEvents.OnOverwrite(
            NativeInterfaces.IFileDialog pfd,
            NativeInterfaces.IShellItem psi,
            out NativeConstants.FDE_OVERWRITE_RESPONSE pResponse)
        {
            pResponse = NativeConstants.FDE_OVERWRITE_RESPONSE.FDEOR_DEFAULT;
        }
    }
}

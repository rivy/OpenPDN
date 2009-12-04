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
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    internal sealed class VistaFileOpenDialog
        : VistaFileDialog,
          IFileOpenDialog,
          NativeInterfaces.IFileDialogEvents
    {
        private CancelableTearOff cancelSink = null;
        private sealed class CancelableTearOff
            : ICancelable
        {
            private volatile bool canceled = false;
            public bool Canceled
            {
                get
                {
                    return this.canceled;
                }
            }

            public void RequestCancel()
            {
                this.canceled = true;
            }
        }

        private string[] fileNames = null;

        private NativeInterfaces.IFileOpenDialog FileOpenDialog
        {
            get
            {
                return this.FileDialog as NativeInterfaces.IFileOpenDialog;
            }
        }

        public bool CheckFileExists
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_FILEMUSTEXIST);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_FILEMUSTEXIST, value);
            }
        }

        public bool Multiselect
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_ALLOWMULTISELECT);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_ALLOWMULTISELECT, value);
            }
        }

        public string[] FileNames
        {
            get
            {
                return (string[])(this.fileNames ?? new string[0]).Clone();
            }

            private set
            {
                this.fileNames = (string[])value.Clone();
            }
        }

        public VistaFileOpenDialog()
            : base(new NativeInterfaces.NativeFileOpenDialog())
        {
            this.FileDialogEvents = this;
        }

        int NativeInterfaces.IFileDialogEvents.OnFileOk(NativeInterfaces.IFileDialog pfd)
        {
            int hr = NativeConstants.S_OK;

            NativeInterfaces.IShellItemArray results = null;
            FileOpenDialog.GetResults(out results);

            uint count = 0;
            results.GetCount(out count);

            List<NativeInterfaces.IShellItem> items = new List<NativeInterfaces.IShellItem>();
            List<NativeInterfaces.IShellItem> needLocalCopy = new List<NativeInterfaces.IShellItem>();
            List<NativeInterfaces.IShellItem> cannotCopy = new List<NativeInterfaces.IShellItem>();
            List<string> localPathNames = new List<string>();

            for (uint i = 0; i < count; ++i)
            {
                NativeInterfaces.IShellItem item = null;
                results.GetItemAt(i, out item);
                items.Add(item);
            }

            foreach (NativeInterfaces.IShellItem item in items)
            {
                // If it's a file system object, nothing special needs to be done.
                NativeConstants.SFGAO sfgaoAttribs;
                item.GetAttributes((NativeConstants.SFGAO)0xffffffff, out sfgaoAttribs);

                if ((sfgaoAttribs & NativeConstants.SFGAO.SFGAO_FILESYSTEM) == NativeConstants.SFGAO.SFGAO_FILESYSTEM)
                {
                    string pathName = null;
                    item.GetDisplayName(NativeConstants.SIGDN.SIGDN_FILESYSPATH, out pathName);

                    localPathNames.Add(pathName);
                }
                else if ((sfgaoAttribs & NativeConstants.SFGAO.SFGAO_STREAM) == NativeConstants.SFGAO.SFGAO_STREAM)
                {
                    needLocalCopy.Add(item);
                }
                else
                {
                    cannotCopy.Add(item);
                }
            }

            Marshal.ReleaseComObject(results);
            results = null;

            if (needLocalCopy.Count > 0)
            {
                IntPtr hwnd = IntPtr.Zero;
                NativeInterfaces.IOleWindow oleWindow = (NativeInterfaces.IOleWindow)pfd;
                oleWindow.GetWindow(out hwnd);
                Win32Window win32Window = new Win32Window(hwnd, oleWindow);

                IFileTransferProgressEvents progressEvents = this.FileDialogUICallbacks.CreateFileTransferProgressEvents();

                ThreadStart copyThreadProc =
                    delegate()
                    {
                        try
                        {
                            progressEvents.SetItemCount(needLocalCopy.Count);

                            for (int i = 0; i < needLocalCopy.Count; ++i)
                            {
                                NativeInterfaces.IShellItem item = needLocalCopy[i];

                                string pathName = null;

                                progressEvents.SetItemOrdinal(i);
                                CopyResult result = CreateLocalCopy(item, progressEvents, out pathName);

                                if (result == CopyResult.Success)
                                {
                                    localPathNames.Add(pathName);
                                }
                                else if (result == CopyResult.Skipped)
                                {
                                    // do nothing
                                }
                                else if (result == CopyResult.CancelOperation)
                                {
                                    hr = NativeConstants.S_FALSE;
                                    break;
                                }
                                else
                                {
                                    throw new InvalidEnumArgumentException();
                                }
                            }
                        }

                        finally
                        {
                            OperationResult result;

                            if (hr == NativeConstants.S_OK)
                            {
                                result = OperationResult.Finished;
                            }
                            else
                            {
                                result = OperationResult.Canceled;
                            }

                            progressEvents.EndOperation(result);
                        }
                    };

                Thread copyThread = new Thread(copyThreadProc);
                copyThread.SetApartmentState(ApartmentState.STA);

                EventHandler onUIShown =
                    delegate(object sender, EventArgs e)
                    {
                        copyThread.Start();
                    };

                this.cancelSink = new CancelableTearOff();
                progressEvents.BeginOperation(win32Window, onUIShown, cancelSink);
                this.cancelSink = null;
                copyThread.Join();

                Marshal.ReleaseComObject(oleWindow);
                oleWindow = null;
            }

            this.FileNames = localPathNames.ToArray();

            // If they selected a bunch of files, and then they all errored or something, then don't proceed.
            if (this.FileNames.Length == 0)
            {
                hr = NativeConstants.S_FALSE;
            }

            foreach (NativeInterfaces.IShellItem item in items)
            {
                Marshal.ReleaseComObject(item);
            }

            items.Clear();
            items = null;

            GC.KeepAlive(pfd);
            return hr;
        }

        private enum CopyResult
        {
            Success,
            Skipped,
            CancelOperation
        }

        // Returns true if the item copied successfully, false if it didn't (error or skipped)
        private CopyResult CreateLocalCopy(
            NativeInterfaces.IShellItem item,
            IFileTransferProgressEvents progressEvents,
            out string pathNameResult)
        {
            CopyResult returnResult;
            WorkItemResult itemResult;

            string displayName = null;
            item.GetDisplayName(NativeConstants.SIGDN.SIGDN_NORMALDISPLAY, out displayName);
            progressEvents.SetItemInfo(displayName);

            progressEvents.BeginItem();

            while (true)
            {
                // Determine whether to copy from HTTP or from IStream. The heuristic we use here is simple:
                // if the attributes has SFGAO_CANCOPY, we IStream it. Else, we HTTP it.
                NativeConstants.SFGAO attribs;
                item.GetAttributes((NativeConstants.SFGAO)0xfffffff, out attribs);

                try
                {
                    if ((attribs & NativeConstants.SFGAO.SFGAO_CANCOPY) == NativeConstants.SFGAO.SFGAO_CANCOPY)
                    {
                        CreateLocalCopyFromIStreamSource(item, progressEvents, out pathNameResult);
                    }
                    else
                    {
                        CreateLocalCopyFromHttpSource(item, progressEvents, out pathNameResult);
                    }

                    returnResult = CopyResult.Success;
                    itemResult = WorkItemResult.Finished;
                    break;
                }

                catch (OperationCanceledException)
                {
                    returnResult = CopyResult.CancelOperation;
                    itemResult = WorkItemResult.Skipped;
                    pathNameResult = null;
                    break;
                }

                catch (Exception ex)
                {
                    WorkItemFailureAction choice = progressEvents.ReportItemFailure(ex);

                    if (choice == WorkItemFailureAction.SkipItem)
                    {
                        pathNameResult = null;
                        returnResult = CopyResult.Skipped;
                        itemResult = WorkItemResult.Skipped;
                        break;
                    }
                    else if (choice == WorkItemFailureAction.RetryItem)
                    {
                        continue;
                    }
                    else if (choice == WorkItemFailureAction.CancelOperation)
                    {
                        pathNameResult = null;
                        returnResult = CopyResult.CancelOperation;
                        itemResult = WorkItemResult.Skipped;
                        break;
                    }
                }
            }

            progressEvents.EndItem(itemResult);

            return returnResult;
        }

        private void CreateLocalCopyFromHttpSource(
            NativeInterfaces.IShellItem item,
            IFileTransferProgressEvents progressEvents,
            out string pathNameResult)
        {
            string url = null;
            item.GetDisplayName(NativeConstants.SIGDN.SIGDN_URL, out url);

            Uri uri = new Uri(url);

            string pathName = FileSystem.GetTempPathName(url);

            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Timeout = 5000;

            using (WebResponse webResponse = webRequest.GetResponse())
            {
                VerifyNotCanceled();

                using (Stream uriStream = webResponse.GetResponseStream())
                {
                    VerifyNotCanceled();

                    using (FileStream outStream = new FileStream(pathName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        VerifyNotCanceled();

                        const int bufSize = 512;
                        long length = webResponse.ContentLength;
                        long bytesLeft = length;
                        byte[] buffer = new byte[bufSize];

                        progressEvents.SetItemWorkTotal(length);

                        while (bytesLeft > 0)
                        {
                            int amtRead = uriStream.Read(buffer, 0, buffer.Length);
                            VerifyNotCanceled();

                            outStream.Write(buffer, 0, amtRead);
                            VerifyNotCanceled();

                            bytesLeft -= amtRead;

                            progressEvents.SetItemWorkProgress(length - bytesLeft);
                            VerifyNotCanceled();

                        }
                    }
                }
            }

            pathNameResult = pathName;
        }

        private unsafe void CreateLocalCopyFromIStreamSource(
            NativeInterfaces.IShellItem item,
            IFileTransferProgressEvents progressEvents,
            out string pathNameResult)
        {
            string fileName = null;
            item.GetDisplayName(NativeConstants.SIGDN.SIGDN_NORMALDISPLAY, out fileName);

            string pathName = FileSystem.GetTempPathName(fileName);

            Guid bhidStream = NativeConstants.BHID_Stream;
            Guid iid_IStream = new Guid(NativeConstants.IID_IStream);
            NativeInterfaces.IStream iStream = null;
            item.BindToHandler(IntPtr.Zero, ref bhidStream, ref iid_IStream, out iStream);

            try
            {
                VerifyNotCanceled();

                NativeStructs.STATSTG statstg = new NativeStructs.STATSTG();
                iStream.Stat(out statstg, NativeConstants.STATFLAG.STATFLAG_NONAME);

                progressEvents.SetItemWorkTotal((long)statstg.cbSize);

                const int bufSize = 4096;
                byte[] buffer = new byte[bufSize];

                fixed (void* pbBuffer = buffer)
                {
                    IntPtr pbBuffer2 = new IntPtr(pbBuffer);

                    ulong qwBytesLeft = statstg.cbSize;

                    using (FileStream localFile = new FileStream(pathName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        VerifyNotCanceled();

                        // NOTE: We do not call VerifyNotCanceled() during any individual item. This is because, while testing
                        //       this, it was determined that oftentimes the transfer gets very confused if we just stop
                        //       calling Read() and jump straight to Marshal.ReleaseComObject(). By "confused" I mean that
                        //       the "Canceling..." text would remain on the progress dialog for up to a minute, and then
                        //       the blinking light on the camera would continue indefinitely and you wouldn't be able to
                        //       use the camera again until you unplugged it and plugged it back in.
                        while (qwBytesLeft > 0)
                        {
                            uint wantToRead = (uint)Math.Min(qwBytesLeft, bufSize);
                            uint amtRead = 0;

                            iStream.Read(pbBuffer2, wantToRead, out amtRead);

                            if (amtRead > qwBytesLeft)
                            {
                                throw new InvalidOperationException("IStream::Read() reported that more bytes were read than were in the file");
                            }

                            qwBytesLeft -= amtRead;
                            localFile.Write(buffer, 0, (int)amtRead);
                            progressEvents.SetItemWorkProgress((long)(statstg.cbSize - qwBytesLeft));
                        }
                    }

                    VerifyNotCanceled();
                }
            }

            finally
            {
                if (iStream != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(iStream);
                    }

                    catch (Exception)
                    {
                    }

                    iStream = null;
                }
            }

            pathNameResult = pathName;
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

        /// <summary>
        /// If the operation has been canceled, then this throws OperationCanceledException.
        /// </summary>
        /// <remarks>
        /// The general guideline for when to call this method is: (1) right after calling any
        /// method that may take awhile to complete, such as Read() or Write(), and (2) right
        /// after calling ReportItemProgress().
        /// </remarks>
        private void VerifyNotCanceled()
        {
            if (this.cancelSink != null && this.cancelSink.Canceled)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
